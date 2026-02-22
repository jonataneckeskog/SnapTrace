using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class GeneratorTestHelper
{
    public static GeneratorDriverRunResult RunGenerator(string source)
    {
        // 1. Give it a fake absolute path
        var syntaxTree = CSharpSyntaxTree.ParseText(source, path: @"C:\Tests\TestProject.cs");

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.Unsafe).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new SnapTrace.Generators.SnapTraceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);

        // 2. Return the actual result again
        return driver.GetRunResult();
    }
}
