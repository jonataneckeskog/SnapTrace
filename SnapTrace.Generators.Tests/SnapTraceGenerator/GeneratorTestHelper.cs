using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class GeneratorTestHelper
{
    private const string AttributeDefinitions = @"
using System;
using System.Diagnostics;

namespace SnapTrace
{
    [Conditional(""SNAPTRACE"")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class SnapTraceAttribute : Attribute { }

    [Conditional(""SNAPTRACE"")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
    public class SnapTraceContextAttribute : Attribute { }

    [Conditional(""SNAPTRACE"")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    public class SnapTraceIgnoreAttribute : Attribute { }

    [Conditional(""SNAPTRACE"")]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, Inherited = false)]
    public class SnapTraceDeepAttribute : Attribute { }
}
";
    public static GeneratorDriverRunResult RunGenerator(string source)
    {
        var parseOptions = CSharpParseOptions.Default.WithPreprocessorSymbols("SNAPTRACE");

        var attributeSyntaxTree = CSharpSyntaxTree.ParseText(AttributeDefinitions, options: parseOptions, path: @"C:\Tests\SnapTraceAttributes.cs");
        var sourceSyntaxTree = CSharpSyntaxTree.ParseText(source, options: parseOptions, path: @"C:\Tests\TestProject.cs");

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.Unsafe).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { sourceSyntaxTree, attributeSyntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
                {
                    // Suppress diagnostics for unused attributes
                    { "CS0169", ReportDiagnostic.Suppress },
                    { "CS0414", ReportDiagnostic.Suppress }
                }));

        var generator = new SnapTrace.Generators.SnapTraceGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);

        return driver.GetRunResult();
    }
}
