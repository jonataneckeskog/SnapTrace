namespace SnapTrace.Generators.Tests.SnapTraceGenerator;

public class SnapTraceGeneratorTests
{
    [Fact]
    public async Task Generates_Interceptor_For_SnapTraced_Method()
    {
        // Arrange: Your test class as a string, including the required attributes.
        var source = @"
using System;
using SnapTrace;

namespace SnapTrace
{
    public class SnapTraceAttribute : Attribute { }
    public class SnapTraceContextAttribute : Attribute { }
    public class SnapTraceIgnoreAttribute : Attribute { }
}

namespace TestApp
{
    public class MyService
    {
        [SnapTrace]
        public void DoWork(string input)
        {
            Console.WriteLine(input);
        }
    }

    public class Program
    {
        public static void Main()
        {
            var service = new MyService();
            service.DoWork(""Hello""); // This invocation should trigger the generator
        }
    }
}";

        // Act: Run the generator
        var result = GeneratorTestHelper.RunGenerator(source);

        // Assert: Verify the generated code
        Assert.NotEmpty(result.GeneratedTrees);

        var generatedCode = result.GeneratedTrees[0].GetText().ToString();
        await Verify(generatedCode, "cs");
    }

    [Fact]
    public async Task Generates_Context_Accessors_For_Traced_Members()
    {
        var source = @"
using SnapTrace;

namespace SnapTrace {
    public class SnapTraceAttribute : System.Attribute { }
    public class SnapTraceContextAttribute : System.Attribute { }
}

namespace TestApp {
    public class UserService {
        [SnapTraceContext]
        private string _userId = ""12345""; // This should generate an UnsafeAccessor

        [SnapTrace]
        public void UpdateUser(string name) { }
    }

    public class Program {
        public static void Main() {
            new UserService().UpdateUser(""Alice"");
        }
    }
}";

        // Act
        var result = GeneratorTestHelper.RunGenerator(source);
        var generatedCode = result.GeneratedTrees[0].GetText().ToString();

        // Assert
        await Verify(generatedCode, "cs");
    }
}
