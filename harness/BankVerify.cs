// Verifies the question bank against the REAL Declare engine: every reference expression of a bank file is executed on the log (loaded WITHOUT the
// artificial @Start/@End events, as the bank assumes) and the set of selected cases is compared with the gold answer computed by plain Python code.
//   DeclarativeIpmEval.exe --verify-bank <bank.json> <log.jsonl>
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using pm4h.data;
using pm4hlab.ext.tpaengine.declaregrammar;
using PMAppSVELTA.DeclarativeIPM;

namespace DeclarativeIpmEval;

public static class BankVerify
{
    /// <summary>Reads the runner's log.jsonl into a PMLog the way the application's reader does, but without adding @Start/@End.</summary>
    public static PMLog LoadLog(string jsonl)
    {
        var log = new PMLog();
        using var sr = new StreamReader(jsonl, new UTF8Encoding(false));
        sr.ReadLine();   // header line (_LogAttrs)
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            if (line.Length == 0) continue;
            var j = JsonNode.Parse(line);
            var t = new PMTrace { SampleId = j["trace_id"].GetValue<string>() };
            if (j["attrs"] is JsonObject attrs)
                foreach (var kv in attrs)
                {
                    if (kv.Value is JsonValue v)
                    {
                        if (v.TryGetValue<double>(out var d)) t[kv.Key] = d;
                        else if (v.TryGetValue<bool>(out var b)) t[kv.Key] = b;
                        else t[kv.Key] = v.ToString();
                    }
                }
            foreach (var e in (JsonObject)j["events"])
            {
                var bs = e.Value["base"];
                t.InsertByStartTime(new PMEvent
                {
                    SampleId = t.SampleId,
                    ActivityName = bs["ActivityName"].GetValue<string>(),
                    Start = DateTime.Parse(bs["start"].GetValue<string>(), CultureInfo.InvariantCulture),
                    End = DateTime.Parse(bs["end"].GetValue<string>(), CultureInfo.InvariantCulture)
                });
            }
            log.addTrace(t);
        }
        return log;
    }

    /// <summary>Ids of the traces the engine keeps for the expression; null when it does not parse or throws.</summary>
    public static HashSet<string> Kept(PMLog log, string expr, out string error)
    {
        error = null;
        try
        {
            var syn = DeclarativeIPMValidation.SyntaxError(expr);
            if (syn != null) { error = "syntax: " + syn; return null; }
            var res = new PMLogDeclareFilter { Expression = expr }.ProcessLog(new IPMLog[] { log });
            return res.First().IterateTraces().Select(t => t.SampleId).ToHashSet();
        }
        catch (Exception ex) { error = ex.GetType().Name + ": " + ex.Message; return null; }
    }

    public static string Hash(IEnumerable<string> ids)
    {
        var s = string.Join("\n", ids.OrderBy(x => x, StringComparer.Ordinal));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).Substring(0, 16).ToLowerInvariant();
    }

    public static void Run(string bankPath, string logPath)
    {
        var bank = JsonNode.Parse(File.ReadAllText(bankPath));
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var log = LoadLog(logPath);
        var allIds = log.IterateTraces().Select(t => t.SampleId).ToList();
        Console.WriteLine($"{bank["log"]}: {allIds.Count} cases loaded in {sw.Elapsed.TotalSeconds:0.0}s");
        int ok = 0, bad = 0;

        foreach (var f in (JsonArray)bank["filters"])
        {
            var kept = Kept(log, f["expr"].GetValue<string>(), out var err);
            int goldCount = f["count"].GetValue<int>();
            string goldHash = f["case_hash"].GetValue<string>();
            if (kept == null) { bad++; Console.WriteLine($"  FAIL {f["id"]}  engine error: {err}  | {f["expr"]}"); continue; }
            if (kept.Count == goldCount && Hash(kept) == goldHash) { ok++; continue; }
            bad++;
            Console.WriteLine($"  FAIL {f["id"]}  engine {kept.Count} vs gold {goldCount}  | {f["expr"]}");
        }

        foreach (var c in (JsonArray)bank["circuits"])
        {
            // a case goes to the first circuit that matches (the application's circuits are ordered)
            var left = new HashSet<string>(allIds); var counts = new Dictionary<string, int>(); bool err = false;
            foreach (var line in (JsonArray)c["circuits"])
            {
                var cexpr = line["expr"].GetValue<string>();
                string e = null;
                var kept = cexpr.Trim() == "." ? new HashSet<string>(allIds) : Kept(log, cexpr, out e);   // "." = every case left (the application's catch-all, not Declare syntax)
                if (kept == null) { Console.WriteLine($"  FAIL {c["id"]}  circuit '{line["name"]}': {e}"); err = true; break; }
                var mine = left.Where(kept.Contains).ToList();
                counts[line["name"].GetValue<string>()] = mine.Count; foreach (var id in mine) left.Remove(id);
            }
            if (left.Count > 0) counts["UNK"] = left.Count;
            var gold = ((JsonObject)c["gold_counts"]).ToDictionary(kv => kv.Key, kv => kv.Value.GetValue<int>());
            var nz = counts.Where(kv => kv.Value > 0).ToDictionary(kv => kv.Key, kv => kv.Value);   // the bank only lists circuits that have cases
            bool same = !err && gold.Count == nz.Count && gold.All(kv => nz.TryGetValue(kv.Key, out var v) && v == kv.Value);
            if (same) ok++; else { bad++; if (!err) Console.WriteLine($"  FAIL {c["id"]}  circuit counts engine {string.Join(",", counts.Select(kv => kv.Key + "=" + kv.Value))} vs gold {string.Join(",", gold.Select(kv => kv.Key + "=" + kv.Value))}"); }
        }

        foreach (var r in (JsonArray)bank["regions"])
            foreach (var line in (JsonArray)r["regions"])
            {
                var kept = Kept(log, line["expr"].GetValue<string>(), out var e);
                int gold = line["traces"].GetValue<int>();
                if (kept != null && kept.Count == gold) ok++;
                else { bad++; Console.WriteLine($"  FAIL {r["id"]}  region '{line["name"]}': engine {(kept == null ? e : kept.Count.ToString())} vs gold {gold}"); }
            }

        Console.WriteLine($"{bank["log"]}: {ok} ok, {bad} fail ({sw.Elapsed.TotalSeconds:0.0}s)");
    }
}
