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

    // --- /me ---

    /// <summary>
    /// Gets the authenticated principal's profile.
    /// </summary>
    Task<MeResponse> GetMeAsync(CancellationToken ct = default);

    /// <summary>
    /// Updates the authenticated principal's profile.
    /// </summary>
    Task<MeResponse> UpdateMeAsync(string? name = null, string? email = null, CancellationToken ct = default);

    /// <summary>
    /// Creates a personal API key scoped to the authenticated principal.
    /// </summary>
    Task<KeyCreateResult> CreateMeKeyAsync(string label, string[]? permissions = null, CancellationToken ct = default);

    /// <summary>
    /// Deletes a personal API key by ID. Returns <c>false</c> if it did not exist.
    /// </summary>
    Task<bool> DeleteMeKeyAsync(string id, CancellationToken ct = default);

    // --- Admin: Orgs ---

    /// <summary>
    /// Creates a new organization.
    /// </summary>
    Task<OrgResponse> CreateOrgAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Lists all organizations.
    /// </summary>
    Task<IReadOnlyList<OrgResponse>> ListOrgsAsync(CancellationToken ct = default);

    /// <summary>
    /// Deletes an organization by ID. Returns <c>false</c> if it did not exist.
    /// </summary>
    Task<bool> DeleteOrgAsync(string id, CancellationToken ct = default);

    // --- Admin: Principals ---

    /// <summary>
    /// Creates a new principal (user or service account).
    /// </summary>
    Task<PrincipalResponse> CreatePrincipalAsync(string role, string? email = null, string? name = null, string? org = null, CancellationToken ct = default);

    /// <summary>
    /// Lists all principals.
    /// </summary>
    Task<IReadOnlyList<PrincipalResponse>> ListPrincipalsAsync(CancellationToken ct = default);

    /// <summary>
    /// Deletes a principal by ID. Returns <c>false</c> if it did not exist.
    /// </summary>
    Task<bool> DeletePrincipalAsync(string id, CancellationToken ct = default);

    // --- Admin: Roles ---

    /// <summary>
    /// Creates a new role.
    /// </summary>
    Task<RoleResponse> CreateRoleAsync(string name, string[] permissions, CancellationToken ct = default);

    /// <summary>
    /// Lists all roles.
    /// </summary>
    Task<IReadOnlyList<RoleResponse>> ListRolesAsync(CancellationToken ct = default);

    /// <summary>
    /// Deletes a role by ID. Returns <c>false</c> if it did not exist.
    /// </summary>
    Task<bool> DeleteRoleAsync(string id, CancellationToken ct = default);
}
