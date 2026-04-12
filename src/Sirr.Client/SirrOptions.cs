namespace Sirr;

/// <summary>
/// Configuration options for <see cref="SirrClient"/>.
/// </summary>
public sealed class SirrOptions
{
    /// <summary>
    /// Base URL of the Sirr server. Defaults to <c>https://sirr.sirrlock.com</c>.
    /// </summary>
    public string Server { get; set; } = "https://sirr.sirrlock.com";

    /// <summary>
    /// Bearer token for authentication.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
