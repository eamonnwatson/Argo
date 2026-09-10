using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Argo.Domain.Common;

/// <summary>
/// Serializes/deserializes single-value record structs (e.g. ProjectId, UserId) as { "Value": "..." },
/// bypassing the type's validating factory since the value is trusted (already-persisted domain data).
/// Requires the type to expose a static "FromTrustedValue(string)" factory method.
/// </summary>
internal sealed class SingleValueJsonConverter<T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var value = doc.RootElement.GetProperty("Value").GetString();

        var factory = typeToConvert.GetMethod("FromTrustedValue",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            ?? throw new InvalidOperationException($"Type {typeToConvert} must define a static FromTrustedValue(string) factory method.");

        return (T)factory.Invoke(null, [value])!;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var valueProp = typeof(T).GetProperty("Value")!.GetValue(value);
        writer.WriteStartObject();
        writer.WriteString("Value", (string?)valueProp);
        writer.WriteEndObject();
    }
}
