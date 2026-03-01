using System.Text.Json.Serialization;

namespace Sirr.Internal;

internal sealed class CreateSecretRequest
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }

    [JsonPropertyName("ttl_seconds")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? TtlSeconds { get; init; }

    [JsonPropertyName("max_reads")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxReads { get; init; }
}

internal sealed class CreateSecretResponse
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }
}

internal sealed class GetSecretResponse
{
    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("value")]
    public required string Value { get; init; }
}

internal sealed class ListSecretsResponse
{
    [JsonPropertyName("secrets")]
    public required SecretMeta[] Secrets { get; init; }
}

internal sealed class DeleteSecretResponse
{
    [JsonPropertyName("deleted")]
    public required bool Deleted { get; init; }
}

internal sealed class PruneResponse
{
    [JsonPropertyName("pruned")]
    public required int Pruned { get; init; }
}

internal sealed class HealthResponse
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }
}

internal sealed class ErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; init; }
}

internal sealed class AuditEventsResponse
{
    [JsonPropertyName("events")]
    public required AuditEvent[] Events { get; init; }
}

internal sealed class CreateWebhookRequest
{
    [JsonPropertyName("url")]
    public required string Url { get; init; }

    [JsonPropertyName("events")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? Events { get; init; }
}

internal sealed class ListWebhooksResponse
{
    [JsonPropertyName("webhooks")]
    public required Webhook[] Webhooks { get; init; }
}

internal sealed class DeletedResponse
{
    [JsonPropertyName("deleted")]
    public required bool Deleted { get; init; }
}

internal sealed class CreateApiKeyRequest
{
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    [JsonPropertyName("permissions")]
    public required string[] Permissions { get; init; }

    [JsonPropertyName("prefix")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Prefix { get; init; }
}

internal sealed class ListApiKeysResponse
{
    [JsonPropertyName("keys")]
    public required ApiKey[] Keys { get; init; }
}
