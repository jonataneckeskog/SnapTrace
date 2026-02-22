using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SnapTrace.Generators.Builders;
using SnapTrace.Generators.Constants;
using SnapTrace.Generators.Definitions;
using SnapTrace.Generators.Models;

namespace SnapTrace.Generators;

[Generator]
public class SnapTraceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Find all invocations of methods that have the SnapTrace attribute
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is InvocationExpressionSyntax,
            transform: static (ctx, ct) => GetInterceptedCall(ctx))
            .Where(static call => call is not null);

        // 2. Collect all valid calls and generate the source
        context.RegisterSourceOutput(
            context.CompilationProvider.Combine(provider.Collect()),
            static (spc, source) => ExecuteGeneration(spc, source.Right!));
    }

    private static InterceptedCall? GetInterceptedCall(GeneratorSyntaxContext ctx)
    {
        var invocation = (InvocationExpressionSyntax)ctx.Node;

        if (ctx.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
            return null;

        bool hasAttribute = HasAttribute(methodSymbol, SnapTraceConstants.SnapTraceAttributeName) ||
                            HasAttribute(methodSymbol.ContainingType, SnapTraceConstants.SnapTraceAttributeName);

        if (!hasAttribute)
            return null;

        var expressionSyntax = invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
            _ => invocation.Expression
        };

        var lineSpan = expressionSyntax.SyntaxTree.GetLineSpan(expressionSyntax.Span);
        if (!lineSpan.IsValid) return null;

        string filePath = expressionSyntax.SyntaxTree.FilePath;
        int line = lineSpan.StartLinePosition.Line + 1;
        int column = lineSpan.StartLinePosition.Character + 1;

        // Build Metadata
        var classData = ExtractClassData(methodSymbol.ContainingType);
        var methodData = ExtractMethodData(methodSymbol);

        return new InterceptedCall(filePath, line, column, methodData, classData);
    }

    private static void ExecuteGeneration(SourceProductionContext spc, ImmutableArray<InterceptedCall> calls)
    {
        if (calls.IsDefaultOrEmpty) return;

        var fileBuilder = new SourceFileBuilder();

        // Grouping: Namespace -> Class -> Method -> Locations
        var byNamespace = calls.GroupBy(c => c.Class.Namespace);

        foreach (var nsGroup in byNamespace)
        {
            fileBuilder.WithNamespace(nsGroup.Key, nsBuilder =>
            {
                var byClass = nsGroup.GroupBy(c => c.Class);
                foreach (var classGroup in byClass)
                {
                    var classInfo = classGroup.Key;
                    nsBuilder.WithClass(classInfo.Name, classInfo.Situation, classBuilder =>
                    {
                        if (!string.IsNullOrEmpty(classInfo.TypeParameters))
                        {
                            classBuilder.WithGenerics(classInfo.TypeParameters, classInfo.WhereConstraints);
                        }

                        foreach (var ctxMember in classInfo.ContextMembers)
                        {
                            classBuilder.AddContextMember(ctxMember.Name, ctxMember.Type);
                        }

                        var byMethod = classGroup.GroupBy(c => c.Method);
                        foreach (var methodGroup in byMethod)
                        {
                            var methodInfo = methodGroup.Key;
                            classBuilder.WithMethod(methodInfo.Name, methodInfo.Situation, methodBuilder =>
                            {
                                methodBuilder.WithReturn(methodInfo.ReturnType, false, false); // Ignored deep/redact for return here

                                if (!string.IsNullOrEmpty(methodInfo.TypeParameters))
                                {
                                    methodBuilder.WithGenerics(methodInfo.TypeParameters, methodInfo.WhereConstraints);
                                }

                                foreach (var p in methodInfo.Parameters)
                                {
                                    methodBuilder.WithParameter(p.Name, p.Type, p.Modifier, p.IsParams, p.DeepCopy, p.Redacted);
                                }

                                foreach (var call in methodGroup)
                                {
                                    methodBuilder.AddLocation(call.FilePath, call.Line, call.Column);
                                }
                            });
                        }
                    });
                }
            });
        }

        spc.AddSource("SnapTrace.Interceptors.g.cs", SourceText.From(fileBuilder.Build(), Encoding.UTF8));
    }

    // --- Helper Methods ---

    private static ClassData ExtractClassData(INamedTypeSymbol typeSymbol)
    {
        var situation = ClassSituation.None;
        if (typeSymbol.IsStatic) situation |= ClassSituation.Static;
        if (typeSymbol.IsValueType) situation |= ClassSituation.IsStruct;
        if (typeSymbol.IsRefLikeType) situation |= ClassSituation.IsRefStruct;

        var ctxMembers = new List<ContextMemberData>();
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IFieldSymbol or IPropertySymbol && HasAttribute(member, SnapTraceConstants.SnapTraceContextAttributeName))
            {
                string memberType = member is IFieldSymbol fs ? fs.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                                                              : ((IPropertySymbol)member).Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                ctxMembers.Add(new ContextMemberData(member.Name, memberType));
            }
        }

        string typeParams = typeSymbol.TypeParameters.Any()
            ? $"<{string.Join(", ", typeSymbol.TypeParameters.Select(t => t.Name))}>"
            : "";

        return new ClassData(
            Namespace: typeSymbol.ContainingNamespace.IsGlobalNamespace ? "" : typeSymbol.ContainingNamespace.ToDisplayString(),
            Name: typeSymbol.Name,
            FullyQualifiedName: typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            Situation: situation,
            TypeParameters: typeParams,
            WhereConstraints: "", // Populate constraints if needed via typeSymbol.TypeParameters bounds
            ContextMembers: ctxMembers
        );
    }

    private static MethodData ExtractMethodData(IMethodSymbol methodSymbol)
    {
        var situation = MethodSituation.None;
        if (methodSymbol.IsStatic) situation |= MethodSituation.Static;
        if (methodSymbol.IsAsync) situation |= MethodSituation.Async;
        if (methodSymbol.ReturnsByRef) situation |= MethodSituation.ReturnsRef;
        if (methodSymbol.ReturnsByRefReadonly) situation |= MethodSituation.ReturnsRefReadonly;

        var parameters = methodSymbol.Parameters.Select(p => new ParameterData(
            Name: p.Name,
            Type: p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            Modifier: p.RefKind switch
            {
                RefKind.Ref => "ref",
                RefKind.Out => "out",
                RefKind.In => "in",
                _ => ""
            },
            IsParams: p.IsParams,
            DeepCopy: false, // Ignored as requested
            Redacted: HasAttribute(p, SnapTraceConstants.SnapTraceIgnoreAttributeName)
        )).ToList();

        string typeParams = methodSymbol.TypeParameters.Any()
            ? $"<{string.Join(", ", methodSymbol.TypeParameters.Select(t => t.Name))}>"
            : "";

        return new MethodData(
            Name: methodSymbol.Name,
            ReturnType: methodSymbol.ReturnsVoid ? "void" : methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            IsVoid: methodSymbol.ReturnsVoid,
            Situation: situation,
            TypeParameters: typeParams,
            WhereConstraints: "", // Add logic if specific constraint rendering is needed
            Parameters: parameters
        );
    }

    private static bool HasAttribute(ISymbol symbol, string fullyQualifiedAttributeName)
    {
        return symbol.GetAttributes().Any(a =>
            a.AttributeClass?.ToDisplayString() == fullyQualifiedAttributeName);
    }
}
