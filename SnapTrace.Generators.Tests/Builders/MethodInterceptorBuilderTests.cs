using System.Text;
using SnapTrace.Generators.Builders;

namespace SnapTrace.Generators.Tests.Builders;

public class MethodInterceptorBuilderTests
{
    [Fact]
    public Task Verify_Builder_State()
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("MyTestClass", "MyTestMethod", default!)
            .WithReturn("string", deepCopy: true)
            .WithTypeParameters("T")
            .WithWhereConstraints("where T : class")
            .WithParameter("id", "int", modifier: "ref")
            .AddLocation("C:\\Project\\File.cs", 15, 30);

        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        string actual = sb.ToString();

        // Assert
        return Verify(actual);
    }
}
