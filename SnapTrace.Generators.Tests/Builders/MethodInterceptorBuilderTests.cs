using System.Text;
using SnapTrace.Generators.Builders;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Tests.Builders;

public class MethodInterceptorBuilderTests
{
    private string Normalize(string input) => input.Replace("\r\n", "\n").Trim();

    [Fact]
    public void Build_VoidMethod_GeneratesCorrectly()
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("MyNamespace.MyClass", "MyMethod", MethodSituation.None)
            .AddLocation(@"C:\project\File.cs", 10, 20);
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        var actual = Normalize(sb.ToString());

        // Assert
        var expected = Normalize(@"
        [global::System.Runtime.CompilerServices.InterceptsLocation(@""C:\project\File.cs"", 10, 20)]
        public static void Interceptor_MyMethod(this global::MyNamespace.MyClass p_instance)
        {
            var p_context = new { };
            Record(new global::SnapTrace.Runtime.Models.SnapEntry(nameof(MyMethod), ""Enter"", p_context));
            try
            {
                p_instance.MyMethod();
            }
            catch (global::System.Exception e)
            {
                Record(new global::SnapTrace.Runtime.Models.SnapEntry(nameof(MyMethod), ""Exception"", new { Exception = e.ToString() }));
                throw;
            }
            Record(new global::SnapTrace.Runtime.Models.SnapEntry(nameof(MyMethod), ""Exit"", p_context));
        }");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Build_MethodWithParams_GeneratesCorrectly()
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("MyNamespace.MyClass", "MyMethodWithParams", MethodSituation.None)
            .WithParameter("a", "int")
            .WithParameter("b", "string")
            .AddLocation(@"C:\project\File.cs", 15, 25);
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        var actual = Normalize(sb.ToString());

        // Assert
        var expected = Normalize(@"
        [global::System.Runtime.CompilerServices.InterceptsLocation(@""C:\project\File.cs"", 15, 25)]
        public static void Interceptor_MyMethodWithParams(this global::MyNamespace.MyClass p_instance, int a, string b)
        {
            var p_params = new { a, b };
            var p_context = new { };
            Record(new global::SnapTrace.Runtime.Models.SnapEntry(nameof(MyMethodWithParams), ""Enter"", p_context, p_params));
            try
            {
                p_instance.MyMethodWithParams(a, b);
            }
            catch (global::System.Exception e)
            {
                Record(new global::SnapTrace.Runtime.Models.SnapEntry(nameof(MyMethodWithParams), ""Exception"", new { Exception = e.ToString() }));
                throw;
            }
            Record(new global::SnapTrace.Runtime.Models.SnapEntry(nameof(MyMethodWithParams), ""Exit"", p_context));
        }");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Build_MethodWithReturnValue_GeneratesCorrectly()
    {
        // Arrange
        var builder = new MethodInterceptorBuilder("MyNamespace.MyClass", "MyMethodWithReturn", MethodSituation.None)
            .WithReturn("string")
            .AddLocation(@"C:\project\File.cs", 20, 30);
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        var actual = Normalize(sb.ToString());

        // Assert
        var expected = Normalize(@"
        [global::System.Runtime.CompilerServices.InterceptsLocation(@""C:\project\File.cs"", 20, 30)]
        public static string Interceptor_MyMethodWithReturn(this global::MyNamespace.MyClass p_instance)
        {
            var p_context = new { };
            Record(new global::SnapTrace.Runtime.Models.SnapEntry(nameof(MyMethodWithReturn), ""Enter"", p_context));
            string result;
            try
            {
                result = p_instance.MyMethodWithReturn();
            }
            catch (global::System.Exception e)
            {
                Record(new global::SnapTrace.Runtime.Models.SnapEntry(nameof(MyMethodWithReturn), ""Exception"", new { Exception = e.ToString() }));
                throw;
            }
            Record(new global::SnapTrace.Runtime.Models.SnapEntry(nameof(MyMethodWithReturn), ""Exit"", p_context, p_returnValue: result));
            return result;
        }");
        Assert.Equal(expected, actual);
    }
}
