using System.Security.Cryptography;

namespace BCAS.Api.Helper;

/// <summary>
/// Generates high-entropy, single-use tokens (password reset / invite links).
/// The raw token is only ever handed to the user via email; only its SHA-256
/// hash is stored, so a leaked database never exposes a usable link.
/// </summary>
public static class SecureTokenGenerator
{
    private const int TokenSizeBytes = 32;

    public static string GenerateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenSizeBytes))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    public static string HashToken(string token) =>
        Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
}
