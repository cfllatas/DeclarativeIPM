// Own minimal calls to the two backends of the study (TEST_PLAN W9). They exist because the study needs what SVELTA's clients do not expose: an EXPLICIT context size,
// temperature, seed and thinking switch, and the token counts of every call. Nothing here changes SVELTA's own clients.
// Endpoints and keys are read from files OUTSIDE the repository: %APPDATA%\DeclarativeIPM\ollama.json and poligpt.json ({Endpoint, Model, ApiKey}).
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DeclarativeIpmEval;

public sealed class ModelSpec2
{
    public string Id { get; set; }          // short label used in reports
    public string Backend { get; set; }     // ollama | poligpt
    public string Model { get; set; }
    /// <summary>Ollama "think" switch: false = off, "low" = the lowest level (models that cannot be switched off, like gpt-oss). Null = do not send.</summary>
    public string Think { get; set; } = "false";
}

public sealed class CallResult
{
    public string Text, Thinking, Error;
    public double Seconds;
    public int? PromptTokens, OutputTokens;
    public double? LoadSeconds, PromptEvalSeconds, EvalSeconds, ServerSeconds;   // Ollama's own timings (null for PoliGPT)
    public double ThrottleSeconds;   // PoliGPT: time waiting for the minimum interval between calls; NOT included in Seconds
    public bool ThinkApplied = true;
}

public sealed class ModelCaller : IDisposable
{
    readonly HttpClient _http = new() { Timeout = TimeSpan.FromMinutes(10) };
    readonly JsonObject _ollama, _poligpt;
    DateTime _lastPoli = DateTime.MinValue;
    public int NumCtx { get; set; } = 16384;
    public int Seed { get; set; } = 1;
    public double MinIntervalPoliSeconds { get; set; } = 10;

    static JsonObject Cfg(string file)
    {
        var p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DeclarativeIPM", file);
        return File.Exists(p) ? JsonNode.Parse(File.ReadAllText(p)) as JsonObject : null;
    }

    public ModelCaller() { _ollama = Cfg("ollama.json"); _poligpt = Cfg("poligpt.json"); }

    public async Task<CallResult> CallAsync(ModelSpec2 m, string system, string user)
    {
        var sw = Stopwatch.StartNew();
        var r = new CallResult();
        try
        {
            if (m.Backend == "ollama") await OllamaAsync(m, system, user, r);
            else if (m.Backend == "poligpt") await PoliAsync(m, system, user, r);
            else throw new ArgumentException("unknown backend " + m.Backend);
        }
        catch (Exception ex) { r.Error = ex.GetType().Name + ": " + ex.Message; }
        r.Seconds = Math.Max(0, sw.Elapsed.TotalSeconds - r.ThrottleSeconds);   // the throttle wait is not the model's time
        return r;
    }

    async Task OllamaAsync(ModelSpec2 m, string system, string user, CallResult r)
    {
        if (_ollama == null) throw new InvalidOperationException("ollama.json not found");
        for (int attempt = 0; attempt < 2; attempt++)
        {
            var body = new JsonObject
            {
                ["model"] = m.Model, ["stream"] = false,
                ["messages"] = new JsonArray(new JsonObject { ["role"] = "system", ["content"] = system }, new JsonObject { ["role"] = "user", ["content"] = user }),
                ["options"] = new JsonObject { ["num_ctx"] = NumCtx, ["temperature"] = 0, ["seed"] = Seed, ["num_predict"] = 4096 },
                ["keep_alive"] = "10m"
            };
            if (attempt == 0 && m.Think != null) body["think"] = m.Think == "false" ? JsonValue.Create(false) : JsonValue.Create(m.Think);
            else r.ThinkApplied = false;
            using var req = new HttpRequestMessage(HttpMethod.Post, _ollama["Endpoint"]!.ToString().TrimEnd('/') + "/api/chat") { Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json") };
            req.Headers.Add("X-API-Key", _ollama["ApiKey"]?.ToString() ?? "");
            using var resp = await _http.SendAsync(req);
            var txt = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                if (attempt == 0 && txt.Contains("think", StringComparison.OrdinalIgnoreCase)) continue;   // the model does not know the switch: ask again without it
                throw new InvalidOperationException($"HTTP {(int)resp.StatusCode}: {(txt.Length > 300 ? txt[..300] : txt)}");
            }
            var j = JsonNode.Parse(txt);
            r.Text = j["message"]?["content"]?.ToString();
            r.Thinking = j["message"]?["thinking"]?.ToString();
            r.PromptTokens = j["prompt_eval_count"]?.GetValue<int>();
            r.OutputTokens = j["eval_count"]?.GetValue<int>();
            double? Ns(string k) => j[k] == null ? null : j[k].GetValue<long>() / 1e9;
            r.LoadSeconds = Ns("load_duration"); r.PromptEvalSeconds = Ns("prompt_eval_duration"); r.EvalSeconds = Ns("eval_duration"); r.ServerSeconds = Ns("total_duration");
            return;
        }
    }

