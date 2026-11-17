#nullable disable
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace PMAppSVELTA.DeclarativeIPM
{
    /// <summary>What a model call returns: the text, the wall-clock time of the call and, when the backend reports it, the number of generated tokens.</summary>
    public sealed class DeclarativeIPMLlmReply
    {
        public string Text { get; set; }
        /// <summary>Wall-clock seconds of the call, measured the same way for every backend (network included).</summary>
        public double Seconds { get; set; }
        public int? OutputTokens { get; set; }
    }

    /// <summary>
    /// The only thing the agent needs from a model: one system prompt + one user message in, text out. Implemented by <see cref="DeclarativeIPMOllama"/> and
    /// <see cref="DeclarativeIPMPoliGpt"/>; anything OpenAI-compatible can be added the same way.
    /// </summary>
    public interface IDeclarativeIPMLlm
    {
        /// <summary>Short label for messages and reports, e.g. "ollama:model-name".</summary>
        string Name { get; }
        Task<DeclarativeIPMLlmReply> ChatAsync(string system, string user, CancellationToken ct = default);
    }

    /// <summary>Extracts the JSON object from a model response.</summary>
    public static class DeclarativeIPMJson
    {
        /// <summary>The JSON object in the response: tolerates "&lt;/think&gt;", markdown fences and surrounding text. null if there is no valid one.</summary>
        public static JsonObject Extract(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            int t = text.LastIndexOf("</think>", StringComparison.Ordinal);
            if (t >= 0) text = text.Substring(t + "</think>".Length);
            int a = text.IndexOf('{'), b = text.LastIndexOf('}');
            if (a < 0 || b <= a) return null;
            try { return JsonNode.Parse(text.Substring(a, b - a + 1)) as JsonObject; }
            catch (JsonException) { return null; }
        }
    }
}
