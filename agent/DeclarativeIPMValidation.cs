#nullable disable
using Irony.Parsing;
using pm4hlab.ext.tpaengine.declaregrammar;
using PMAppSVELTA.src.chat.chains.ingestionchain.typeinferrer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PMAppSVELTA.DeclarativeIPM
{
    /// <summary>
    /// Checks made BEFORE anything is applied. Declare does not raise an error on what it does not understand: it returns a filter that lets nobody through,
    /// which looks like "no patient matches". Here that is detected and reported. Own copy (it does not depend on any other SVELTA assistant).
    /// </summary>
    public static class DeclarativeIPMValidation
    {
        // The grammar is expensive to compile and Irony is not thread-safe: a single parser, behind a lock.
        private static readonly Lazy<AnalizadorSintactico> Parser = new(() => new AnalizadorSintactico(new DeclareFunctionGrammar()));

        /// <summary>The first syntax error of the Declare expression (what and where), or null if it parses.</summary>
        public static string SyntaxError(string expression)
        {
            try
            {
                lock (Parser)
                {
                    var tree = Parser.Value.Parse(expression);
                    if (!tree.HasErrors()) return null;
                    var m = tree.ParserMessages.FirstOrDefault();
                    return m == null ? "cannot be parsed" : $"{m.Message} (position {m.Location.Column + 1})";
                }
            }
            catch (Exception ex) { return "cannot be parsed (" + ex.Message + ")"; }
        }

        private static readonly Regex Brackets = new(@"\[[^\]]*\]", RegexOptions.Compiled);
        private static readonly Regex Quoted = new("'([^']*)'", RegexOptions.Compiled);

        /// <summary>Every quoted activity name that does not exist, and every regular expression ('#…') that is invalid or matches no activity. What is inside square brackets is ignored.</summary>
        public static List<string> UnknownActivities(string expression, IReadOnlyCollection<string> activities)
        {
            var problems = new List<string>();
            if (string.IsNullOrWhiteSpace(expression) || activities == null || activities.Count == 0) return problems;
            var known = new HashSet<string>(activities, StringComparer.Ordinal);

            foreach (Match m in Quoted.Matches(Brackets.Replace(expression, " ")))
            {
                var name = m.Groups[1].Value;
                if (name is "@Start" or "@End" || name.Length == 0) continue;
                string problem = null;
                if (name.StartsWith("#"))
                {
                    try
                    {
                        var rx = new Regex(name.Substring(1));
                        if (!activities.Any(a => rx.IsMatch(a))) problem = $"the regular expression «{name}» matches no activity";
                    }
                    catch (ArgumentException) { problem = $"the regular expression «{name}» is not valid"; }
                }
                else if (!known.Contains(name)) problem = $"«{name}» is not the name of any activity";
                if (problem != null && !problems.Contains(problem)) problems.Add(problem);
            }
            return problems;
        }

        private static readonly Regex Comparison = new(@"(?<![\w.])('(?<var>[^']+)'|(?<var>[A-Za-zÀ-ÿ_][\w]*))\s*(?<op>==|!=)\s*(?:'(?<val>[^']*)'|(?<num>-?\d+(?:[.,]\d+)?))", RegexOptions.Compiled);

        /// <summary>
        /// What the expression compares on a TRACE variable (inside square brackets) and does not exist or is badly written: a variable that is not in the model, a value that
        /// is not one of the variable's, or a value of a categorical variable without quotes (its values are text: '1', not 1). Declare raises no error: the filter lets nobody through.
        /// Values that are a regular expression ('#…') and variables without defined values are ignored.
        /// </summary>
        public static List<string> UnknownValues(string expression, ModelInfoDto mi)
        {
            var problems = new List<string>();
            if (string.IsNullOrWhiteSpace(expression) || mi?.Tracedata == null || mi.Tracedata.Count == 0) return problems;
            foreach (Match br in Brackets.Matches(expression))
                foreach (Match m in Comparison.Matches(br.Value))
                {
                    var name = m.Groups["var"].Value;
                    var attr = mi.Tracedata.Values.FirstOrDefault(a => string.Equals(a.Id, name, StringComparison.OrdinalIgnoreCase) || string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase));
                    string problem = null;
                    if (attr == null) problem = $"«{name}» is not a variable of the process";
                    else if (attr.Kind == "Categorical" && attr.Values is { Count: > 0 } values)
                    {
                        string shown = string.Join(", ", values.Take(15).Select(v => "'" + (string.IsNullOrWhiteSpace(v.Id) ? v.Name : v.Id) + "'")) + (values.Count > 15 ? ", …" : "");
                        var val = m.Groups["val"].Success ? m.Groups["val"].Value : m.Groups["num"].Value;   // the engine accepts a text value with or without quotes
                        if (!val.StartsWith("#") && !values.Any(v => string.Equals(v.Id, val, StringComparison.OrdinalIgnoreCase) || string.Equals(v.Name, val, StringComparison.OrdinalIgnoreCase)))
                            problem = $"«{val}» is not a value of «{attr.Id}»; its values are: {shown}";
                    }
                    if (problem != null && !problems.Contains(problem)) problems.Add(problem);
                }
            return problems;
        }

        private static readonly Regex Special = new(@"(?<![\w@])@(Start|End)\b", RegexOptions.Compiled);

        /// <summary>Puts '@Start' and '@End' in single quotes when they are outside quotes; what is already quoted is left alone.</summary>
        public static string Normalize(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression)) return "";
            var sb = new StringBuilder();
            var outside = new StringBuilder();
            bool inQuote = false;
            foreach (var ch in expression)
            {
                if (ch == '\'')
                {
                    if (!inQuote) { sb.Append(Special.Replace(outside.ToString(), "'@$1'")); outside.Clear(); }
                    inQuote = !inQuote;
                    sb.Append(ch);
                }
                else if (inQuote) sb.Append(ch);
                else outside.Append(ch);
            }
            sb.Append(Special.Replace(outside.ToString(), "'@$1'"));
            return sb.ToString().Trim();
        }

        private static readonly Regex Identifier = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        /// <summary>
        /// Validates a "name = expression" block, line by line (circuits and regions). Returns the normalized block ('@Start'/'@End' quoted) and the problems.
        /// In circuits the wildcard "." is a valid expression (catch-all); in regions it is not.
        /// </summary>
        public static (string Text, List<string> Problems) CheckAssignments(string text, bool allowWildcard, IReadOnlyCollection<string> activities, ModelInfoDto mi = null)
        {
            var problems = new List<string>();
            var lines = new List<string>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var raw in (text ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var line = raw.Trim();
                if (line.Length == 0) continue;
                int eq = line.IndexOf('=');
                if (eq <= 0) { problems.Add($"the line «{line}» does not have the form name = expression"); continue; }
                var name = line.Substring(0, eq).Trim();
                var expr = Normalize(line.Substring(eq + 1));
                if (!Identifier.IsMatch(name)) problems.Add($"«{name}» is not a valid name (a short identifier without spaces)");
                else if (!names.Add(name)) problems.Add($"the name «{name}» is repeated");
                if (expr == "." && allowWildcard) { lines.Add($"{name} = ."); continue; }
                if (expr.Length == 0) { problems.Add($"«{name}» has no expression"); continue; }
                if (expr == ".") { problems.Add($"«{name}»: the wildcard «.» does not exist in regions"); continue; }
                var syntax = SyntaxError(expr);
                if (syntax != null) problems.Add($"«{name}»: syntax error in «{expr}»: {syntax}");
                foreach (var u in UnknownActivities(expr, activities)) problems.Add($"«{name}»: {u}");
                foreach (var u in UnknownValues(expr, mi)) problems.Add($"«{name}»: {u}");
                lines.Add($"{name} = {expr}");
            }
            if (lines.Count == 0 && problems.Count == 0) problems.Add("there is no definition");
            return (string.Join("\n", lines), problems);
        }
    }
}
