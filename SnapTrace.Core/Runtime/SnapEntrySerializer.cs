using System.Text.Json;
using System.Text.Json.Serialization;

namespace SnapTrace.Core.Runtime;

internal class SnapEntrySerializer
{
    private readonly JsonSerializerOptions _options;
    private readonly bool _includeTime;

    public SnapEntrySerializer(bool includeTime)
    {
        _options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

            // Ignore reference loops
            ReferenceHandler = ReferenceHandler.IgnoreCycles,

            // Serialize Enums as their string names
            Converters = { new JsonStringEnumConverter(), new RuntimeTypeConverter() }
        };
        _includeTime = includeTime;
    }

    /// <summary>
    /// Serialize a SnapEntry to a string.
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public string Serialize(SnapEntry entry)
    {
        // Projecting into an anonymous object guarantees the exact property order:
        // Status first, then Method, etc.
        var envelope = new
        {
            Status = entry.Status,
            Method = entry.Method,
            Timestamp = _includeTime ? (DateTime?)entry.Timestamp : null,
            Data = entry.Data,
            Context = entry.Context
        };

        try
        {
            return JsonSerializer.Serialize(envelope, _options);
        }
        catch (Exception ex)
        {
            // FALLBACK: If serialization catastrophically fails (e.g., unsupported unmanaged types in args),
            // we still want to log the entry without taking down the host application.
            var fallbackEnvelope = new
            {
                Status = entry.Status,
                Method = entry.Method,
                Timestamp = _includeTime ? (DateTime?)entry.Timestamp : null,
                Data = entry.Data?.ToString(),
                Context = entry.Context?.ToString(),
                SerializationError = ex.Message
            };

            return JsonSerializer.Serialize(fallbackEnvelope);
        }
    }
}
