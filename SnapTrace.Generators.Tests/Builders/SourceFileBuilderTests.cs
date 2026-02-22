using SnapTrace.Generators.Builders;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Tests.Builders;

public class SourceFileBuilderTests
{
    [Fact]
    public Task Build_WithOneClass_GeneratesCorrectFile()
    {
        // Arrange
        var builder = new SourceFileBuilder()
            .WithNamespace("MyNamespace", n => { });

        // Act
        var actual = builder.Build();

        // Assert
        return Verify(actual);
    }

    [Fact]
    public Task Build_WithMultipleClasses_GeneratesCorrectFile()
    {
        // Arrange
        var builder = new SourceFileBuilder()
            .WithNamespace("MyNamespace1", n => { })
            .WithNamespace("MyNamespace2", n => { });

        // Act
        var actual = builder.Build();

        // Assert
        return Verify(actual);
    }
}
