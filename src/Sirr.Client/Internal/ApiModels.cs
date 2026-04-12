using System.Text.Json.Serialization;

namespace Sirr.Internal;

internal sealed class PushRequest
{
    [JsonPropertyName("value")]
    public required string Value { get; init; }

    [JsonPropertyName("ttl_seconds")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? TtlSeconds { get; init; }

    [JsonPropertyName("reads")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Reads { get; init; }

    [JsonPropertyName("prefix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Prefix { get; init; }
}

internal sealed class PatchRequest
{
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Value { get; init; }

    [JsonPropertyName("ttl_seconds")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? TtlSeconds { get; init; }

    [JsonPropertyName("reads")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Reads { get; init; }
}

internal sealed class AuditResponseInternal
{
    [JsonPropertyName("hash")]
    public required string Hash { get; init; }

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; init; }

    [JsonPropertyName("events")]
    public required AuditEvent[] Events { get; init; }
}

internal sealed class ApiErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }
}
