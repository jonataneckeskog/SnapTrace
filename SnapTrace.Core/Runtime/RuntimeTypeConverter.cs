using System.Text.Json;
using System.Text.Json.Serialization;

namespace SnapTrace.Core.Runtime;

/// <summary>
/// Forces System.Text.Json to serialize 'object' properties using their actual runtime type.
/// </summary>
internal class RuntimeTypeConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException("Deserialization is not required for SnapTrace.");

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        var type = value.GetType();

        // Safeguard to prevent infinite recursion if the object is literally a raw `new object()`
        if (type == typeof(object))
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
            return;
        }

        try
        {
            // Serialize using the actual runtime type (e.g., Customer, String, Exception)
            JsonSerializer.Serialize(writer, value, type, options);
        }
        catch
        {
            // If it's a completely unserializable type (like a Reflection pointer or an open Stream),
            // gracefully degrade to its string representation so the trace doesn't crash.
            writer.WriteStringValue(value.ToString());
        }
    }
}
