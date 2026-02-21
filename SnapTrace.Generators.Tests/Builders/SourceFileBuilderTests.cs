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
            .WithClass("MyClass", ClassSituation.None, c => { });

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
            .WithClass("MyClass1", ClassSituation.None, c => { })
            .WithClass("MyClass2", ClassSituation.None, c => { });

        // Act
        var actual = builder.Build();

        // Assert
        return Verify(actual);
    }
}
