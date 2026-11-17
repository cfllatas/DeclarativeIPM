#nullable disable
using PMAppSVELTA.Chat.PoliGpt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace PMAppSVELTA.DeclarativeIPM
{
    /// <summary>
    /// Own access to PoliGPT (OpenAI-compatible, https://api.poligpt.upv.es), using SVELTA's <see cref="PoliGptClient"/> and not the LLM profiles. One call, no history.
    ///
    /// Configuration OUTSIDE the repository: <c>%APPDATA%\DeclarativeIPM\poligpt.json</c> (or the path in DECLARATIVEIPM_POLIGPT_CONFIG):
    /// <c>{ "Endpoint": "https://api.poligpt.upv.es", "Model": "model-id", "ApiKey": "…" }</c>. The properties can also be set in code.
    /// </summary>
    public class DeclarativeIPMPoliGpt : IDeclarativeIPMLlm
    {
        public string Endpoint { get; set; } = "https://api.poligpt.upv.es";
        public string Model { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public int TimeoutSeconds { get; set; } = 300;

        public string Name => "poligpt:" + Model;

        public static string ConfigPath
        {
            get
            {
                var env = Environment.GetEnvironmentVariable("DECLARATIVEIPM_POLIGPT_CONFIG");
                if (!string.IsNullOrWhiteSpace(env)) return env.Trim();
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeclarativeIPM", "poligpt.json");
            }
        }

        public DeclarativeIPMPoliGpt()
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
            catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) { }
        }

        public async Task<DeclarativeIPMLlmReply> ChatAsync(string system, string user, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(Model)) throw new InvalidOperationException("No PoliGPT model set (property Model or poligpt.json)");
            using var client = new PoliGptClient(new PoliGptOptions { BaseUrl = Endpoint, ApiKey = ApiKey, Timeout = TimeSpan.FromSeconds(TimeoutSeconds) });
            var sw = Stopwatch.StartNew();
            var res = await client.ChatAsync(Model, new[] { PoliGptMessage.System(system), PoliGptMessage.User(user) }, ct);
            sw.Stop();
            var content = res?.Message?.Content;
            if (string.IsNullOrWhiteSpace(content)) throw new InvalidOperationException("PoliGPT returned an empty response");
            return new DeclarativeIPMLlmReply { Text = content, Seconds = sw.Elapsed.TotalSeconds };
        }

        /// <summary>The model ids the endpoint offers (GET /v1/models).</summary>
        public async Task<List<string>> ListModelsAsync(CancellationToken ct = default)
        {
            using var http = new HttpClient { BaseAddress = new Uri(Endpoint.TrimEnd('/') + "/"), Timeout = TimeSpan.FromSeconds(60) };
            if (!string.IsNullOrWhiteSpace(ApiKey)) http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
            using var resp = await http.GetAsync("v1/models", ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode) throw new InvalidOperationException($"HTTP {(int)resp.StatusCode}: {(body.Length > 300 ? body.Substring(0, 300) : body)}");
            var data = (JsonNode.Parse(body)?["data"] as JsonArray) ?? new JsonArray();
            return data.Select(n => n?["id"]?.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)).OrderBy(s => s, StringComparer.Ordinal).ToList();
        }
    }
}