    async Task PoliAsync(ModelSpec2 m, string system, string user, CallResult r)
    {
        if (_poligpt == null) throw new InvalidOperationException("poligpt.json not found");
        var wait = _lastPoli.AddSeconds(MinIntervalPoliSeconds) - DateTime.UtcNow;   // PoliGPT is shared: never faster than one call every MinIntervalPoliSeconds
        if (wait > TimeSpan.Zero) { await Task.Delay(wait); r.ThrottleSeconds = wait.TotalSeconds; }
        _lastPoli = DateTime.UtcNow;
        var body = new JsonObject
        {
            ["model"] = m.Model, ["stream"] = false, ["temperature"] = 0, ["seed"] = Seed, ["max_tokens"] = 4096,
            ["messages"] = new JsonArray(new JsonObject { ["role"] = "system", ["content"] = system }, new JsonObject { ["role"] = "user", ["content"] = user })
        };
        using var req = new HttpRequestMessage(HttpMethod.Post, _poligpt["Endpoint"]!.ToString().TrimEnd('/') + "/v1/chat/completions") { Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json") };
        req.Headers.Add("Authorization", "Bearer " + _poligpt["ApiKey"]);
        using var resp = await _http.SendAsync(req);
        var txt = await resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode) throw new InvalidOperationException($"HTTP {(int)resp.StatusCode}: {(txt.Length > 300 ? txt[..300] : txt)}");
        var j = JsonNode.Parse(txt);
        r.Text = j["choices"]?[0]?["message"]?["content"]?.ToString();
        r.Thinking = j["choices"]?[0]?["message"]?["reasoning_content"]?.ToString();
        r.PromptTokens = j["usage"]?["prompt_tokens"]?.GetValue<int>();
        r.OutputTokens = j["usage"]?["completion_tokens"]?.GetValue<int>();
        _lastPoli = DateTime.UtcNow;
    }

    /// <summary>Ollama server version and the digest and size of the given models (for the run's metadata).</summary>
    public async Task<JsonObject> OllamaInfoAsync(IEnumerable<string> models)
    {
        var res = new JsonObject();
        if (_ollama == null) return res;
        async Task<JsonNode> Get(string path)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, _ollama["Endpoint"]!.ToString().TrimEnd('/') + path);
            req.Headers.Add("X-API-Key", _ollama["ApiKey"]?.ToString() ?? "");
            using var resp = await _http.SendAsync(req);
            return JsonNode.Parse(await resp.Content.ReadAsStringAsync());
        }
        try { res["version"] = (await Get("/api/version"))["version"]?.ToString(); } catch { }
        try
        {
            var tags = (await Get("/api/tags"))["models"] as JsonArray ?? new JsonArray();
            var want = models.ToHashSet();
            var d = new JsonObject();
            foreach (var t in tags)
                if (want.Contains(t["name"]?.ToString() ?? "")) d[t["name"].ToString()] = new JsonObject { ["digest"] = t["digest"]?.ToString(), ["sizeBytes"] = t["size"]?.GetValue<long>(), ["modifiedAt"] = t["modified_at"]?.ToString() };
            res["models"] = d;
        }
        catch { }
        return res;
    }

    public void Dispose() => _http.Dispose();
}
