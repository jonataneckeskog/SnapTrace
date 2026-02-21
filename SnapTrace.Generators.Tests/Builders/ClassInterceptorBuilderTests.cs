using System.Text;
using SnapTrace.Generators.Builders;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Tests.Builders;

public class ClassInterceptorBuilderTests
{
    [Fact]
    public Task Build_SimpleClass_GeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassInterceptorBuilder("MyClass", ClassSituation.None);
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        var actual = sb.ToString();

        // Assert
        return Verify(actual);
    }

    [Fact]
    public Task Build_WithOneMethod_GeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassInterceptorBuilder("MyClass", ClassSituation.None)
            .WithMethod("MyMethod", MethodSituation.None, m => { });
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        var actual = sb.ToString();

        // Assert
        return Verify(actual);
    }

    [Fact]
    public Task Build_WithTwoMethods_GeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassInterceptorBuilder("MyClass", ClassSituation.None)
            .WithMethod("MyMethod1", MethodSituation.None, m => { })
            .WithMethod("MyMethod2", MethodSituation.None, m => { });
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        var actual = sb.ToString();

        // Assert
        return Verify(actual);
    }

    [Fact]
    public Task Build_ClassWithContext_GeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassInterceptorBuilder("MyClass", ClassSituation.None)
            .AddContextMember("_balance")
            .AddContextMember("_name");
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        var actual = sb.ToString();

        // Assert
        return Verify(actual);
    }

    [Fact]
    public Task Build_ClassWithGenerics_GeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassInterceptorBuilder("MyClass", ClassSituation.IsGeneric)
            .WithTypeParameters("<T>")
            .WithWhereConstraints("where T : new()");
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        var actual = sb.ToString();

        // Assert
        return Verify(actual);
    }
}
