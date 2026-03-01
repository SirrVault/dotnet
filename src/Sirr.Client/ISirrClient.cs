namespace Sirr;

/// <summary>
/// Async client for the Sirr ephemeral secrets API.
/// </summary>
public interface ISirrClient
{
    /// <summary>
    /// Checks if the Sirr server is healthy. Does not require authentication.
    /// </summary>
    Task<bool> HealthAsync(CancellationToken ct = default);

    /// <summary>
    /// Stores a secret with optional TTL and read limit.
    /// </summary>
    Task PushAsync(string key, string value, TimeSpan? ttl = null, int? reads = null, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a secret value. Returns <c>null</c> if the secret is burned, expired, or does not exist.
    /// </summary>
    Task<string?> GetAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Deletes a secret. Returns <c>true</c> if deleted, <c>false</c> if it did not exist.
    /// </summary>
    Task<bool> DeleteAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Lists metadata for all active secrets. Values are never included.
    /// </summary>
    Task<IReadOnlyList<SecretMeta>> ListAsync(CancellationToken ct = default);

    /// <summary>
    /// Pulls all secrets into a dictionary of key-value pairs.
    /// Secrets that are burned during retrieval are silently skipped.
    /// </summary>
    Task<IReadOnlyDictionary<string, string>> PullAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Prunes expired secrets from the server. Returns the number of secrets pruned.
    /// </summary>
    Task<int> PruneAsync(CancellationToken ct = default);

    /// <summary>
    /// Pulls all secrets and sets them as environment variables.
    /// Dispose the returned scope to restore original values.
    /// </summary>
    Task<EnvScope> CreateEnvScopeAsync(CancellationToken ct = default);

    /// <summary>
    /// Queries the audit log with optional filters.
    /// </summary>
    Task<IReadOnlyList<AuditEvent>> GetAuditLogAsync(long? since = null, long? until = null, string? action = null, int? limit = null, CancellationToken ct = default);

    /// <summary>
    /// Registers a webhook endpoint.
    /// </summary>
    Task<WebhookCreateResult> CreateWebhookAsync(string url, string[]? events = null, CancellationToken ct = default);

    /// <summary>
    /// Lists all registered webhooks. Signing secrets are redacted.
    /// </summary>
    Task<IReadOnlyList<Webhook>> ListWebhooksAsync(CancellationToken ct = default);

    /// <summary>
    /// Deletes a webhook by ID. Returns <c>false</c> if it did not exist.
    /// </summary>
    Task<bool> DeleteWebhookAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Creates a scoped API key. The raw key is returned once.
    /// </summary>
    Task<ApiKeyCreateResult> CreateApiKeyAsync(string label, string[]? permissions = null, string? prefix = null, CancellationToken ct = default);

    /// <summary>
    /// Lists all scoped API keys. Key hashes are never returned.
    /// </summary>
    Task<IReadOnlyList<ApiKey>> ListApiKeysAsync(CancellationToken ct = default);

    /// <summary>
    /// Deletes an API key by ID. Returns <c>false</c> if it did not exist.
    /// </summary>
    Task<bool> DeleteApiKeyAsync(string id, CancellationToken ct = default);
}
