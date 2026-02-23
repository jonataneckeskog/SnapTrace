using System.CodeDom.Compiler;
using System.Text;
using SnapTrace.Generators.Builders;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Tests.Builders;

public class MethodInterceptorBuilderTests
{
    private string GetGeneratedOutput(Action<IndentedTextWriter, string> action, string targetType = "global::MyNamespace.MyTestClass")
    {
        var sb = new StringBuilder();
        using (var sw = new StringWriter(sb))
        using (var writer = new IndentedTextWriter(sw, "    "))
        {
            action(writer, targetType);
        }
        return sb.ToString();
    }

    [Fact]
    public Task Build_WithBaseMethod_GeneratesCorrectly()
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("MyTestMethod", default!, default!);

        // Act
        string actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual, "cs");
    }

    [Fact]
    public Task Build_StaticMethod_OnInstanceClass_HasNullContext()
    {
        // Arrange: Class is default (instance), but Method is Static
        var builder = new MethodInterceptorBuilder("MyTestMethod", MethodSituation.Static, default!);

        // Act
        string actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual, "cs");
    }

    [Fact]
    public Task Build_WithGenericMethod_GeneratesCorrectly()
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("MyTestMethod", MethodSituation.None, default!)
            .WithGenerics("<T>", "where T : class");

        // Act
        string actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual, "cs");
    }

    [Fact]
    public Task Build_WithGenericClass_GeneratesCorrectly()
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("MyTestMethod", default!, ClassSituation.None);

        // Act: Passing a generic type string
        string targetType = "global::MyNamespace.MyTestClass<T>";
        string actual = GetGeneratedOutput(builder.InternalBuild, targetType);

        // Assert
        return Verify(actual, "cs");
    }

    [Theory]
    [InlineData(MethodSituation.Async)]
    [InlineData(MethodSituation.Static)]
    [InlineData(MethodSituation.Unsafe)]
    [InlineData(MethodSituation.ReturnsRef)]
    [InlineData(MethodSituation.ReturnsRefReadonly)]
    public Task Build_WithMethodSituation_GeneratesCorrectly(MethodSituation situation)
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("MyTestMethod", situation, default!);

        // Act
        var actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual, "cs").UseParameters(situation);
    }

    [Theory]
    [InlineData(ClassSituation.Static)]
    [InlineData(ClassSituation.Unsafe)]
    [InlineData(ClassSituation.IsStruct)]
    [InlineData(ClassSituation.IsRefStruct)]
    public Task Build_WithClassSituation_GeneratesCorrectly(ClassSituation situation)
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("MyTestMethod", default!, situation);

        // Act
        var actual = GetGeneratedOutput(builder.InternalBuild);

        // Assert
        return Verify(actual, "cs").UseParameters(situation);
    }
}
