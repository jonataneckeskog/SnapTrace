using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SnapTrace.Generators.Builders;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators;

[Generator(LanguageNames.CSharp)]
public class SnapTraceGenerator : IIncrementalGenerator
{
    private const string SnapTraceAttributeName = "SnapTrace.SnapTraceAttribute";
    private const string SnapTraceContextAttributeName = "SnapTrace.SnapTraceContextAttribute";
    private const string SnapTraceIgnoreAttributeName = "SnapTrace.SnapTraceIgnoreAttribute";
    private const string SnapTraceDeepAttributeName = "SnapTrace.SnapTraceDeepAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .WithTrackingName("ClassDeclarations");

        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        IncrementalValuesProvider<ClassToGenerate?> classesToGenerate = compilationAndClasses.SelectMany((source, ct) =>
        {
            var compilation = source.Left;
            var classes = source.Right;
            var results = ImmutableArray.CreateBuilder<ClassToGenerate?>();

            foreach (var classDeclaration in classes)
            {
                ct.ThrowIfCancellationRequested();
                results.Add(GetSemanticTargetForGeneration(classDeclaration, compilation, ct));
            }
            return results.ToImmutable();
        }).WithTrackingName("SemanticTarget");

        IncrementalValuesProvider<ClassToGenerate> validClassesToGenerate = classesToGenerate
            .Where(static m => m is not null)
            .Select(static (m, _) => m!)
            .WithTrackingName("ValidClasses");

