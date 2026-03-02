#pragma warning disable CS1591 // Record properties are self-documenting via JsonPropertyName
using System.Text.Json.Serialization;

namespace Sirr;

/// <summary>
/// Response from GET /me — the authenticated principal's profile.
/// </summary>
public sealed record MeResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("email")]
    public string? Email { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("role")]
    public string? Role { get; init; }

    [JsonPropertyName("org")]
    public string? Org { get; init; }

    [JsonPropertyName("created_at")]
    public required long CreatedAt { get; init; }
}
