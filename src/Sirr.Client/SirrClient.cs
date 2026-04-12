using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Sirr.Internal;

namespace Sirr;

/// <summary>
/// HTTP client for the Sirr ephemeral secrets API.
/// </summary>
public sealed class SirrClient : ISirrClient, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
    };

    private readonly HttpClient _http;
    private readonly bool _ownsHttpClient;

    /// <summary>Initializes a new client with options.</summary>
    public SirrClient(SirrOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _http = new HttpClient
        {
            BaseAddress = new Uri(options.Server.TrimEnd('/')),
        };
        if (!string.IsNullOrEmpty(options.Token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.Token);
        }
        _ownsHttpClient = true;
    }

    /// <summary>Initializes a new client with server and token.</summary>
    public SirrClient(string server, string? token = null)
        : this(new SirrOptions { Server = server, Token = token ?? string.Empty })
    {
    }

    /// <summary>Initializes a new client with an existing HttpClient.</summary>
    [ActivatorUtilitiesConstructor]
    public SirrClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _http = httpClient;
        _ownsHttpClient = false;
    }

    // --- Secrets ---

    /// <inheritdoc />
    public async Task<SecretResponse> PushAsync(string value, TimeSpan? ttl = null, int? reads = null, string? prefix = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(value);
        var payload = new PushRequest
        {
            Value = value,
            TtlSeconds = ttl.HasValue ? (long)ttl.Value.TotalSeconds : null,
            Reads = reads,
            Prefix = prefix
        };
        return await SendAsync<SecretResponse>(HttpMethod.Post, "/secret", payload, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<string?> GetAsync(string hash, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(hash);
        using var response = await _http.GetAsync($"/secret/{Uri.EscapeDataString(hash)}", ct).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.Gone) return null;
        if (!response.IsSuccessStatusCode)
        {
            var msg = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            throw new SirrException((int)response.StatusCode, msg);
        }
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SecretStatus?> InspectAsync(string hash, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(hash);
        using var request = new HttpRequestMessage(HttpMethod.Head, $"/secret/{Uri.EscapeDataString(hash)}");
        using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.Gone) return null;
        if (!response.IsSuccessStatusCode)
        {
            var msg = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            throw new SirrException((int)response.StatusCode, msg);
        }

        var h = response.Headers;
        static string? GetHeader(System.Net.Http.Headers.HttpResponseHeaders headers, string name) =>
            headers.TryGetValues(name, out var vals) ? vals.FirstOrDefault() : null;

        var readsRaw = GetHeader(h, "X-Sirr-Reads-Remaining");
        return new SecretStatus
        {
            Created = GetHeader(h, "X-Sirr-Created") ?? "",
            TtlExpires = GetHeader(h, "X-Sirr-TTL-Expires"),
            ReadsRemaining = int.TryParse(readsRaw, out var r) ? r : null,
            Owned = GetHeader(h, "X-Sirr-Owned") == "true"
        };
    }

    /// <inheritdoc />
    public async Task<SecretResponse> PatchAsync(string hash, string? value = null, TimeSpan? ttl = null, int? reads = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(hash);
        var payload = new PatchRequest
        {
            Value = value,
            TtlSeconds = ttl.HasValue ? (long)ttl.Value.TotalSeconds : null,
            Reads = reads
        };
        return await SendAsync<SecretResponse>(HttpMethod.Patch, $"/secret/{Uri.EscapeDataString(hash)}", payload, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task BurnAsync(string hash, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(hash);
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/secret/{Uri.EscapeDataString(hash)}");
        using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.Gone) return;
        if (!response.IsSuccessStatusCode)
        {
            var msg = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            throw new SirrException((int)response.StatusCode, msg);
        }
    }

    /// <inheritdoc />
    public async Task<AuditResponse> AuditAsync(string hash, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(hash);
        var internalResp = await SendAsync<AuditResponseInternal>(HttpMethod.Get, $"/secret/{Uri.EscapeDataString(hash)}/audit", null, ct).ConfigureAwait(false);
        return new AuditResponse
        {
            Hash = internalResp.Hash,
            CreatedAt = internalResp.CreatedAt,
            Events = internalResp.Events
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SecretMetadata>> ListAsync(CancellationToken ct = default)
    {
        var response = await SendAsync<SecretMetadata[]>(HttpMethod.Get, "/secrets", null, ct).ConfigureAwait(false);
        return response;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, string>> PullAllAsync(CancellationToken ct = default)
    {
        var metas = await ListAsync(ct).ConfigureAwait(false);
        var result = new Dictionary<string, string>();
        foreach (var meta in metas)
        {
            if (!meta.Burned)
            {
                var val = await GetAsync(meta.Hash, ct).ConfigureAwait(false);
                if (val is not null) result[meta.Hash] = val;
            }
        }
        return result;
    }

    /// <inheritdoc />
    public async Task<EnvScope> CreateEnvScopeAsync(CancellationToken ct = default)
    {
        var secrets = await PullAllAsync(ct).ConfigureAwait(false);
        return new EnvScope(secrets);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsHttpClient) _http.Dispose();
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? content, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (content is not null) request.Content = JsonContent.Create(content, options: JsonOptions);
        using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            string? errorMessage = null;
            try
            {
                var errorBody = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions, ct).ConfigureAwait(false);
                errorMessage = errorBody?.Message ?? errorBody?.Error;
            }
            catch { errorMessage = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false); }
            throw new SirrException((int)response.StatusCode, errorMessage ?? response.ReasonPhrase ?? "Unknown error");
        }

        if (response.StatusCode == HttpStatusCode.NoContent) return default!;
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct).ConfigureAwait(false);
        return result ?? throw new SirrException((int)response.StatusCode, "Empty response body");
    }
}
