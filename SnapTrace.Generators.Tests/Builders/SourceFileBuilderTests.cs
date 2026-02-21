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
            .WithClass("MyClass", ClassSituation.None, c => c.WithMethod("MyMethod", MethodSituation.None, m =>
            {
                m.AddLocation("C:\\Project\\File.cs", 15, 30);
                m.WithParameter("id", "int", modifier: "ref");
                m.WithReturn("string", deepCopy: true);
            }));

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
            .WithClass("MyClass1", ClassSituation.None, c => c.WithMethod("MyMethod1", MethodSituation.None, m =>
            {
                m.AddLocation("C:\\Project\\File.cs", 15, 30);
                m.WithParameter("id", "int", modifier: "ref");
                m.WithReturn("string", deepCopy: true);
            }))
            .WithClass("MyClass2", ClassSituation.None, c => c.WithMethod("MyMethod2", MethodSituation.Unsafe, m =>
            {
                m.AddLocation("C:\\Project\\File.cs", 16, 30);
                m.WithParameter("password", "int");
            }));

        // Act
        var actual = builder.Build();

        // Assert
        return Verify(actual);
    }
}
