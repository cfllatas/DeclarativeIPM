// Evaluation harness of the DeclarativeIPM agent. Headless (no application). Usage:
//   DeclarativeIpmEval.exe                          every model in models.json x the whole order bank
//   DeclarativeIpmEval.exe --models a,b             only these model labels
//   DeclarativeIpmEval.exe --repeat 5               every order 5 times per model (rates instead of single outcomes)
//   DeclarativeIpmEval.exe F4 M1                    only orders whose id or text contains any of these words
//   DeclarativeIpmEval.exe --list-models poligpt    ids the PoliGPT endpoint offers   (also: ollama)
// Each run writes results\run-<yyyyMMdd-HHmm>\ with calls.jsonl (one line per call: model, order, seconds, tokens, verdict, raw output) and report.md.
// Endpoints and keys live OUTSIDE the repository: %APPDATA%\DeclarativeIPM\ollama.json and poligpt.json.
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using PMAppSVELTA.declare;
using PMAppSVELTA.DeclarativeIPM;
using PMAppSVELTA.src.chat.chains.ingestionchain.typeinferrer;

Console.OutputEncoding = Encoding.UTF8;

// ---------------------------------------------------------------- arguments
var words = new List<string>(); string[] modelFilter = null; int repeat = 1; string listBackend = null; bool probeSemantics = false; string verifyBank = null, verifyLog = null; string stage1Bank = null, stage1Log = null, stage1Info = null;
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--models" && i + 1 < args.Length) modelFilter = args[++i].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    else if (args[i] == "--repeat" && i + 1 < args.Length) repeat = Math.Max(1, int.Parse(args[++i]));
    else if (args[i] == "--list-models" && i + 1 < args.Length) listBackend = args[++i];
    else if (args[i] == "--probe-semantics") probeSemantics = true;
    else if (args[i] == "--verify-bank" && i + 2 < args.Length) { verifyBank = args[++i]; verifyLog = args[++i]; }
    else if (args[i] == "--stage1" && i + 3 < args.Length) { stage1Bank = args[++i]; stage1Log = args[++i]; stage1Info = args[++i]; }
    else words.Add(args[i]);
}

if (stage1Bank != null)
{
    await DeclarativeIpmEval.Stage1.Run(stage1Bank, stage1Log, stage1Info, args);
    return;
}

if (verifyBank != null)
{
    DeclarativeIpmEval.BankVerify.Run(verifyBank, verifyLog);
    return;
}

if (probeSemantics)
{
    Console.WriteLine(DeclarativeIpmEval.Probe.Semantics());
    return;
}

if (listBackend != null)
{
    if (listBackend == "poligpt") foreach (var m in await new DeclarativeIPMPoliGpt().ListModelsAsync()) Console.WriteLine(m);
    else if (listBackend == "ollama")
    {
        var o = new DeclarativeIPMOllama();
        using var c = new PMAppSVELTA.Chat.Ollama.OllamaClient(new PMAppSVELTA.Chat.Ollama.OllamaOptions { BaseUrl = o.Endpoint, ApiKey = o.ApiKey });
        foreach (var m in await c.ListModelsAsync()) Console.WriteLine($"{m.Name}  {m.Size / 1e9:0.0} GB");
    }
    return;
}

