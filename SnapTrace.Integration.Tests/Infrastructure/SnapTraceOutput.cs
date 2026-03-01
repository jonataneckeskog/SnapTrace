using System.Text.Json;

namespace SnapTrace.Integration.Tests.Infrastructure;

public record SnapTraceEntry(string Status, string Method, JsonElement? Data, JsonElement? Context);

public static class SnapTraceOutput
{
    public static List<SnapTraceEntry> Parse(string stdout)
    {
        var entries = new List<SnapTraceEntry>();
        var jsonObjects = ExtractJsonObjects(stdout);

        foreach (var json in jsonObjects)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var status = root.GetProperty("Status").GetString() ?? "";
            var method = root.GetProperty("Method").GetString() ?? "";
            var data = root.TryGetProperty("Data", out var d) ? d.Clone() : (JsonElement?)null;
            var context = root.TryGetProperty("Context", out var c) ? c.Clone() : (JsonElement?)null;

            entries.Add(new SnapTraceEntry(status, method, data, context));
        }

        return entries;
    }

    private static List<string> ExtractJsonObjects(string text)
    {
        var objects = new List<string>();
        var depth = 0;
        var start = -1;

        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            if (ch == '{')
            {
                if (depth == 0) start = i;
                depth++;
            }
            else if (ch == '}')
            {
                depth--;
                if (depth == 0 && start >= 0)
                {
                    objects.Add(text[start..(i + 1)]);
                    start = -1;
                }
            }
        }

        return objects;
    }
}
