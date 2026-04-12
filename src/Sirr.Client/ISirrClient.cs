namespace Sirr;

/// <summary>
/// Async client for the Sirr ephemeral secrets API.
/// </summary>
public interface ISirrClient
{
    /// <summary>
    /// Create a secret. Returns metadata including the hash.
    /// </summary>
    Task<SecretResponse> PushAsync(string value, TimeSpan? ttl = null, int? reads = null, string? prefix = null, CancellationToken ct = default);

    /// <summary>
    /// Read a secret's value. Consumes a read.
    /// Returns null if 410 (burned/expired/non-existent).
    /// </summary>
    Task<string?> GetAsync(string hash, CancellationToken ct = default);

    /// <summary>
    /// Metadata only via HEAD. Does NOT consume a read.
    /// Returns null if 410 (burned/expired/non-existent).
    /// </summary>
    Task<SecretStatus?> InspectAsync(string hash, CancellationToken ct = default);

    /// <summary>
    /// Update a secret's value/TTL/reads (owner key required).
    /// </summary>
    Task<SecretResponse> PatchAsync(string hash, string? value = null, TimeSpan? ttl = null, int? reads = null, CancellationToken ct = default);

    /// <summary>
    /// Burn a secret immediately (DELETE).
    /// </summary>
    Task BurnAsync(string hash, CancellationToken ct = default);

    /// <summary>
    /// Get the audit trail for a secret (owner key required).
    /// </summary>
    Task<AuditResponse> AuditAsync(string hash, CancellationToken ct = default);

    /// <summary>
    /// List all secrets owned by the calling key.
    /// </summary>
    Task<IReadOnlyList<SecretMetadata>> ListAsync(CancellationToken ct = default);

    /// <summary>
    /// Helper: list all owned secrets and fetch their values.
    /// Note: consumes a read for each.
    /// </summary>
    Task<IReadOnlyDictionary<string, string>> PullAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Pulls all secrets and sets them as environment variables.
    /// Dispose the returned scope to restore original values.
    /// </summary>
    Task<EnvScope> CreateEnvScopeAsync(CancellationToken ct = default);
}
