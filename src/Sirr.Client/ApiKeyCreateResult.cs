#pragma warning disable CS1591 // Record properties are self-documenting via JsonPropertyName
using System.Text.Json.Serialization;

namespace Sirr;

/// <summary>
/// Result of creating an API key — includes the raw key (shown once).
/// </summary>
public sealed record ApiKeyCreateResult
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("key")]
    public required string Key { get; init; }

    [JsonPropertyName("label")]
    public required string Label { get; init; }

    [JsonPropertyName("permissions")]
    public required string[] Permissions { get; init; }

    [JsonPropertyName("prefix")]
    public string? Prefix { get; init; }
}
