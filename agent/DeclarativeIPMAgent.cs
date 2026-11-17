#nullable disable
using pm4h.data;
using pm4h.svelta;
using pm4h.svelta.chains;
using pm4h.windows.interfaces;
using pm4h.windows.ui.fragments.appmanager;
using PMApp4Base.src.helper;
using PMAppSVELTA.declare;
using PMAppSVELTA.src;
using PMAppSVELTA.src.chat;
using PMAppSVELTA.src.chat.chains;
using PMAppSVELTA.src.chat.chains.ingestionchain.typeinferrer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PMAppSVELTA.DeclarativeIPM
{
    /// <summary>
    /// The agent: a natural-language order (query.prompt) -> ONE call to Ollama -> a typed operation on the log (query.json_result, with "kind").
    /// No retries: the response is validated once and, if anything is wrong, nothing is applied and query.result explains what (json_result stays null, so
    /// <see cref="DeclarativeIPMDispatch"/> does nothing). The agent never asks back (no second turn): a "clarification" answer is just an invalid kind.
    /// </summary>
    public class DeclarativeIPMAgent : Chain
    {
        /// <summary>The model. Ollama with the external configuration by default; any <see cref="IDeclarativeIPMLlm"/> works (e.g. PoliGPT).</summary>
        public IDeclarativeIPMLlm Llm { get; set; } = new DeclarativeIPMOllama();

        public override async Task<ChainValue> RunStepAsync(ChainValue ctx)
        {
            var cui = ctx.GetSingleton<IChatUI>();
            var query = ctx.Wrap<ILLMQuery>();
            query.json_result = null;

            try
            {
                if (string.IsNullOrWhiteSpace(query.prompt)) return Fail(ctx, "You have not told me what you want to change in the log.");
                if (PMAppWinHelper4.GetActiveExperiment() is not ExperimentArguments args) return Fail(ctx, "There is no open experiment.");
                if (PMAppWinHelper4.GetILogProvider(this, args.ContextId) is not IPMAppMainFactory provider || provider.getLog()?.FirstOrDefault() is not IPMLog log)
                    return Fail(ctx, "I cannot find the experiment's log.");
                var modelInfo = PMAppWinHelper4.LoadModelInfo(args);   // the saved one, or the automatic one if there is none
                if (modelInfo == null) return Fail(ctx, "The experiment has no ModelInfo.");

                cui?.SetStatus("Analysing data");
                var patterns = DeclareProcessPatternMiner.Mine(log);
                var dto = ModelInfoMapper.ToDto(modelInfo);
                var system = DeclarativeIPMPrompt.Build(dto, patterns, DeclarativeIPMState.Read(args));

                cui?.SetStatus("Asking the LLM");
                string raw;
                try { raw = (await Llm.ChatAsync(system, query.prompt.Trim(), ctx.CancellationToken)).Text; }
                catch (Exception ex) { return Fail(ctx, $"I could not query the model ({Llm.Name}): {ex.Message}"); }

                var json = DeclarativeIPMJson.Extract(raw);
                if (json == null) return Fail(ctx, "The model did not return a valid JSON.");

                var problems = Validate(json, patterns.Activities.ToList(), dto);
                if (problems.Count > 0)
                    return Fail(ctx, "I applied nothing, because what the model proposed is not valid:\n" + string.Join("\n", problems.Select(p => "- " + p)) + Shown(json));

                query.json_result = json;
                return ctx;
            }
            catch (Exception ex)
            {
                return Fail(ctx, ex.Message);
            }
        }

        /// <summary>Validates (and normalizes inside the JSON itself) the operation. An empty list means it can be applied.</summary>
        public static List<string> Validate(JsonObject json, IReadOnlyCollection<string> activities, ModelInfoDto mi = null)
        {
            var problems = new List<string>();
            switch (Text(json, "kind"))
            {
                case "filter":
                    {
                        var expr = DeclarativeIPMValidation.Normalize(Text(json, "expression"));
                        if (expr.Length == 0) { problems.Add(Text(json, "description") ?? "it cannot be expressed with the filter language"); break; }
                        json["expression"] = expr;
                        var syntax = DeclarativeIPMValidation.SyntaxError(expr);
                        if (syntax != null) problems.Add($"syntax error in «{expr}»: {syntax}");
                        problems.AddRange(DeclarativeIPMValidation.UnknownActivities(expr, activities));
                        problems.AddRange(DeclarativeIPMValidation.UnknownValues(expr, mi));
                        break;
                    }
                case "circuits":
                case "regions":
                    {
                        bool circuits = Text(json, "kind") == "circuits";
                        string key = circuits ? "circuits" : "patterns";
                        var (text, probs) = DeclarativeIPMValidation.CheckAssignments(Text(json, key), allowWildcard: circuits, activities, mi);
                        problems.AddRange(probs);
                        if (probs.Count == 0) json[key] = text;
                        break;
                    }
                case "milestones":
                    {
                        var nodes = (json["nodes"] as JsonArray)?.Select(n => n?.ToString()?.Trim().Trim('\'').Trim()).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList();   // the model sometimes copies the quotes of the list: here they are names without quotes
                        if (nodes == null || nodes.Count == 0) { problems.Add("there is no activity as a milestone"); break; }
                        var known = new HashSet<string>(activities, StringComparer.Ordinal);
                        foreach (var n in nodes.Where(n => !known.Contains(n))) problems.Add($"«{n}» is not the name of any activity");
                        if (problems.Count == 0) json["nodes"] = new JsonArray(nodes.Select(n => (JsonNode)JsonValue.Create(n)).ToArray());
                        break;
                    }
                default:
                    problems.Add($"unknown operation: «{Text(json, "kind")}»");
                    break;
            }
            return problems;
        }

        private static string Shown(JsonObject json) =>
            Text(json, "expression") is string e ? $"\nProposed expression: {e}"
            : Text(json, "circuits") is string c ? $"\nProposal:\n{c}"
            : Text(json, "patterns") is string p ? $"\nProposal:\n{p}" : "";

        private static string Text(JsonObject json, string key) =>
            json[key] is JsonNode n && n.ToString() is string s && !string.IsNullOrWhiteSpace(s) ? s.Trim() : null;

        private static ChainValue Fail(ChainValue ctx, string message)
        {
            var query = ctx.Wrap<ILLMQuery>();
            query.json_result = null;
            query.result = message;
            return ctx;
        }
    }
}
