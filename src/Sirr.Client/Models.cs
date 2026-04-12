using System.Text.Json.Serialization;

namespace Sirr;

/// <summary>Response from POST /secret and PATCH /secret/{hash}.</summary>
public sealed class SecretResponse
{
    /// <summary>Unique secret hash.</summary>
    [JsonPropertyName("hash")]
    public required string Hash { get; init; }

    /// <summary>Direct URL to retrieve the secret.</summary>
    [JsonPropertyName("url")]
    public required string Url { get; init; }

    /// <summary>Unix epoch timestamp when the secret expires.</summary>
    [JsonPropertyName("expires_at")]
    public long? ExpiresAt { get; init; }

    /// <summary>Remaining read budget.</summary>
    [JsonPropertyName("reads_remaining")]
    public int? ReadsRemaining { get; init; }

    /// <summary>Whether the secret is owned by the calling key.</summary>
    [JsonPropertyName("owned")]
    public bool Owned { get; init; }
}

/// <summary>Metadata returned by HEAD /secret/{hash}. Does not consume a read.</summary>
public sealed class SecretStatus
{
    /// <summary>ISO-8601 creation timestamp.</summary>
    public required string Created { get; init; }
    /// <summary>ISO-8601 expiration timestamp.</summary>
    public string? TtlExpires { get; init; }
    /// <summary>Remaining read budget.</summary>
    public int? ReadsRemaining { get; init; }
    /// <summary>Whether the secret is owned by the calling key.</summary>
    public bool Owned { get; init; }
}

/// <summary>Single audit event from GET /secret/{hash}/audit.</summary>
public sealed class AuditEvent
{
    /// <summary>Event type (e.g. secret.read).</summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>Unix epoch timestamp of the event.</summary>
    [JsonPropertyName("at")]
    public long At { get; init; }

    /// <summary>Originating IP address.</summary>
    [JsonPropertyName("ip")]
    public required string Ip { get; init; }
}

/// <summary>Response from GET /secret/{hash}/audit.</summary>
public sealed class AuditResponse
{
    /// <summary>Secret hash.</summary>
    public required string Hash { get; init; }
    /// <summary>Unix epoch creation timestamp.</summary>
    public long CreatedAt { get; init; }
    /// <summary>List of audit events.</summary>
    public required IReadOnlyList<AuditEvent> Events { get; init; }
}

/// <summary>Metadata for a secret from GET /secrets.</summary>
public sealed class SecretMetadata
{
    /// <summary>Unique secret hash.</summary>
    [JsonPropertyName("hash")]
    public required string Hash { get; init; }

    /// <summary>Unix epoch creation timestamp.</summary>
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; init; }

    /// <summary>Unix epoch expiration timestamp.</summary>
    [JsonPropertyName("ttl_expires_at")]
    public long? TtlExpiresAt { get; init; }

    /// <summary>Remaining read budget.</summary>
    [JsonPropertyName("reads_remaining")]
    public int? ReadsRemaining { get; init; }

    /// <summary>Whether the secret is burned.</summary>
    [JsonPropertyName("burned")]
    public bool Burned { get; init; }

    /// <summary>Unix epoch burn timestamp.</summary>
    [JsonPropertyName("burned_at")]
    public long? BurnedAt { get; init; }

    /// <summary>Whether the secret is owned by the calling key.</summary>
    [JsonPropertyName("owned")]
    public bool Owned { get; init; }
}
