using System.CodeDom.Compiler;
using System.Text;
using SnapTrace.Generators.Builders;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Tests.Builders;

public class ClassInterceptorBuilderTests
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
    public Task Build_SimpleClass_GeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassInterceptorBuilder("global::MyNamespace", "MyClass", ClassSituation.None);

        // Act
        var actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual);
    }

    [Fact]
    public Task Build_WithOneMethod_GeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassInterceptorBuilder("global::MyNamespace", "MyClass", ClassSituation.None)
            .WithMethod("MyMethod", MethodSituation.None, m => { });

        // Act
        var actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual);
    }

    [Fact]
    public Task Build_WithTwoMethods_GeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassInterceptorBuilder("global::MyNamespace", "MyClass", ClassSituation.None)
            .WithMethod("MyMethod1", MethodSituation.None, m => { })
            .WithMethod("MyMethod2", MethodSituation.None, m => { });

        // Act
        var actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual);
    }

    [Fact]
    public Task Build_WithContext_GeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassInterceptorBuilder("global::MyNamespace", "MyClass", ClassSituation.None)
            .AddContextMember("_balance", "double")
            .AddContextMember("_name", "string");

        // Act
        var actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual);
    }

    [Fact]
    public Task Build_WithGenerics_GeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassInterceptorBuilder("global::MyNamespace", "MyClass", ClassSituation.None)
            .WithGenerics("<T>", "where T : class");

        // Act
        var actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual);
    }

    [Theory]
    [InlineData(ClassSituation.Static)]
    [InlineData(ClassSituation.Unsafe)]
    [InlineData(ClassSituation.IsStruct)]
    [InlineData(ClassSituation.IsRefStruct)]
    public Task Build_WithSituation_GeneratesCorrectly(ClassSituation situation)
    {
        // Arrange
        var builder = new ClassInterceptorBuilder("global::MyNamespace", "MyClass", situation);

        // Act
        var actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual).UseParameters(situation);
    }
}
