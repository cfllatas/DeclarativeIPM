// STAGE 1 of the study: ranking of the models on ONE log, with the FIRST phrasing of every question, in the language of the log.
//   DeclarativeIpmEval.exe --stage1 <bank.json> <log.jsonl> <__ModelInfo> [--models a,b] [--limit N] [--ctx 16384] [--questions id,id] [--resume <dir>] [--models-file f.json]
// What it does for every (model, question): builds the REAL system prompt of the agent for this log (real ModelInfo, flow patterns mined from the log, no runner state),
// calls the model ONCE (no retries, no history), runs the REAL validation of the agent, executes what the model returned with the REAL Declare engine (log loaded without
// @Start/@End) and compares the result with the known answer of the bank. Results go to results\stage1-<log>-<stamp>\ (prompt.txt, calls.jsonl, report.md).
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using pm4h.data;
using PMAppSVELTA.declare;
using PMAppSVELTA.DeclarativeIPM;
using PMAppSVELTA.src.chat.chains.ingestionchain.typeinferrer;

namespace DeclarativeIpmEval;

public static class Stage1
{
    sealed class Q { public string Id, Kind, Text; public List<string> Texts; public JsonNode Node; }
    static int StageNo = 1; static int[] Phr = { 0 };   // --stage N, --phrasings 0,1,2