        context.RegisterSourceOutput(validClassesToGenerate,
            static (spc, source) => Execute(source, spc));
    }

    private static ClassToGenerate? GetSemanticTargetForGeneration(ClassDeclarationSyntax classDeclaration, Compilation compilation, CancellationToken cancellationToken)
    {
        var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
        if (semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        if (!classSymbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceAttributeName))
        {
            return null;
        }

        string namespaceName = classSymbol.ContainingNamespace.IsGlobalNamespace
            ? ""
            : classSymbol.ContainingNamespace.ToDisplayString();

        var classSituation = GetClassSituation(classSymbol);

        var methods = classSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary && m.IsImplicitlyDeclared == false && m.DeclaredAccessibility == Accessibility.Public)
            .Where(m => !m.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceIgnoreAttributeName))
            .Select(m => GetMethodToGenerate(m, compilation))
            .ToImmutableArray();

        var contextMembers = classSymbol.GetMembers()
            .Where(m => m.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceContextAttributeName))
            .OfType<ISymbol>()
            .Select(GetContextMemberToGenerate)
            .ToImmutableArray();
        
        string? typeParameters = null;
        string? whereConstraints = null;
        if (classSymbol.IsGenericType)
        {
            typeParameters = $"<{string.Join(", ", classSymbol.TypeParameters.Select(tp => tp.Name))}>";

            var constraints = new StringBuilder();
            foreach (var tp in classSymbol.TypeParameters)
            {
                var fullConstraints = tp.ConstraintTypes.Select(c => c.ToDisplayString())
                    .Concat(tp.HasReferenceTypeConstraint ? new[] { "class" } : Enumerable.Empty<string>())
                    .Concat(tp.HasValueTypeConstraint ? new[] { "struct" } : Enumerable.Empty<string>())
                    .Concat(tp.HasConstructorConstraint ? new[] { "new()" } : Enumerable.Empty<string>())
                    .ToImmutableArray();
                
                if (fullConstraints.Any())
                {
                    constraints.Append($" where {tp.Name} : {string.Join(", ", fullConstraints)}");
                }
            }
            whereConstraints = constraints.ToString();
        }

        return new ClassToGenerate(
            fullyQualifiedName: classSymbol.ToDisplayString(),
            className: classSymbol.Name,
            @namespace: namespaceName,
            situation: classSituation,
            typeParameters: typeParameters,
            whereConstraints: whereConstraints,
            methods: methods,
            contextMembers: contextMembers);
    }

    private static MethodToGenerate GetMethodToGenerate(IMethodSymbol methodSymbol, Compilation compilation)
    {
        var parameters = methodSymbol.Parameters.Select(p =>
        {
            var modifier = p.RefKind switch
            {
                RefKind.Ref => "ref",
                RefKind.Out => "out",
                RefKind.In => "in",
                _ => string.Empty
            };
            var deepCopy = p.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceDeepAttributeName);
            var redacted = p.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceIgnoreAttributeName);

            return new ParameterDefinition(p.Name, p.Type.ToDisplayString(), modifier, p.IsParams, deepCopy, redacted);
        }).ToImmutableArray();
        
        var returnTypeAttributes = methodSymbol.GetReturnTypeAttributes();
        var returnsDeepCopy = returnTypeAttributes.Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceDeepAttributeName);
        var returnsRedacted = returnTypeAttributes.Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceIgnoreAttributeName);

        var returnDefinition = new ReturnDefinition(
            methodSymbol.ReturnType.ToDisplayString(),
            methodSymbol.ReturnsVoid,
            returnsDeepCopy,
            returnsRedacted);

        var situation = GetMethodSituation(methodSymbol);

        return new MethodToGenerate(methodSymbol.Name, situation, parameters, returnDefinition, GetGenericConstraints(methodSymbol));
    }
    
    private static string? GetGenericConstraints(IMethodSymbol methodSymbol)
    {
        if (!methodSymbol.IsGenericMethod)
        {
            return null;
        }

        var constraints = new StringBuilder();
        foreach (var tp in methodSymbol.TypeParameters)
        {
            var fullConstraints = tp.ConstraintTypes.Select(c => c.ToDisplayString())
                .Concat(tp.HasReferenceTypeConstraint ? new[] { "class" } : Enumerable.Empty<string>())
                .Concat(tp.HasValueTypeConstraint ? new[] { "struct" } : Enumerable.Empty<string>())
                .Concat(tp.HasConstructorConstraint ? new[] { "new()" } : Enumerable.Empty<string>())
                .ToImmutableArray();
            
            if (fullConstraints.Any())
            {
                constraints.Append($" where {tp.Name} : {string.Join(", ", fullConstraints)}");
            }
        }
        return constraints.Length > 0 ? constraints.ToString().Trim() : null;
    }


    private static ContextMemberToGenerate GetContextMemberToGenerate(ISymbol symbol)
    {
        return symbol switch
        {
            IFieldSymbol field => new ContextMemberToGenerate(field.Name, field.Type.ToDisplayString()),
            IPropertySymbol prop => new ContextMemberToGenerate(prop.Name, prop.Type.ToDisplayString()),
            _ => throw new InvalidOperationException("Unsupported context member type")
        };
    }

    private static ClassSituation GetClassSituation(INamedTypeSymbol classSymbol)
    {
        var situation = ClassSituation.None;
        if (classSymbol.IsStatic) situation |= ClassSituation.Static;
        if (classSymbol.IsValueType) situation |= ClassSituation.IsStruct;
        if (classSymbol.IsRefLikeType) situation |= ClassSituation.IsRefStruct;
        if (classSymbol.IsGenericType) situation |= ClassSituation.IsGeneric;
        if (classSymbol.IsUnsafe()) situation |= ClassSituation.Unsafe;
        return situation;
    }

    private static MethodSituation GetMethodSituation(IMethodSymbol methodSymbol)
    {
        var situation = MethodSituation.None;
        if (methodSymbol.IsStatic) situation |= MethodSituation.Static;
        if (methodSymbol.IsAsync) situation |= MethodSituation.Async;
        if (methodSymbol.IsGenericMethod) situation |= MethodSituation.Generic;
        if (methodSymbol.IsUnsafe()) situation |= MethodSituation.Unsafe;
        if (methodSymbol.ReturnsByRef) situation |= MethodSituation.ReturnsRef;
        if (methodSymbol.ReturnsByRefReadonly) situation |= MethodSituation.ReturnsRefReadonly;
        return situation;
    }

    private static void Execute(ClassToGenerate classToGenerate, SourceProductionContext context)
    {
        var sourceFileBuilder = new SourceFileBuilder();
        sourceFileBuilder.WithNamespace(classToGenerate.@namespace, nsBuilder =>
        {
            nsBuilder.WithClass(classToGenerate.className, classToGenerate.situation, classBuilder =>
            {
                if (classToGenerate.typeParameters is not null)
                {
                    classBuilder.WithTypeParameters(classToGenerate.typeParameters);
                }
                
                if (classToGenerate.whereConstraints is not null)
                {
                    classBuilder.WithWhereConstraints(classToGenerate.whereConstraints);
                }
                
                foreach (var member in classToGenerate.contextMembers)
                {
                    classBuilder.AddContextMember(member.Name, member.Type);
                }

                foreach (var method in classToGenerate.methods)
                {
                    classBuilder.WithMethod(method.Name, method.Situation, methodBuilder =>
                    {
                        if (method.GenericConstraints is not null)
                        {
                            methodBuilder.WithWhereConstraints(method.GenericConstraints);
                        }

                        foreach (var p in method.Parameters)
                        {
                            methodBuilder.WithParameter(p.Name, p.Type, p.Modifier, p.IsParams, p.DeepCopy, p.Redacted);
                        }

                        methodBuilder.WithReturn(method.ReturnDefinition.Type, method.ReturnDefinition.DeepCopy, method.ReturnDefinition.Redacted);
                    });
                }
            });
        });

        var sourceCode = sourceFileBuilder.Build();
        context.AddSource($"{classToGenerate.className}.g.cs", sourceCode);
    }
}

internal static class SymbolExtensions
{
    public static bool IsUnsafe(this ISymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences
            .Select(syntaxRef => syntaxRef.GetSyntax())
            .Any(syntaxNode => syntaxNode.ChildTokens().Any(token => token.IsKind(SyntaxKind.UnsafeKeyword)));
    }
}

internal record ClassToGenerate(
    string fullyQualifiedName,
    string className,
    string @namespace,
    ClassSituation situation,
    string? typeParameters,
    string? whereConstraints,
    ImmutableArray<MethodToGenerate> methods,
    ImmutableArray<ContextMemberToGenerate> contextMembers);

internal record MethodToGenerate(
    string Name,
    MethodSituation Situation,
    ImmutableArray<ParameterDefinition> Parameters,
    ReturnDefinition ReturnDefinition,
    string? GenericConstraints);
    
internal record ContextMemberToGenerate(string Name, string Type);
