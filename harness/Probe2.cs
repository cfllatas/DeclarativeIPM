// Second part of the probe: (a) the same letter traces read by the REAL reader (which adds @Start and @End events) compared with hand-made traces,
// and (b) the examples of the grammar documentation (Declare_Gramatica.md) checked against the engine.
using System.IO;
using System.Text;
using pm4h.data;
using DeclarativeIPMNs = PMAppSVELTA.DeclarativeIPM;

namespace DeclarativeIpmEval;

public static class Probe2
{
    static readonly DateTime T0 = new(2024, 1, 1, 8, 0, 0);

    static PMLog Load(List<string> lines)
    {
        var dir = Path.Combine(Path.GetTempPath(), "ipm-probe2-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, "log.jsonl");
        File.WriteAllLines(file, lines, new UTF8Encoding(false));
        var log = new PMApp4Base.src.factories.JSONLLogReader { jsonlfile = pm4h.vio.VIOHelper.CreateIPMAppIO(file) }.getLog().First();
        try { Directory.Delete(dir, true); } catch { }
        return (PMLog)log;
    }

    static string EventsJson(string letters, int step = 10) =>
        string.Join(",", letters.Select((c, i) => "\"" + i.ToString("D8") + "\":{\"base\":{\"ActivityName\":\"" + c + "\",\"start\":\"" + T0.AddMinutes(i * step).ToString("yyyy-MM-ddTHH:mm:ss") + "\",\"end\":\"" + T0.AddMinutes(i * step + 5).ToString("yyyy-MM-ddTHH:mm:ss") + "\"},\"extra\":{}}"));

    public static void Extra(StringBuilder md, string[] letterTraces)
    {
        var ts = letterTraces.Where(x => x != "").ToArray();

        // ---------- (a) hand-made traces vs the same traces read by the real reader
        var lines = new List<string> { "{\"_LogAttrs\":{\"Escenario\":\"probe\",\"_ECSVTraceCount\":" + ts.Length + "}}" };
        foreach (var s in ts) lines.Add("{\"trace_id\":\"" + s + "\",\"attrs\":{},\"events\":{" + EventsJson(s) + "}}");
        var loaded = Load(lines).IterateTraces().ToDictionary(t => t.SampleId, t => t);

        string[] cmp =
        {
            "'A'", "!'A'", "('A' > 'B')", "('A' >> 'B')", "('A' > ? > 'B')", "('A' >> ? >> 'B')", "('A' > ?)", "(? > 'B')",
            "'A' ->* 'B'", "'A' ->+ 'B'", "'A' -> 'B'", "'B' *<- 'A'", "'B' +<- 'A'", "'B' <- 'A'", "'A' =>* 'B'", "'A' =>+ 'B'", "'A' => 'B'",
            "^'A'", "'A'$",
        };
        md.AppendLine("### 6. Hand-made traces vs the same traces read by the real reader\n");
        md.AppendLine("Each cell is one string with one character per trace, in the order `" + string.Join(" ", ts) + "` (`1` kept, `0` discarded, `E` exception). The real reader adds `@Start` and `@End` events to every trace; a `*` marks an expression whose result differs.\n");
        md.AppendLine("| Expression | hand-made | read by the real reader | differs |");
        md.AppendLine("|---|---|---|---|");
        foreach (var e in cmp)
        {
            var a = string.Concat(ts.Select(s => Cell(Probe.Run(e, Probe.Make(s)))));
            var b = string.Concat(ts.Select(s => Cell(Probe.Run(e, loaded[s]))));
            md.AppendLine($"| `{e}` | `{a}` | `{b}` | {(a == b ? "" : "*")} |");
        }
        md.AppendLine();

        // ---------- (b) the documentation's examples
        var l2 = new List<string> { "{\"_LogAttrs\":{\"Escenario\":\"probe\",\"_ECSVTraceCount\":4}}" };
        (string Id, string Attrs, string Seq)[] cases =
        {
            ("t70F", "{\"Age\":70,\"Sex\":\"F\",\"Flag\":\"si\"}", "AB"),
            ("t50M", "{\"Age\":50,\"Sex\":\"M\",\"Flag\":\"no\"}", "AB"),
            ("t65F", "{\"Age\":65,\"Sex\":\"F\",\"Flag\":\"no\"}", "AC"),
            ("t30M", "{\"Age\":30,\"Sex\":\"M\",\"Flag\":\"si\"}", "CC"),
        };
        foreach (var (id, attrs, seq) in cases) l2.Add("{\"trace_id\":\"" + id + "\",\"attrs\":" + attrs + ",\"events\":{" + EventsJson(seq, 40) + "}}");
        var doc = Load(l2).IterateTraces().Select(t => (t.SampleId, t)).ToArray();
        md.AppendLine("### 7. Examples taken from the grammar documentation (letters instead of the documentation's names)\n");
        md.AppendLine("Traces (read by the real reader): `t70F` = A,B with Age 70, Sex F, Flag si · `t50M` = A,B with 50, M, no · `t65F` = A,C with 65, F, no · `t30M` = C,C with 30, M, si. Events are 40 minutes apart.\n");
        md.AppendLine("| Documentation example (adapted) | Syntax | t70F | t50M | t65F | t30M |");
        md.AppendLine("|---|---|---|---|---|---|");
        string[] ex =
        {
            "('A' >> 'B')[Sex == 'F', Age > 65]",
            "('A' >> 'B')[Sex == 'F', Age > 65][2h-]",
            "('A' >> 'B')[Sex == 'F', Age > 65][-2h]",
            "('A' >> 'B')[(Sex == 'F' | Sex == 'M') & Age > 18]",
            "('A' > 'B')[Sex == 'F', ?a]",
            "(!('A' >> 'C') & ('A' >> 'B'))[-2h] & [Age > 20]",
            "(!('A' >> 'C') & ('A' >> 'B')) & [Age > 20]",
            "!(('A') | ('B'))", "!('A') & !('B')",
            "[Sex == 'M' | Age > 60 & Flag == 'si']", "[(Sex == 'M' | Age > 60) & Flag == 'si']", "[Sex == 'M' | (Age > 60 & Flag == 'si')]",
            "'A' & 'A' ->* 'B'", "'A' & 'A' -> 'B'", "('A' & ('A' ->+ 'B'))", "!('A' >> 'C') & ('A' ->+ 'B')",
            "A['Sex' == 'F']", "('A')['Sex' == 'F']", "A > B", "A >> B",
        };
        foreach (var e in ex)
        {
            var syn = DeclarativeIPMNs.DeclarativeIPMValidation.SyntaxError(e) == null ? "ok" : "SYN";
            md.AppendLine($"| `{e}` | {syn} | " + string.Join(" | ", doc.Select(x => syn == "SYN" ? "–" : Probe.Run(e, x.t))) + " |");
        }
        md.AppendLine();
    }

    static string Cell(string r) => r switch { "1" => "1", "0" => "0", _ => "E" };
}