    static readonly JsonSerializerOptions Pretty = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    static List<(string name, string expr)> Lines(string text)
    {
        var res = new List<(string, string)>();
        if (string.IsNullOrWhiteSpace(text)) return res;
        foreach (var l in text.Replace("\\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            int i = l.IndexOf('=');
            if (i > 0) res.Add((l[..i].Trim(), l[(i + 1)..].Trim()));
        }
        return res;
    }

    /// <summary>Partition of the cases by ordered lines (first match; "." = what is left). Canonical form ignores the names. Null when an expression fails.</summary>
    static string Partition(PMLog log, List<string> allIds, List<(string name, string expr)> lines)
    {
        var left = new HashSet<string>(allIds); var hashes = new List<string>();
        foreach (var (_, expr) in lines)
        {
            var kept = expr.Trim() == "." ? new HashSet<string>(left) : BankVerify.Kept(log, expr, out _);
            if (kept == null) return null;
            var mine = left.Where(kept.Contains).ToList();
            foreach (var id in mine) left.Remove(id);
            if (mine.Count > 0) hashes.Add(BankVerify.Hash(mine));
        }
        if (left.Count > 0) hashes.Add(BankVerify.Hash(left));
        return string.Join("|", hashes.OrderBy(x => x, StringComparer.Ordinal));
    }

    static string RegionSet(PMLog log, List<(string name, string expr)> lines)
    {
        var hs = new List<string>();
        foreach (var (_, expr) in lines)
        {
            var kept = BankVerify.Kept(log, expr, out _);
            if (kept == null) return null;
            hs.Add(BankVerify.Hash(kept));
        }
        return string.Join("|", hs.OrderBy(x => x, StringComparer.Ordinal));
    }

    public static async Task Run(string bankPath, string logPath, string modelInfoPath, string[] args)
    {
        string Opt(string n, string def = null) { int i = Array.IndexOf(args, n); return i >= 0 && i + 1 < args.Length ? args[i + 1] : def; }
        var bank = JsonNode.Parse(File.ReadAllText(bankPath));
        string logName = bank["log"].ToString();
        int numCtx = int.Parse(Opt("--ctx", "16384")); int limit = int.Parse(Opt("--limit", "0"));
        var modelsFile = Opt("--models-file", Path.Combine(SourceDir(), "models-stage1.json"));
        var models = JsonSerializer.Deserialize<List<ModelSpec2>>(File.ReadAllText(modelsFile), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (Opt("--models") is { } mf) { var set = mf.Split(',').ToHashSet(); models = models.Where(m => set.Contains(m.Id)).ToList(); }
        var only = Opt("--questions")?.Split(',').ToHashSet();
        StageNo = int.Parse(Opt("--stage", "1")); Phr = Opt("--phrasings", "0").Split(',').Select(int.Parse).ToArray();

        // ---- log, model info, patterns, prompt (the same pieces the application gives the agent)
        var log = BankVerify.LoadLog(logPath);
        var allIds = log.IterateTraces().Select(t => t.SampleId).ToList();
        var mi = JsonSerializer.Deserialize<ModelInfoDto>(File.ReadAllText(modelInfoPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var patterns = DeclareProcessPatternMiner.Mine(log);
        var activities = patterns.Activities.ToList();
        var system = DeclarativeIPMPrompt.Build(mi, patterns, new DeclarativeIPMState());
        if (args.Contains("--prompt-size")) { Console.WriteLine($"PROMPT_SIZE log={logName} activities={activities.Count} traceVariables={mi.Tracedata.Count} cases={allIds.Count} chars={system.Length}"); return; }

        // ---- questions: first phrasing of each (bank "text" = texts[0])
        var qs = new List<Q>();
        foreach (var (key, kind) in new[] { ("filters", "filter"), ("circuits", "circuits"), ("regions", "regions"), ("milestones", "milestones"), ("asks", "clarification") })
            foreach (var n in (JsonArray)bank[key]) qs.Add(new Q { Id = n["id"].ToString(), Kind = kind, Text = n["text"].ToString(), Texts = (n["texts"] as JsonArray)?.Select(t => t.ToString()).ToList() ?? new List<string> { n["text"].ToString() }, Node = n });
        if (only != null) qs = qs.Where(q => only.Contains(q.Id)).ToList();
        if (limit > 0) qs = qs.Take(limit).ToList();

        // ---- known answers executed on the engine once (the bank already matches the engine; this gives the canonical forms for circuits and regions)
        var gold = new Dictionary<string, string>();
        foreach (var q in qs)
        {
            if (q.Kind == "circuits") gold[q.Id] = Partition(log, allIds, ((JsonArray)q.Node["circuits"]).Select(l => (l["name"].ToString(), l["expr"].ToString())).ToList());
            else if (q.Kind == "regions") gold[q.Id] = RegionSet(log, ((JsonArray)q.Node["regions"]).Select(l => (l["name"].ToString(), l["expr"].ToString())).ToList());
        }

        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");   // seconds: two runs of the same log started in the same minute used to collide
        var outDir = Opt("--resume") ?? Path.Combine(ResultsRoot.Get(), Path.Combine("stage" + StageNo, logName, stamp));
        if (Opt("--resume") == null && Directory.Exists(outDir) && Directory.EnumerateFileSystemEntries(outDir).Any())
        { Console.Error.WriteLine($"ABORT: the results folder already exists and is not a --resume: {outDir}"); Environment.Exit(3); }
        Directory.CreateDirectory(outDir);
        File.WriteAllText(Path.Combine(outDir, "prompt.txt"), system, new UTF8Encoding(false));
        var callsPath = Path.Combine(outDir, "calls.jsonl");
        WriteRunMeta(outDir, stamp, args, bankPath, logPath, modelInfoPath, modelsFile, system, models, numCtx, allIds.Count, qs.Count, logName, bank["lang"]?.ToString());
        var done = new HashSet<string>();
        if (File.Exists(callsPath))
            foreach (var l in File.ReadLines(callsPath)) { var j = JsonNode.Parse(l); done.Add(j["model"] + "|" + j["question"] + "|" + (j["phrasing"]?.ToString() ?? "0")); }
        using var calls = new StreamWriter(callsPath, true, new UTF8Encoding(false));
        using var caller = new ModelCaller { NumCtx = numCtx };
        AddOllamaInfo(outDir, caller, models);

        Console.WriteLine($"{logName}: {allIds.Count} cases, {qs.Count} questions, {models.Count} model(s), num_ctx {numCtx}; results in {outDir}");

        // WARM-UP of the agent's validator and of the Declare engine: their first use pays a start-up cost (JIT, parser tables) that must not count in the timings
        {
            var a0 = activities.First(); var a1 = activities.Last();
            DeclarativeIPMAgent.Validate(new JsonObject { ["kind"] = "filter", ["name"] = "w", ["description"] = "w", ["expression"] = $"('{a0}' >> '{a1}')" }, activities, mi);
            BankVerify.Kept(log, $"'{a0}'", out _);
            BankVerify.Kept(log, $"('{a0}' >> '{a1}')", out _);
        }

        foreach (var m in models)
        {
            int consecutiveErrors = 0;
            // WARM-UP call of the model: loads it into the GPU and fills the cache of the prompt. It is recorded (warmup = true) but excluded from the report and the statistics.
            if (!done.Any(d => d.StartsWith(m.Id + "|")))
            {
                var w = await caller.CallAsync(m, system, qs[0].Text);
                if (m.Backend == "ollama" && w.PromptTokens.HasValue && w.PromptTokens.Value >= numCtx - 256)   // Ollama truncates the prompt silently when it does not fit: refuse to measure
                {
                    Console.WriteLine($"  CONTEXT TOO SMALL: the prompt has {w.PromptTokens} tokens and num_ctx is {numCtx}; {m.Id} is skipped for this log");
                    calls.WriteLine(JsonSerializer.Serialize(new { model = m.Id, backend = m.Backend, question = qs[0].Id, verdict = "CONTEXT_TOO_SMALL", warmup = true, stage = StageNo, log = logName, promptTokens = w.PromptTokens, numCtx }, Pretty)); calls.Flush();
                    continue;
                }
                calls.WriteLine(JsonSerializer.Serialize(new
                {
                    model = m.Id, backend = m.Backend, modelId = m.Model, question = qs[0].Id, expectedKind = qs[0].Kind, text = qs[0].Text, verdict = "WARMUP", warmup = true,
                    stage = StageNo, log = logName, phrasing = 0, runId = stamp, seconds = Math.Round(w.Seconds, 3), llmSeconds = Math.Round(w.Seconds, 3),
                    promptTokens = w.PromptTokens, outputTokens = w.OutputTokens, loadSeconds = w.LoadSeconds, promptEvalSeconds = w.PromptEvalSeconds, evalSeconds = w.EvalSeconds, serverSeconds = w.ServerSeconds,
                    numCtx, thinkApplied = w.ThinkApplied, cases = allIds.Count, error = w.Error
                }, Pretty));
                calls.Flush();
                Console.WriteLine($"[WARMUP    ] {m.Id} ({w.Seconds:0.0}s, load {w.LoadSeconds:0.0}s){(w.Error != null ? " " + w.Error : "")}");
            }
            foreach (var (ph, q) in Phr.SelectMany(ph => qs.Select(q => (ph, q))))
            {
                if (done.Contains(m.Id + "|" + q.Id + "|" + ph)) continue;
                string qtext = q.Texts[Math.Min(ph, q.Texts.Count - 1)];
                if (m.Backend == "poligpt" && consecutiveErrors >= 3) { Console.WriteLine($"  {m.Id}: 3 errors in a row, the rest of this model is skipped"); break; }
                var res = await caller.CallAsync(m, system, qtext);
                string verdict, kind = null, detail = null; List<string> problems = null; int shown = 0; double valSec = 0, engSec = 0;
                if (res.Error != null) { verdict = "ERROR"; detail = res.Error; consecutiveErrors++; }
                else
                {
                    consecutiveErrors = 0;
                    var json = DeclarativeIPMJson.Extract(res.Text ?? "");
                    if (json == null) { verdict = "NO_JSON"; detail = res.Text; }
                    else if (json["clarification"] != null) { kind = "clarification"; verdict = q.Kind == "clarification" ? "CORRECT" : "ASKED"; }
                    else
                    {
                        kind = json["kind"]?.ToString();
                        var swv = System.Diagnostics.Stopwatch.StartNew();
                        problems = DeclarativeIPMAgent.Validate(json, activities, mi);
                        valSec = swv.Elapsed.TotalSeconds;
                        if (q.Kind == "clarification") verdict = problems.Count > 0 ? "INVALID" : "ACTED";
                        else if (problems.Count > 0) verdict = "INVALID";
                        else if (kind != q.Kind) verdict = "WRONG_KIND";
                        else
                        {
                            var swe = System.Diagnostics.Stopwatch.StartNew();
                            verdict = Score(q, json, log, allIds, gold, out detail);
                            engSec = swe.Elapsed.TotalSeconds;
                        }
                    }
                    detail ??= json?.ToJsonString(Pretty);
                }
                calls.WriteLine(JsonSerializer.Serialize(new
                {
                    model = m.Id, backend = m.Backend, modelId = m.Model, question = q.Id, expectedKind = q.Kind, text = qtext, verdict, kind,
                    stage = StageNo, log = logName, phrasing = ph, runId = stamp, warmup = false, cases = allIds.Count,
                    llmSeconds = Math.Round(res.Seconds, 3), validationSeconds = Math.Round(valSec, 4), engineSeconds = Math.Round(engSec, 4), totalSeconds = Math.Round(res.Seconds + valSec + engSec, 3),
                    seconds = Math.Round(res.Seconds, 3), throttleSeconds = Math.Round(res.ThrottleSeconds, 3), promptTokens = res.PromptTokens, outputTokens = res.OutputTokens,
                    loadSeconds = res.LoadSeconds, promptEvalSeconds = res.PromptEvalSeconds, evalSeconds = res.EvalSeconds, serverSeconds = res.ServerSeconds,
                    numCtx, thinkApplied = res.ThinkApplied,
                    problems, detail, thinking = string.IsNullOrEmpty(res.Thinking) ? null : (res.Thinking.Length > 1500 ? res.Thinking[..1500] : res.Thinking), raw = res.Text
                }, Pretty));
                calls.Flush();
                Console.WriteLine($"[{verdict,-10}] {m.Id} {q.Id} ({res.Seconds:0.0}s, prompt {res.PromptTokens}, out {res.OutputTokens}) {kind}");
                shown++;
            }
        }
        WriteReport(outDir, logName, numCtx);
        Console.WriteLine("report: " + Path.Combine(outDir, "report.md"));
    }

    static string Score(Q q, JsonObject json, PMLog log, List<string> allIds, Dictionary<string, string> gold, out string detail)
    {
        detail = null;
        switch (q.Kind)
        {
            case "filter":
                {
                    var expr = json["expression"]?.ToString();
                    var kept = BankVerify.Kept(log, expr, out var err);
                    if (kept == null) { detail = "engine: " + err; return "ENGINE_ERROR"; }
                    detail = $"{expr}  ->  {kept.Count} cases";
                    return kept.Count == q.Node["count"].GetValue<int>() && BankVerify.Hash(kept) == q.Node["case_hash"].ToString() ? "CORRECT" : "WRONG";
                }
            case "circuits":
                {
                    var lines = Lines(json["circuits"]?.ToString());
                    var p = Partition(log, allIds, lines);
                    detail = string.Join(" ; ", lines.Select(l => l.name + " = " + l.expr));
                    if (p == null) return "ENGINE_ERROR";
                    return p == gold[q.Id] ? "CORRECT" : "WRONG";
                }
            case "regions":
                {
                    var lines = Lines(json["patterns"]?.ToString());
                    var p = RegionSet(log, lines);
                    detail = string.Join(" ; ", lines.Select(l => l.name + " = " + l.expr));
                    if (p == null) return "ENGINE_ERROR";
                    return p == gold[q.Id] ? "CORRECT" : "WRONG";
                }
            case "milestones":
                {
                    var got = ((JsonArray)json["nodes"]).Select(n => n.ToString()).OrderBy(x => x, StringComparer.Ordinal).ToList();
                    var want = ((JsonArray)q.Node["nodes"]).Select(n => n.ToString()).OrderBy(x => x, StringComparer.Ordinal).ToList();
                    detail = string.Join(", ", got);
                    return got.SequenceEqual(want) ? "CORRECT" : "WRONG";
                }
        }
        return "WRONG";
    }


    static string Sha(string path) { using var fs = File.OpenRead(path); return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(fs)).ToLowerInvariant(); }
    static string Sha(byte[] b) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(b)).ToLowerInvariant();

    static string Git(string dir, string a)
    {
        try
        {
            if (!Directory.Exists(dir)) return null;
            var psi = new System.Diagnostics.ProcessStartInfo("git", $"-C \"{dir}\" {a}") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
            using var pr = System.Diagnostics.Process.Start(psi);
            var o = pr.StandardOutput.ReadToEnd().Trim(); pr.WaitForExit(5000);
            return pr.ExitCode == 0 ? o : null;
        }
        catch { return null; }
    }

    static readonly JsonSerializerOptions Indented = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>run.json: everything needed to reproduce and to analyse the run later (settings, file hashes, versions, commits).</summary>
    static void WriteRunMeta(string outDir, string stamp, string[] args, string bankPath, string logPath, string modelInfoPath, string modelsFile, string system, List<ModelSpec2> models, int numCtx, int cases, int questions, string logName, string lang)
    {
        var path = Path.Combine(outDir, "run.json");
        if (File.Exists(path))
        {
            var old = JsonNode.Parse(File.ReadAllText(path));
            var rs = old["resumedAt"] as JsonArray ?? new JsonArray(); rs.Add(JsonValue.Create(DateTime.Now.ToString("o"))); old["resumedAt"] = rs;
            File.WriteAllText(path, old.ToJsonString(Indented), new UTF8Encoding(false));
            return;
        }
        var svelta = Path.GetFullPath(Path.Combine(SourceDir(), "..", ".."));
        const string engine = @"<folder of the Declare engine repository (pm4hResearch)>";
        var meta = new JsonObject
        {
            ["stage"] = StageNo, ["log"] = logName, ["language"] = lang, ["runId"] = stamp, ["startedAt"] = DateTime.Now.ToString("o"),
            ["machine"] = Environment.MachineName, ["command"] = string.Join(" ", args),
            ["settings"] = new JsonObject { ["temperature"] = 0, ["seed"] = 1, ["numCtx"] = numCtx, ["numPredict"] = 4096, ["phrasing"] = "phrasings " + string.Join(",", Phr) + " (0 = first), in the language of the log", ["retries"] = 0, ["poligptMinIntervalSeconds"] = 15, ["warmup"] = "one call per model with the first question (warmup=true, excluded from statistics) + one validation and two engine executions before measuring" },
            ["cases"] = cases, ["questions"] = questions,
            ["files"] = new JsonObject
            {
                ["bank"] = new JsonObject { ["path"] = bankPath, ["sha256"] = Sha(bankPath) }, ["log"] = new JsonObject { ["path"] = logPath, ["sha256"] = Sha(logPath) },
                ["modelInfo"] = new JsonObject { ["path"] = modelInfoPath, ["sha256"] = Sha(modelInfoPath) }, ["models"] = new JsonObject { ["path"] = modelsFile, ["sha256"] = Sha(modelsFile) }
            },
            ["prompt"] = new JsonObject { ["sha256"] = Sha(Encoding.UTF8.GetBytes(system)), ["characters"] = system.Length },
            ["models"] = JsonNode.Parse(JsonSerializer.Serialize(models)),
            ["git"] = new JsonObject
            {
                ["svelta"] = new JsonObject { ["commit"] = Git(svelta, "rev-parse HEAD"), ["dirty"] = !string.IsNullOrEmpty(Git(svelta, "status --porcelain")) },
                ["engine_pm4hResearch"] = new JsonObject { ["commit"] = Git(engine, "rev-parse HEAD"), ["dirty"] = !string.IsNullOrEmpty(Git(engine, "status --porcelain")) }
            },
            ["notes"] = "The Declare engine is compiled from source: the engine commit above plus the uncommitted fixes of 2026-11-07 (wildcard, numeric ==) define the semantics used to execute the answers."
        };
        File.WriteAllText(path, meta.ToJsonString(Indented), new UTF8Encoding(false));
    }

    static void AddOllamaInfo(string outDir, ModelCaller caller, List<ModelSpec2> models)
    {
        try
        {
            var path = Path.Combine(outDir, "run.json");
            var meta = JsonNode.Parse(File.ReadAllText(path));
            if (meta["ollama"] != null) return;
            meta["ollama"] = caller.OllamaInfoAsync(models.Where(m => m.Backend == "ollama").Select(m => m.Model)).GetAwaiter().GetResult();
            File.WriteAllText(path, meta.ToJsonString(Indented), new UTF8Encoding(false));
        }
        catch { }
    }

    static void WriteCsv(string dir, List<JsonNode> rows)
    {
        string C(JsonNode n) { var t = n?.ToString() ?? ""; return t.Contains(',') || t.Contains('"') || t.Contains('\n') ? "\"" + t.Replace("\"", "\"\"") + "\"" : t; }
        var cols = new[] { "runId", "stage", "log", "model", "backend", "modelId", "question", "expectedKind", "phrasing", "warmup", "cases", "verdict", "kind", "seconds", "llmSeconds", "validationSeconds", "engineSeconds", "totalSeconds", "throttleSeconds", "serverSeconds", "loadSeconds", "promptEvalSeconds", "evalSeconds", "promptTokens", "outputTokens", "numCtx", "thinkApplied" };
        var sb = new StringBuilder(string.Join(",", cols) + ",problemCount\n");
        foreach (var r in rows)
            sb.Append(string.Join(",", cols.Select(c => C(r[c])))).Append(',').Append((r["problems"] as JsonArray)?.Count ?? 0).Append('\n');
        File.WriteAllText(Path.Combine(dir, "calls.csv"), sb.ToString(), new UTF8Encoding(false));
    }

    static void WriteReport(string dir, string logName, int numCtx)
    {
        var allRows = new List<JsonNode>();
        using (var fs = new FileStream(Path.Combine(dir, "calls.jsonl"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var rd = new StreamReader(fs, Encoding.UTF8))
        {
            string ln;
            while ((ln = rd.ReadLine()) != null) if (ln.Length > 0) allRows.Add(JsonNode.Parse(ln));
        }
        var rows = allRows.Where(r => !(r["warmup"]?.GetValue<bool>() ?? false)).ToList();   // warm-up calls are kept in the files but are not statistics
        var md = new StringBuilder();
        md.AppendLine($"# Stage {StageNo} — ranking of the models on `{logName}` ({DateTime.Now:yyyy-MM-dd HH:mm})\n");
        md.AppendLine($"Phrasing(s) {string.Join(", ", Phr)} (0 = the first) of every question, in the language of the log. One call per question and model, no retries, temperature 0, seed 1, num_ctx {numCtx} (local models), thinking off where the model allows it. The prompt is the real prompt of the agent (`prompt.txt`). Every model gets one **warm-up call** (the first question, recorded with `warmup: true` and excluded from this report) and the validator and the engine are warmed up once before measuring. Times: LLM = the call as the client sees it; engine = executing the answer with Declare and comparing it; total = LLM + validation + engine.\n");
        md.AppendLine("Verdicts: `CORRECT` = it does what the bank says (executed on the real engine and compared with the known answer; for ask questions, it asked); `WRONG` = valid and executed but another result; `WRONG_KIND` = another operation; `INVALID` = the agent's validation rejects it (nothing would be applied); `ENGINE_ERROR` = valid for the validation but the engine fails; `ASKED` = asked when it should act; `ACTED` = acted when it should ask; `NO_JSON`, `ERROR` = no usable answer.\n");
        string K(JsonNode r) => r["expectedKind"].ToString();
        md.AppendLine("## Ranking\n");
        md.AppendLine("| # | Model | Backend | Questions | CORRECT | % correct | valid % | filters % | circuits % | regions % | milestones % | ask % | mean LLM s | mean engine s | mean total s | out tokens/s |");
        md.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");
        var groups = rows.GroupBy(r => r["model"].ToString()).Select(g =>
        {
            int n = g.Count(), ok = g.Count(r => r["verdict"].ToString() == "CORRECT");
            string P(string k) { var x = g.Where(r => K(r) == k).ToList(); return x.Count == 0 ? "–" : (100.0 * x.Count(r => r["verdict"].ToString() == "CORRECT") / x.Count).ToString("0", CultureInfo.InvariantCulture); }
            int valid = g.Count(r => r["verdict"].ToString() is not ("INVALID" or "NO_JSON" or "ERROR"));
            var secs = g.Select(r => r["seconds"].GetValue<double>()).ToList();
            var tps = g.Where(r => r["outputTokens"] != null && r["seconds"].GetValue<double>() > 0).Select(r => r["outputTokens"].GetValue<int>() / r["seconds"].GetValue<double>()).ToList();
            return (Model: g.Key, Backend: g.First()["backend"].ToString(), n, ok, pct: 100.0 * ok / n, line: $"| {g.Key} | {g.First()["backend"]} | {n} | {ok} | {100.0 * ok / n:0.0} | {100.0 * valid / n:0} | {P("filter")} | {P("circuits")} | {P("regions")} | {P("milestones")} | {P("clarification")} | {secs.Average():0.0} | {g.Average(r => r["engineSeconds"]?.GetValue<double>() ?? 0):0.000} | {g.Average(r => r["totalSeconds"]?.GetValue<double>() ?? r["seconds"].GetValue<double>()):0.0} | {(tps.Count == 0 ? 0 : tps.Average()):0} |");
        }).OrderByDescending(x => x.pct).ToList();
        int rank = 1; foreach (var g in groups) md.AppendLine($"| {rank++} " + g.line);
        md.AppendLine("\n## What went wrong, per model\n");
        md.AppendLine("| Model | " + string.Join(" | ", new[] { "WRONG", "WRONG_KIND", "INVALID", "ENGINE_ERROR", "ASKED", "ACTED", "NO_JSON", "ERROR" }) + " |");
        md.AppendLine("|---|" + string.Join("|", Enumerable.Repeat("---", 8)) + "|");
        foreach (var g in rows.GroupBy(r => r["model"].ToString()))
            md.AppendLine($"| {g.Key} | " + string.Join(" | ", new[] { "WRONG", "WRONG_KIND", "INVALID", "ENGINE_ERROR", "ASKED", "ACTED", "NO_JSON", "ERROR" }.Select(v => g.Count(r => r["verdict"].ToString() == v))) + " |");
        md.AppendLine("\n## Questions that few models get right\n");
        md.AppendLine("| Question | Kind | models CORRECT / answered | text |");
        md.AppendLine("|---|---|---|---|");
        foreach (var g in rows.GroupBy(r => r["question"].ToString()).OrderBy(g => g.Count(r => r["verdict"].ToString() == "CORRECT") / (double)g.Count()).Take(15))
            md.AppendLine($"| {g.Key} | {K(g.First())} | {g.Count(r => r["verdict"].ToString() == "CORRECT")} / {g.Count()} | {g.First()["text"].ToString().Replace("|", "\\|")} |");
        File.WriteAllText(Path.Combine(dir, "report.md"), md.ToString(), new UTF8Encoding(false));
        WriteCsv(dir, allRows);
    }

    static string SourceDir([System.Runtime.CompilerServices.CallerFilePath] string p = "") => Path.GetDirectoryName(p);
}
