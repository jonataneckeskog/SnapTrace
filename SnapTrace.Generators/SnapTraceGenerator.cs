using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SnapTrace.Generators.Definitions;
using SnapTrace.Generators.Processing;

namespace SnapTrace.Generators;

[Generator(LanguageNames.CSharp)]
public class SnapTraceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .WithTrackingName("ClassDeclarations");

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        IncrementalValuesProvider<ClassToGenerate> validClassesToGenerate = compilationAndClasses.SelectMany((source, ct) =>
        {
            var compilation = source.Left;
            var classes = source.Right;
            var results = ImmutableArray.CreateBuilder<ClassToGenerate>();

            foreach (var classDeclaration in classes)
            {
                ct.ThrowIfCancellationRequested();
                var classToGenerate = SnapTraceParser.GetSemanticTargetForGeneration(classDeclaration, compilation, ct);
                if (classToGenerate is not null)
                {
                    results.Add(classToGenerate);
                }
            }
            return results.ToImmutable();
        })
        .WithTrackingName("ValidClasses");

        context.RegisterSourceOutput(validClassesToGenerate,
            static (spc, source) => SnapTraceEmitter.Emit(spc, source));
    }
}
