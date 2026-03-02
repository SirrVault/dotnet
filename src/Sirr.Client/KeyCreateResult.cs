#pragma warning disable CS1591 // Record properties are self-documenting via JsonPropertyName
using System.Text.Json.Serialization;

namespace Sirr;

/// <summary>
/// Result of creating a personal API key via /me/keys — includes the raw key (shown once).
/// </summary>
public sealed record KeyCreateResult
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("key")]
    public required string Key { get; init; }
}
