using System.Text;
using SnapTrace.Generators.Builders;

namespace SnapTrace.Generators.Tests.Builders;

public class MethodInterceptorBuilderTests
{
    [Fact]
    public Task Build_WithBaseMethod_GeneratesCorrectly()
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("MyTestClass", "MyTestMethod", default!);

        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        string actual = sb.ToString();

        // Assert
        return Verify(actual);
    }
}
