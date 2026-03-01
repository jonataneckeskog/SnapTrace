namespace SnapTrace.Generators.Tests.SnapTraceGenerator;

using System.Runtime.CompilerServices;

public class SnapTraceGeneratorTests
{
    private string LoadTestData(string fileName, [CallerFilePath] string sourceFilePath = "")
    {
        var testFileDirectory = Path.GetDirectoryName(sourceFilePath);

        var path = Path.Combine(testFileDirectory ?? @"", "Resources", fileName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Still can't find it! Looked at: {path}");
        }

        return File.ReadAllText(path);
    }

    [Fact]
    public async Task Generates_Interceptor_For_SnapTraced_Method()
    {
        var source = LoadTestData("TestFile1.cs");

        var result = GeneratorTestHelper.RunGenerator(source);

        Assert.NotEmpty(result.GeneratedTrees);

        var generatedTree = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.EndsWith("SnapTrace.TestApp.MyService.g.cs"));
        Assert.NotNull(generatedTree);

        var generatedCode = generatedTree.GetText().ToString();
        await Verify(generatedCode, "cs");
    }

    [Fact]
    public async Task Generates_Context_Accessors_For_Traced_Members()
    {
        var source = LoadTestData("TestFile2.cs");

        var result = GeneratorTestHelper.RunGenerator(source);

        var generatedTree = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.EndsWith("SnapTrace.TestApp.UserService.g.cs"));
        Assert.NotNull(generatedTree);

        var generatedCode = generatedTree.GetText().ToString();

        await Verify(generatedCode, "cs");
    }

    [Fact]
    public async Task GeneratesUniqueLocations_ForMethod()
    {
        var source = LoadTestData("MultipleMethodCalls.cs");

        var result = GeneratorTestHelper.RunGenerator(source);

        var generatedTree = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.EndsWith("SnapTrace.TestApp.MyService.g.cs"));
        Assert.NotNull(generatedTree);

        var generatedCode = generatedTree.GetText().ToString();

        await Verify(generatedCode, "cs");
    }
}
