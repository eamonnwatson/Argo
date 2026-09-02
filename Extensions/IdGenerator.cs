using System.Security.Cryptography;

namespace Argo.Extensions;

public static class IdGenerator
{
    // Crockford Base32 alphabet (uppercase only). Excludes I, L, O, U to avoid
    // visual ambiguity with 1, 1, 0, and V respectively.
    private const string Base32Alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

    public static string New(string prefix) => $"{prefix}-{NewSuffix()}";

    // 5 random bytes = 40 bits = exactly eight 5-bit Base32 characters, no padding needed.
    private static string NewSuffix()
    {
        Span<byte> bytes = stackalloc byte[5];
        RandomNumberGenerator.Fill(bytes);
        return Encode(bytes);
    }

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
