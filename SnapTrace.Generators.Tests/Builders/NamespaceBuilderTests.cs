using System.CodeDom.Compiler;
using System.Text;
using SnapTrace.Generators.Builders;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Tests.Builders;

public class NamespaceBuilderTests
{
    private string GetGeneratedOutput(Action<IndentedTextWriter> action)
    {
        var sb = new StringBuilder();
        using (var sw = new StringWriter(sb))
        using (var writer = new IndentedTextWriter(sw, "    "))
        {
            action(writer);
        }
        return sb.ToString();
    }

    [Fact]
    public Task Build_NoClasses_GeneratesCorrectly()
    {
        // Arrange
        var builder = new NamespaceBuilder("MyNamespace");

        // Act
        var actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual);
    }

    [Fact]
    public Task Build_OneBasicClass_GeneratesCorrectly()
    {
        // Arrange
        var builder = new NamespaceBuilder("MyNamespace")
            .WithClass("MyClass", ClassSituation.None, c => { });

        // Act
        var actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual);
    }

    [Fact]
    public Task Build_TwoBasicClasses_GeneratesCorrectly()
    {
        // Arrange
        var builder = new NamespaceBuilder("MyNamespace")
            .WithClass("MyClass1", ClassSituation.None, c => { })
            .WithClass("MyClass2", ClassSituation.None, c => { });

        // Act
        var actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual);
    }
}
