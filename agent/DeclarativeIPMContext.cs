#nullable disable
using PMAppSVELTA.declare;
using PMAppSVELTA.src.chat.chains.ingestionchain.typeinferrer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PMAppSVELTA.DeclarativeIPM
{
    /// <summary>
    /// Plain-text description of the process, given to the model as context: activities, trace-level variables and the flow patterns observed in the log.
    /// Only trace-level variables are described (event-level attributes are out of scope).
    /// </summary>
    public static class DeclarativeIPMContext
    {
        public static string ProcessDescription(ModelInfoDto mi) =>
            mi?.Properties != null && mi.Properties.TryGetValue("description", out var d) && !string.IsNullOrWhiteSpace(d) ? d.Trim() : "No process description available.";

        /// <summary>Every activity, always in single quotes (the only form that is always valid in a Declare expression), with its description.</summary>
        public static string Activities(ModelInfoDto mi)
        {
            if (mi?.NodesInfo == null || mi.NodesInfo.Count == 0) return "(no activities)";
            var sb = new StringBuilder();
            foreach (var kv in mi.NodesInfo.OrderBy(k => k.Key, StringComparer.Ordinal))
            {
                string id = string.IsNullOrEmpty(kv.Value?.Id) ? kv.Key : kv.Value.Id;
                sb.AppendLine($"- {Quote(id)}: {Text(kv.Value?.Description)}");
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>Trace-level variables (one value per trace), with kind, range and, for categorical ones, the exact list of values.</summary>
        public static string TraceVariables(ModelInfoDto mi)
        {
            var attrs = mi?.Tracedata?.Values;
            if (attrs == null || !attrs.Any()) return "(no trace variables)";
            var sb = new StringBuilder();
            foreach (var a in attrs.OrderBy(x => x.Id, StringComparer.Ordinal))
            {
                string range = (a.Kind == "Numeric" || a.Kind == "DateTime" || a.Kind == "Time") && (a.Min.HasValue || a.Max.HasValue) ? $" (range: {a.Min?.ToString() ?? "?"} - {a.Max?.ToString() ?? "?"})" : "";
                string values = a.Kind == "Categorical" && a.Values is { Count: > 0 } ? $" (possible values: {string.Join(", ", a.Values.Select(v => Quote(string.IsNullOrWhiteSpace(v.Id) ? v.Name : v.Id)))})" : "";
                sb.AppendLine($"- {Quote(a.Id)} [{a.Kind}]: {Text(a.Description)}{range}{values}");
            }
            return sb.ToString().TrimEnd();
        }

        /// <summary>What the log allows with '>' and '>>': start/end activities, direct transitions with support, and what can happen later after each activity.</summary>
        public static string Patterns(ProcessPatterns p)
        {
            if (p == null || !p.Activities.Any()) return "(no flow patterns available)";
            var sb = new StringBuilder();
            sb.AppendLine($"Start activities (can be the first of a trace): {Join(p.StartActivities)}");
            sb.AppendLine($"End activities (can be the last of a trace): {Join(p.EndActivities)}");
            sb.AppendLine();
            sb.AppendLine("Observed direct transitions (X > Y is possible):");
            if (p.DirectFollows.Count == 0) sb.AppendLine("(none)");
            foreach (var kv in p.DirectFollows.OrderBy(k => k.Key.From, StringComparer.Ordinal).ThenBy(k => k.Key.To, StringComparer.Ordinal))
                sb.AppendLine($"{Quote(kv.Key.From)} > {Quote(kv.Key.To)} (seen in {kv.Value} trace{(kv.Value == 1 ? "" : "s")})");
            sb.AppendLine();
            sb.AppendLine("What can happen later after each activity (X >> Y is possible):");
            foreach (var a in p.Activities.OrderBy(a => a, StringComparer.Ordinal))
            {
                var reach = p.Reachable.TryGetValue(a, out var set) ? set : new HashSet<string>();
                sb.AppendLine($"{Quote(a)}: {(reach.Count == 0 ? "(nothing)" : Join(reach))}");
            }
            sb.AppendLine();
            sb.AppendLine("If a pair of activities is neither a direct transition nor in the later-reach of the first one, do NOT relate them with > or >>: it never happens in the data.");
            return sb.ToString().TrimEnd();
        }

        private static string Quote(string id) => id == null ? "''" : "'" + id + "'";
        private static string Text(string s) => string.IsNullOrWhiteSpace(s) ? "(no description)" : s.Trim();
        private static string Join(IEnumerable<string> items)
        {
            var list = items?.OrderBy(x => x, StringComparer.Ordinal).Select(Quote).ToList();
            return list == null || list.Count == 0 ? "(none)" : string.Join(", ", list);
        }
    }
}
