using System.Text;
using SnapTrace.Generators.Builders;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Tests.Builders;

public class ClassInterceptorBuilderTests
{
    private string Normalize(string input) => input.Replace("\r\n", "\n").Trim();

    [Fact]
    public void Build_SimpleClass_GeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassInterceptorBuilder("MyNamespace.MyClass", ClassSituation.None);
        builder.WithMethod("MyMethod", MethodSituation.None, m => m.AddLocation(@"C:\project\File.cs", 10, 20));
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        var actual = Normalize(sb.ToString());

        // Assert
        // For this test, we'll just check the class structure, not the full method implementation
        Assert.Contains("file static class MyClassInterceptor", actual);
        Assert.Contains("[global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = \"Record\")]", actual);
        Assert.Contains("private static extern void Record(global::SnapTrace.Runtime.Models.SnapEntry entry);", actual);
        Assert.Contains("[global::System.Runtime.CompilerServices.InterceptsLocation(@\"C:\\project\\File.cs\", 10, 20)]", actual);
    }

    [Fact]
    public void Build_ClassWithContext_GeneratesCorrectly()
    {
        // Arrange
        var builder = new ClassInterceptorBuilder("MyNamespace.MyClass", ClassSituation.None)
            .AddContextMember("_balance")
            .AddContextMember("_user")
            .WithMethod("MyMethod", MethodSituation.None, m => m.AddLocation(@"C:\project\File.cs", 10, 20));
        var sb = new StringBuilder();

        // Act
        builder.InternalBuild(sb);
        var actual = Normalize(sb.ToString());

        // Assert
        var expected = Normalize(@"
        [global::System.Runtime.CompilerServices.InterceptsLocation(@""C:\project\File.cs"", 10, 20)]
        public static void Interceptor_MyMethod(this global::MyNamespace.MyClass p_instance)
        {
            var p_context = new { _balance = p_instance._balance, _user = p_instance._user };
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

        // The context should be injected into the method body
        Assert.Contains("var p_context = new { _balance = p_instance._balance, _user = p_instance._user };", actual);
    }
}
