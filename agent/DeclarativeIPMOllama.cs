#nullable disable
using PMAppSVELTA.Chat.Ollama;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace PMAppSVELTA.DeclarativeIPM
{
    /// <summary>
    /// Own access to Ollama: the same <see cref="OllamaClient"/> SVELTA's profile uses, but without going through the profiles. A single call
    /// (system + user), no history.
    ///
    /// The configuration (endpoint, model and API key) lives in a file OUTSIDE this folder, so the key is never published with the code:
    /// <c>%APPDATA%\DeclarativeIPM\ollama.json</c> (or the path in the environment variable DECLARATIVEIPM_CONFIG), shaped like
    /// <c>{ "Endpoint": "http://server:port", "Model": "model", "ApiKey": "…" }</c>. The properties can also be set in code (e.g. to compare models).
    /// </summary>
    public class DeclarativeIPMOllama : IDeclarativeIPMLlm
    {
        public string Endpoint { get; set; } = "http://localhost:10980";
        public string Model { get; set; } = "n27/gemma-4-26B-A4B-it-UD-Q4_K_M-32k";
        public string ApiKey { get; set; } = "";
        public int TimeoutSeconds { get; set; } = 300;

        public string Name => "ollama:" + Model;

        /// <summary>Where the configuration is looked up.</summary>
        public static string ConfigPath
        {
            get
            {
                var env = Environment.GetEnvironmentVariable("DECLARATIVEIPM_CONFIG");
                if (!string.IsNullOrWhiteSpace(env)) return env.Trim();
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeclarativeIPM", "ollama.json");
            }
        }

        public DeclarativeIPMOllama()
        {
            try
            {
                var path = ConfigPath;
                if (!File.Exists(path)) return;
                if (JsonNode.Parse(File.ReadAllText(path)) is not JsonObject cfg) return;
                if (cfg["Endpoint"]?.ToString() is { Length: > 0 } e) Endpoint = e;
                if (cfg["Model"]?.ToString() is { Length: > 0 } m) Model = m;
                if (cfg["ApiKey"]?.ToString() is { Length: > 0 } k) ApiKey = k;
            }
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { }   // no readable file: keep the defaults
        }

        public async Task<DeclarativeIPMLlmReply> ChatAsync(string system, string user, CancellationToken ct = default)
        {
            using var client = new OllamaClient(new OllamaOptions
            {
                BaseUrl = Endpoint,
                ApiKey = ApiKey,
                Timeout = TimeSpan.FromSeconds(TimeoutSeconds)
            });

            var messages = new List<OllamaChatMessage>
            {
                OllamaChatMessage.System(system),
                new OllamaChatMessage { Role = "user", Content = user }
            };
            var sw = Stopwatch.StartNew();
            var chat = await client.ChatAsync(Model, messages, ct);
            sw.Stop();
            var content = chat?.Message?.Content;
            if (string.IsNullOrWhiteSpace(content)) throw new InvalidOperationException("Ollama returned an empty response");
            return new DeclarativeIPMLlmReply { Text = content, Seconds = sw.Elapsed.TotalSeconds, OutputTokens = chat.EvalCount };
        }
    }
}
