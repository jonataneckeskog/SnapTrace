using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SnapTrace.Core.Runtime;

/// <summary>
/// Forces System.Text.Json to serialize 'object' properties using their actual runtime type,
/// degrading gracefully to string representations when standard serialization fails.
/// </summary>
internal class RuntimeTypeConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException("Deserialization is not required for SnapTrace.");

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        Type type = value.GetType();

        if (type == typeof(object))
        {
            writer.WriteStartObject();
            writer.WriteEndObject();
            return;
        }

        if (typeof(Delegate).IsAssignableFrom(type))
        {
            writer.WriteStringValue($"[Delegate: {type.Name}]");
            return;
        }

        if (typeof(MemberInfo).IsAssignableFrom(type) || typeof(Type).IsAssignableFrom(type))
        {
            writer.WriteStringValue($"[Reflection: {value}]");
            return;
        }

        try
        {
            // Serialize to a temporary, in-memory JSON document first.
            // If this crashes midway, the main 'writer' remains completely untouched and clean.
            using var doc = JsonSerializer.SerializeToDocument(value, type, options);
            doc.WriteTo(writer);
        }
        catch
        {
            WriteSafeFallback(writer, value);
        }
    }

    private void WriteSafeFallback(Utf8JsonWriter writer, object value)
    {
        try
        {
            string stringValue = value.ToString() ?? "null";
            writer.WriteStringValue(stringValue);
        }
        catch
        {
            writer.WriteStringValue($"[Unserializable Object of type {value.GetType().Name}]");
        }
    }
}
