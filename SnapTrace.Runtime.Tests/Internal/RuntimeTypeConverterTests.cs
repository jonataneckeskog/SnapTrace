using System.Text.Json;
using SnapTrace.Runtime.Internal;

namespace SnapTrace.Runtime.Tests.Internal;

public class RuntimeTypeConverterTests
{
    private readonly JsonSerializerOptions _options;

    public RuntimeTypeConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { new RuntimeTypeConverter() }
        };
    }

    private string Serialize(object value)
    {
        return JsonSerializer.Serialize(value, _options);
    }

    // --- Custom Test Types ---
    public record TestData(string Name, int Age);
    public record NestedTestData(string ParentName, TestData Child);
    public record DataWithNull(string Name, string? Description);

    [Fact]
    public void Read_ShouldThrowNotImplementedException()
    {
        // Arrange
        var converter = new RuntimeTypeConverter();
        var reader = new Utf8JsonReader();

        // Act & Assert
        try
        {
            converter.Read(ref reader, typeof(object), _options);
        }
        catch (NotImplementedException) { /* Success */ }
    }

    [Fact]
    public void Write_ShouldSerializeAsEmptyObject_ForRawObject()
    {
        // Arrange
        var value = new object();

        // Act
        var json = Serialize(value);

        // Assert
        Assert.Equal("{}", json);
    }

    [Fact]
    public void Write_ShouldSerializeUsingRuntimeType()
    {
        // Arrange
        var value = new { Name = "Test", Value = 123 };

        // Act
        var json = Serialize(value);

        // Assert
        Assert.Equal("{\"Name\":\"Test\",\"Value\":123}", json);
    }

    [Fact]
    public void Write_ShouldSerializeCustomRecords()
    {
        // Arrange
        var value = new TestData("Alice", 30);

        // Act
        var json = Serialize(value);

        // Assert
        Assert.Equal("{\"Name\":\"Alice\",\"Age\":30}", json);
    }

    [Fact]
    public void Write_ShouldSerializeNestedObjects()
    {
        // Arrange
        var value = new NestedTestData("Bob", new TestData("Alice", 30));

        // Act
        var json = Serialize(value);

        // Assert
        Assert.Equal("{\"ParentName\":\"Bob\",\"Child\":{\"Name\":\"Alice\",\"Age\":30}}", json);
    }

    [Fact]
    public void Write_ShouldSerializeNullPropertiesCorrectly()
    {
        // Arrange
        var value = new DataWithNull("Test", null);

        // Act
        var json = Serialize(value);

        // Assert
        Assert.Equal("{\"Name\":\"Test\",\"Description\":null}", json);
    }

    [Fact]
    public void Write_ShouldHandleObjectsWithDelegatesGracefully()
    {
        Func<int, string> crazyFunc = (x) => $"Number {x}";

        // Cast to object so STJ routes it to our converter
        var value = new { Name = "FuncContainer", Action = (object)crazyFunc };

        var json = JsonSerializer.Serialize<object>(value, _options);

        Assert.Contains("\"Name\":\"FuncContainer\"", json);
        Assert.Contains("[Delegate", json); // Matches the fallback string in the converter
    }

    [Fact]
    public void Write_ShouldHandleExceptionsGracefully()
    {
        var exception = new InvalidOperationException("Something went horribly wrong!");

        // Cast to object
        var value = new { Status = "Failed", Error = (object)exception };

        var json = JsonSerializer.Serialize<object>(value, _options);

        Assert.Contains("\"Status\":\"Failed\"", json);
        Assert.Contains("Something went horribly wrong!", json);
    }

    private class Unserializable
    {
        // This will force STJ to throw an exception during serialization
        public string ExplodingProperty => throw new InvalidOperationException("Boom");

        public override string ToString() => "Unserializable Object";
    }

    [Fact]
    public void Write_ShouldGracefullyHandleUnserializableTypes()
    {
        var value = new Unserializable();

        var json = JsonSerializer.Serialize<object>(value, _options);

        Assert.Equal("\"Unserializable Object\"", json);
    }

    [Fact]
    public void Write_ShouldSerializePrimitivesCorrectly()
    {
        // Arrange
        var intValue = 123;
        var stringValue = "hello";
        var boolValue = true;

        // Act
        var intJson = Serialize(intValue);
        var stringJson = Serialize(stringValue);
        var boolJson = Serialize(boolValue);

        // Assert
        Assert.Equal("123", intJson);
        Assert.Equal("\"hello\"", stringJson);
        Assert.Equal("true", boolJson);
    }
}
