using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class GeneratorTestHelper
{
    public static GeneratorDriverRunResult RunGenerator(string source)
{
    var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols("SNAPTRACE");

    // 1. Parse the user's source
    var sourceSyntaxTree = CSharpSyntaxTree.ParseText(source, options: parseOptions, path: @"C:\Tests\TestProject.cs");

    // 2. ALSO parse your static "PostInitialization" sources so the compiler knows they exist
    var attributeTree = CSharpSyntaxTree.ParseText(SnapTrace.Generators.Constants.AttributeDefinitions.Definitions, options: parseOptions);
    var clonerTree = CSharpSyntaxTree.ParseText(SnapTrace.Generators.Constants.GeneratorUtils.SnapCloner, options: parseOptions);

    var references = new[]
    {
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.ITuple).Assembly.Location), // Vital for the Cloner!
        MetadataReference.CreateFromFile(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory() + "mscorlib.dll"),
        MetadataReference.CreateFromFile(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory() + "System.Runtime.dll"),
    };

    // 3. Create compilation with the static definitions included
    var compilation = CSharpCompilation.Create(
        assemblyName: "Tests",
        syntaxTrees: new[] { sourceSyntaxTree, attributeTree, clonerTree }, // Include them here!
        references: references,
        options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    var generator = new SnapTrace.Generators.SnapTraceGenerator();

    GeneratorDriver driver = CSharpGeneratorDriver.Create(generator)
                                                 .WithUpdatedParseOptions(parseOptions);
    
    driver = driver.RunGenerators(compilation);

    return driver.GetRunResult();
}
}
