using System.Text.Json;
using System.Text.Json.Nodes;
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
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Converters = { new JsonStringEnumConverter(), new RuntimeTypeConverter() }
        };
        _includeTime = includeTime;
    }

    /// <summary>
    /// Serialize a SnapEntry to a string representation for logging.
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public string Serialize(SnapEntry entry)
    {
        // Use JsonObject for dynamic property building
        var rootNode = new JsonObject
        {
            ["Status"] = entry.Status.ToString(),
            ["Method"] = entry.Method
        };

        if (_includeTime)
        {
            rootNode["Timestamp"] = entry.Timestamp;
        }

        // Try to serialize Data and Context, capturing any errors
        JsonNode? dataNode = SerializeNode(entry.Data, out var dataError);
        JsonNode? contextNode = SerializeNode(entry.Context, out var contextError);

        rootNode["Data"] = dataNode;
        rootNode["Context"] = contextNode;

        if (dataError != null || contextError != null)
        {
            var errors = new JsonObject();
            if (dataError != null)
            {
                errors["DataError"] = dataError;
            }
            if (contextError != null)
            {
                errors["ContextError"] = contextError;
            }
            rootNode["SerializationError"] = errors;
        }

        return rootNode.ToJsonString(_options);
    }

    private JsonNode? SerializeNode(object? value, out string? errorMessage)
    {
        errorMessage = null;
        if (value == null)
        {
            return null;
        }

        try
        {
            // Using JsonSerializer.SerializeToNode to convert the object to a JsonNode
            return JsonSerializer.SerializeToNode(value, value.GetType(), _options);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            // Fallback to a string representation if serialization fails
            return JsonValue.Create(value.ToString());
        }
    }
}