// ---------------------------------------------------------------- models
var models = JsonSerializer.Deserialize<List<ModelSpec>>(File.ReadAllText(Path.Combine(SourceDir(), "models.json")), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
if (modelFilter != null) models = models.Where(m => modelFilter.Contains(m.Label)).ToList();
static IDeclarativeIPMLlm Make(ModelSpec m) => m.Backend switch
{
    "ollama" => new DeclarativeIPMOllama { Model = m.Model },
    "poligpt" => new DeclarativeIPMPoliGpt { Model = m.Model },
    _ => throw new ArgumentException("unknown backend " + m.Backend)
};

// ---------------------------------------------------------------- synthetic process (emergency department)
static AttrDto Cat(string id, string desc, params string[] vals) =>
    new AttrDto { Id = id, Name = id, Description = desc, Kind = "Categorical", Values = vals.Select(v => new CategoryDto { Id = v, Name = v }).ToList() };

var mi = new ModelInfoDto();
mi.Properties["description"] = "Hospital emergency department process: the patient arrives, goes through triage, is seen by a doctor, may have tests, and ends with discharge, voluntary discharge, admission or death.";
foreach (var (id, d) in new[]
{
    ("Triage", "Severity classification on arrival"), ("Consultation", "Visit with the doctor"), ("Radiology", "Imaging test"), ("Lab test", "Blood test"),
    ("Observation", "Monitoring in the observation room"), ("Admission", "Hospital admission"), ("Discharge", "Discharge to home"),
    ("Voluntary discharge", "The patient leaves against medical advice"), ("Death", "The patient dies")
}) mi.NodesInfo[id] = new NodeInfoDto { Id = id, Name = id, Description = d };
mi.Tracedata["Age"] = new AttrDto { Id = "Age", Name = "Age", Description = "Patient age in years", Kind = "Numeric", Min = 0, Max = 100 };
mi.Tracedata["Sex"] = Cat("Sex", "Patient sex", "M", "F");
mi.Tracedata["TriageLevel"] = Cat("TriageLevel", "Triage severity level (1 is the most severe)", "1", "2", "3", "4", "5");

var patterns = new ProcessPatterns();
patterns.StartActivities.Add("Triage");
foreach (var e in new[] { "Discharge", "Voluntary discharge", "Admission", "Death" }) patterns.EndActivities.Add(e);
var edges = new (string, string, int)[]
{
    ("Triage","Consultation",900), ("Consultation","Radiology",300), ("Consultation","Lab test",400), ("Consultation","Observation",120), ("Consultation","Discharge",350),
    ("Consultation","Voluntary discharge",40), ("Consultation","Admission",90), ("Radiology","Lab test",100), ("Radiology","Consultation",150), ("Radiology","Discharge",80),
    ("Radiology","Admission",40), ("Lab test","Consultation",200), ("Lab test","Radiology",60), ("Lab test","Observation",50), ("Lab test","Discharge",90), ("Lab test","Admission",45),
    ("Observation","Discharge",100), ("Observation","Admission",60), ("Observation","Death",5), ("Admission","Death",8), ("Admission","Discharge",70)
};
foreach (var (a, b, n) in edges) patterns.DirectFollows[(a, b)] = n;
foreach (var a in mi.NodesInfo.Keys) patterns.Reachable[a] = new HashSet<string>();
foreach (var (a, b, _) in edges) patterns.Reachable[a].Add(b);
for (bool changed = true; changed;)
{
    changed = false;
    foreach (var a in patterns.Reachable.Keys.ToList())
        foreach (var b in patterns.Reachable[a].ToList())
            foreach (var c in patterns.Reachable[b].ToList()) changed |= patterns.Reachable[a].Add(c);
}
var activities = patterns.Activities.ToList();

// ---------------------------------------------------------------- order bank (headless pilot)
// Only what can be measured WITHOUT the application: order -> validated operation. Orders that depend on the real runner (incremental changes over an existing
// definition, removing the last definition) are NOT here: they need the application and belong to the in-app phase.
// Expected = acceptable kinds ("clarification" when asking is the right answer); Note = what a correct answer looks like (judged by a human).
var none = new DeclarativeIPMState();
var bank = new List<Order>
{
    new("F1",  "show me patients older than 65 who had radiology", "filter", "'Radiology' & [Age > 65]"),
    new("F2",  "patients who leave by voluntary discharge within 3 hours of triage", "filter", "('Triage' >> 'Voluntary discharge')[-3h]"),
    new("F3",  "those who end up admitted after having both a lab test and radiology", "filter", "lab test and radiology (any order) before 'Admission'"),
    new("F4",  "women with triage level 1 or 2", "filter", "[Sex == 'F'] and TriageLevel '1' or '2' (QUOTED values, categorical)"),
    new("F5",  "patients who never saw a doctor", "filter", "!'Consultation'"),
    new("F6",  "patients with triage level 3", "filter", "['TriageLevel' == '3'] (quoted value)"),
    new("F7",  "men over 80 who are discharged", "filter", "[Sex == 'M'] & [Age > 80] & 'Discharge'"),
    new("F8",  "patients who take more than 4 hours from triage to discharge", "filter|clarification", "('Triage' >> 'Discharge')[4h-] (asking whether 'Voluntary discharge' counts is acceptable)"),
    new("F9",  "those who die after being in observation", "filter", "('Observation' >> 'Death')"),
    new("F10", "patients who wait a long time", "clarification", "ASK: 'wait' has several readings (total stay, between two activities...)"),
    new("F11", "patients who had a CT scan", "clarification", "ASK: there is no 'CT scan' activity; do not silently map it to 'Radiology'"),
    new("F12", "show me admitted patients over 50", "filter", "'Admission' & [Age > 50]"),
    new("F13", "muéstrame los pacientes mayores de 70 que ingresan", "filter", "'Admission' & [Age > 70]; name/description in Spanish (language of the order)"),
    new("C1",  "separate patients into those who are admitted, those who go home, and the rest", "circuits", "admitted first, then discharged, then 'other = .' last"),
    new("R1",  "I want to analyse separately what happens between the consultation and the admission", "regions", "reg = ('Consultation' >> 'Admission'); no catch-all"),
    new("M1",  "use admission as a milestone to separate before and after", "milestones", "[\"Admission\"] (no quotes)"),
    new("M3",  "mark lab test and radiology as milestones", "milestones", "[\"Lab test\",\"Radiology\"]"),
};

var orders = bank.Where(o => words.Count == 0 || words.Any(a => o.Id.Equals(a, StringComparison.OrdinalIgnoreCase) || o.Text.Contains(a, StringComparison.OrdinalIgnoreCase))).ToList();

// ---------------------------------------------------------------- run
var pretty = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
var outDir = Path.Combine(DeclarativeIpmEval.ResultsRoot.Get(), "run-" + stamp);
Directory.CreateDirectory(outDir);
using var calls = new StreamWriter(Path.Combine(outDir, "calls.jsonl"), false, new UTF8Encoding(false));

Console.WriteLine($"{models.Count} model(s) x {orders.Count} order(s) x {repeat} = {models.Count * orders.Count * repeat} calls\n");
var rows = new List<Row>();
foreach (var spec in models)
{
    var llm = Make(spec);
    foreach (var o in orders)
        for (int r = 1; r <= repeat; r++)
        {
            var system = DeclarativeIPMPrompt.Build(mi, patterns, none);
            var row = new Row { Model = spec, Order = o, Repeat = r };
            try
            {
                var reply = await llm.ChatAsync(system, o.Text);
                row.Seconds = reply.Seconds; row.Tokens = reply.OutputTokens;
                var json = DeclarativeIPMJson.Extract(reply.Text);
                if (json == null) { row.Kind = "(no JSON)"; row.Output = reply.Text; row.Verdict = "FAIL"; }
                else if (json["clarification"] != null) { row.Kind = "clarification"; row.Output = json.ToJsonString(pretty); row.Verdict = o.Expected.Split('|').Contains("clarification") ? "OK" : "UNEXPECTED"; }
                else
                {
                    row.Kind = json["kind"]?.ToString() ?? "(none)";
                    row.Problems = DeclarativeIPMAgent.Validate(json, activities, mi);
                    row.Output = json.ToJsonString(pretty);
                    row.Verdict = row.Problems.Count > 0 ? "INVALID" : o.Expected.Split('|').Contains(row.Kind) ? "OK" : "UNEXPECTED";
                }
            }
            catch (Exception ex) { row.Kind = "(error)"; row.Output = ex.GetType().Name + ": " + ex.Message; row.Verdict = "FAIL"; }
            rows.Add(row);
            calls.WriteLine(JsonSerializer.Serialize(new
            {
                run = stamp, model = spec.Label, backend = spec.Backend, modelId = spec.Model, order = o.Id, text = o.Text, repeat = r,
                seconds = Math.Round(row.Seconds, 2), outputTokens = row.Tokens, kind = row.Kind, verdict = row.Verdict, problems = row.Problems, output = row.Output
            }, pretty));
            calls.Flush();
            Console.WriteLine($"[{row.Verdict,-10}] {spec.Label} {o.Id}#{r} ({row.Seconds:0.0}s) {row.Kind}" + (row.Problems?.Count > 0 ? "  !! " + string.Join(" | ", row.Problems) : ""));
        }
}

// ---------------------------------------------------------------- report
static double Pct(IEnumerable<double> v, double p) { var s = v.OrderBy(x => x).ToList(); if (s.Count == 0) return 0; return s[Math.Min(s.Count - 1, (int)Math.Ceiling(p * s.Count) - 1)]; }
var md = new StringBuilder();
md.AppendLine($"# DeclarativeIPM evaluation run — {DateTime.Now:yyyy-MM-dd HH:mm}");
md.AppendLine();
md.AppendLine($"- {models.Count} model(s) x {orders.Count} order(s) x {repeat} repetition(s) = {rows.Count} calls. One call per order, no retries, no history, default sampling.");
md.AppendLine("- Process: synthetic emergency department (9 activities, 3 trace variables). Only trace-level variables. Only what can be measured without the application.");
md.AppendLine("- Verdict: `OK` = valid (syntax, activities, variable values) AND an acceptable kind; `INVALID` = validation rejected it; `UNEXPECTED` = valid but another kind; `FAIL` = no JSON or error.");
md.AppendLine("- Seconds = wall-clock time of each call (network included). Semantic correctness is not checked automatically here.");
md.AppendLine();
md.AppendLine("## Per model");
md.AppendLine();
md.AppendLine("| Model | Calls | OK | INVALID | UNEXPECTED | FAIL | OK % | mean s | median s | p95 s | tokens/s |");
md.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|");
foreach (var g in rows.GroupBy(r => r.Model.Label))
{
    var secs = g.Select(r => r.Seconds).ToList();
    var tps = g.Where(r => r.Tokens > 0 && r.Seconds > 0).Select(r => r.Tokens.Value / r.Seconds).ToList();
    md.AppendLine($"| {g.Key} | {g.Count()} | {g.Count(r => r.Verdict == "OK")} | {g.Count(r => r.Verdict == "INVALID")} | {g.Count(r => r.Verdict == "UNEXPECTED")} | {g.Count(r => r.Verdict == "FAIL")} | {100.0 * g.Count(r => r.Verdict == "OK") / g.Count():0}% | {secs.Average():0.0} | {Pct(secs, 0.5):0.0} | {Pct(secs, 0.95):0.0} | {(tps.Count > 0 ? tps.Average().ToString("0.0") : "–")} |");
}
md.AppendLine();
md.AppendLine("## Per order (OK count / repetitions)");
md.AppendLine();
md.AppendLine("| Order | Expected | " + string.Join(" | ", models.Select(m => m.Label)) + " |");
md.AppendLine("|---|---|" + string.Join("|", models.Select(_ => "---")) + "|");
foreach (var o in orders)
    md.AppendLine($"| {o.Id} {o.Text.Replace("|", "\\|")} | {o.Expected} | " + string.Join(" | ", models.Select(m => { var rs = rows.Where(r => r.Model == m && r.Order == o).ToList(); return $"{rs.Count(r => r.Verdict == "OK")}/{rs.Count}" + (rs.Any(r => r.Verdict != "OK") ? " (" + string.Join(",", rs.Where(r => r.Verdict != "OK").Select(r => r.Verdict).Distinct()) + ")" : ""); })) + " |");
File.WriteAllText(Path.Combine(outDir, "report.md"), md.ToString(), new UTF8Encoding(false));
Console.WriteLine($"\nreport: {Path.Combine(outDir, "report.md")}");

static string SourceDir([System.Runtime.CompilerServices.CallerFilePath] string file = "") => Path.GetDirectoryName(file) ?? AppContext.BaseDirectory;

record ModelSpec(string Label, string Backend, string Model);
record Order(string Id, string Text, string Expected, string Note);
class Row { public ModelSpec Model; public Order Order; public int Repeat; public string Kind = ""; public string Output = ""; public string Verdict = ""; public double Seconds; public int? Tokens; public List<string> Problems; }
