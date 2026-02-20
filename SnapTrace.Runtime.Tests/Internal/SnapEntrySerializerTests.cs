using SnapTrace.Runtime.Internal;
using SnapTrace.Runtime.Models;
using System.Text.Json;

namespace SnapTrace.Runtime.Tests.Internal;

public class SnapEntrySerializerTests
{
    [Fact]
    public void Serialize_ShouldIncludeTimestamp_WhenConfigured()
    {
        // Arrange
        var serializer = new SnapEntrySerializer(true);
        var entry = new SnapEntry("TestMethod", null, null, SnapStatus.Call);

        // Act
        var json = serializer.Serialize(entry);

        // Assert
        Assert.Contains("\"Timestamp\":", json);
    }

    [Fact]
    public void Serialize_ShouldNotIncludeTimestamp_WhenNotConfigured()
    {
        // Arrange
        var serializer = new SnapEntrySerializer(false);
        var entry = new SnapEntry("TestMethod", null, null, SnapStatus.Call);

        // Act
        var json = serializer.Serialize(entry);

        // Assert
        Assert.DoesNotContain("\"Timestamp\":", json);
    }

    [Fact]
    public void Serialize_ShouldFormatCorrectly()
    {
        // Arrange
        var serializer = new SnapEntrySerializer(false);
        var entry = new SnapEntry("TestMethod", new { Arg = "Value" }, new { Ctx = "Info" }, SnapStatus.Return);

        // Act
        var json = serializer.Serialize(entry);

        // Assert
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("Return", root.GetProperty("Status").GetString());
        Assert.Equal("TestMethod", root.GetProperty("Method").GetString());
        Assert.Equal("Value", root.GetProperty("Data").GetProperty("Arg").GetString());
        Assert.Equal("Info", root.GetProperty("Context").GetProperty("Ctx").GetString());
    }

    public class BadSerializable
    {
        public string ThrowingProperty => throw new InvalidOperationException("This property will always throw.");
    }

    [Fact]
    public void Serialize_ShouldHandleSerializationErrorsGracefully()
    {
        // Arrange
        var serializer = new SnapEntrySerializer(false);
        var badData = new BadSerializable();
        var entry = new SnapEntry("TestMethod", badData, null, SnapStatus.Error);

        // Act
        var json = serializer.Serialize(entry);

        // Assert
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("Error", root.GetProperty("Status").GetString());
        Assert.Equal("TestMethod", root.GetProperty("Method").GetString());
        Assert.Equal(badData.ToString(), root.GetProperty("Data").GetString());
        Assert.True(root.TryGetProperty("SerializationError", out var errorNode));
        Assert.True(errorNode.TryGetProperty("DataError", out _));
    }

    [Fact]
    public void Serialize_ShouldHandleNestedStructuresAndHeterogeneousArrays()
    {
        // Arrange
        var serializer = new SnapEntrySerializer(false);

        // A complex anonymous object containing an array of different types (polymorphic)
        var complexData = new
        {
            Identifier = "Session_99",
            Metrics = new object?[]
            {
                    "StringValue",
                    404,
                    new { InnerKey = "InnerValue", IsActive = true },
                    null
            }
        };
        var entry = new SnapEntry("ProcessMetrics", complexData, null, SnapStatus.Call);

        // Act
        var json = serializer.Serialize(entry);

        // Assert
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var dataElement = root.GetProperty("Data");

        // Validate root property
        Assert.Equal("Session_99", dataElement.GetProperty("Identifier").GetString());

        // Validate the heterogeneous array
        var metricsArray = dataElement.GetProperty("Metrics");
        Assert.Equal(4, metricsArray.GetArrayLength());
        Assert.Equal("StringValue", metricsArray[0].GetString());
        Assert.Equal(404, metricsArray[1].GetInt32());

        // Validate the deeply nested object inside the array
        var nestedObject = metricsArray[2];
        Assert.Equal("InnerValue", nestedObject.GetProperty("InnerKey").GetString());
        Assert.True(nestedObject.GetProperty("IsActive").GetBoolean());

        // Validate null handling in array
        Assert.Equal(JsonValueKind.Null, metricsArray[3].ValueKind);
    }

    [Fact]
    public void Serialize_ShouldHandleDictionariesWithCollectionValues()
    {
        // Arrange
        var serializer = new SnapEntrySerializer(false);

        // Dictionaries are common in Context objects for custom tracing states
        var complexContext = new Dictionary<string, object>
            {
                { "TenantId", "Tenant_ABC" },
                { "ActiveRoles", new[] { "Admin", "Contributor" } },
                { "FeatureFlags", new Dictionary<string, bool> { { "UseNewUI", true } } }
            };

        var entry = new SnapEntry("ValidateAccess", null, complexContext, SnapStatus.Return);

        // Act
        var json = serializer.Serialize(entry);

        // Assert
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var contextElement = root.GetProperty("Context");

        // Validate top-level dictionary keys
        Assert.Equal("Tenant_ABC", contextElement.GetProperty("TenantId").GetString());

        // Validate array inside dictionary
        var rolesArray = contextElement.GetProperty("ActiveRoles");
        Assert.Equal(2, rolesArray.GetArrayLength());
        Assert.Equal("Admin", rolesArray[0].GetString());
        Assert.Equal("Contributor", rolesArray[1].GetString());

        // Validate nested dictionary
        var flagsDict = contextElement.GetProperty("FeatureFlags");
        Assert.True(flagsDict.GetProperty("UseNewUI").GetBoolean());
    }
}
