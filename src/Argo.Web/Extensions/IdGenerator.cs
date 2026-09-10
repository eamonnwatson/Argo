using System.Security.Cryptography;

namespace Argo.Extensions;

/// <summary>
/// Generates compact identifier values composed of an entity prefix and a random base-32 suffix.
/// </summary>
public static class IdGenerator
{
    private const string Base32Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

    /// <summary>
    /// Creates a new identifier in the format <c>{prefix}-{suffix}</c>.
    /// </summary>
    /// <param name="prefix">The entity prefix, such as <c>PRJ</c> or <c>WI</c>.</param>
    /// <returns>A new identifier string with a cryptographically random suffix.</returns>
    public static string New(string prefix) => $"{prefix}-{NewSuffix()}";

    /// <summary>
    /// Generates a random 5-byte payload and encodes it as an 8-character base-32 string.
    /// </summary>
    /// <returns>An encoded random suffix suitable for user-visible identifiers.</returns>
    private static string NewSuffix()
    {
        Span<byte> bytes = stackalloc byte[5];
        RandomNumberGenerator.Fill(bytes);
        return Encode(bytes);
    }

    /// <summary>
    /// Encodes binary data using the custom base-32 alphabet expected by Argo identifiers.
    /// </summary>
    /// <param name="bytes">The binary payload to encode.</param>
    /// <returns>An 8-character base-32 representation of <paramref name="bytes"/>.</returns>
    private static string Encode(ReadOnlySpan<byte> bytes)
    {
        Span<char> chars = stackalloc char[8];
        ulong buffer = 0;
        for (var i = 0; i < bytes.Length; i++)
            buffer = (buffer << 8) | bytes[i];

        for (var i = 7; i >= 0; i--)
        {
            chars[i] = Base32Alphabet[(int)(buffer & 0x1F)];
            buffer >>= 5;
        }

        return new string(chars);
    }
}
