using System.Text;
using SnapTrace.Generators.Builders;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Tests.Builders;

public class MethodInterceptorBuilderTests
{
    [Fact]
    public Task Build_WithBaseMethod_GeneratesCorrectly()
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("global::MyNamespace.MyTestClass", "MyTestMethod", default!, default!);

        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        string actual = sb.ToString();

        // Assert
        return Verify(actual);
    }

    [Fact]
    public Task Build_StaticMethod_OnInstanceClass_HasNullContext()
    {
        // Arrange: Class is default (instance), but Method is Static
        var builder = new MethodInterceptorBuilder("global::MyNamespace.MyTestClass", "MyTestMethod", MethodSituation.Static, default!);
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);

        // Assert
        return Verify(sb.ToString());
    }

    [Fact]
    public Task Build_WithGenericMethod_GeneratesCorrectly()
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("global::MyNamespace.MyTestClass", "MyTestMethod", MethodSituation.Generic, default!)
            .WithTypeParameters("<T>")
            .WithWhereConstraints("where T : class");

        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        string actual = sb.ToString();

        // Assert
        return Verify(actual);
    }

    [Fact]
    public Task Build_WithGenericClass_GeneratesCorrectly()
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("global::MyNamespace.MyTestClass<T>", "MyTestMethod", default!, ClassSituation.IsGeneric);
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        string actual = sb.ToString();

        // Assert
        return Verify(actual);
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
        var builder = new MethodInterceptorBuilder("global::MyNamespace.MyTestClass", "MyTestMethod", situation, default!);
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        var actual = sb.ToString();

        // Assert
        return Verify(actual).UseParameters(situation);
    }

    [Theory]
    [InlineData(ClassSituation.Static)]
    [InlineData(ClassSituation.Unsafe)]
    [InlineData(ClassSituation.IsStruct)]
    [InlineData(ClassSituation.IsRefStruct)]
    public Task Build_WithClassSituation_GeneratesCorrectly(ClassSituation situation)
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("global::MyNamespace.MyTestClass", "MyTestMethod", default!, situation);
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        var actual = sb.ToString();

        // Assert
        return Verify(actual).UseParameters(situation);
    }
}
