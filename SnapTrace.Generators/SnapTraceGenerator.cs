using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using SnapTrace.Generators.Builders;
using SnapTrace.Generators.Definitions;
using SnapTrace.Generators.Models;
using Microsoft.CodeAnalysis.CSharp;
using SnapTrace.Generators.Constants;
using System;

namespace SnapTrace.Generators;

[Generator]
public class SnapTraceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource(
                "SnapTrace.Attributes.g.cs",
                SourceText.From(AttributeDefinitions.Definitions, Encoding.UTF8));
        });
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource(
                "SnapTrace.SnapCloner.g.cs",
                SourceText.From(GeneratorUtils.SnapCloner, Encoding.UTF8));
        });

        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (node, _) => node is InvocationExpressionSyntax,
            transform: static (ctx, ct) => GetInterceptedCall(ctx, ct))
            .Where(static call => call is not null);

        var disableSignal = context.AnalyzerConfigOptionsProvider.Select(static (options, _) =>
        {
            options.GlobalOptions.TryGetValue("build_property.SnapTraceDisable", out var isDisabledStr);
            return string.Equals(isDisabledStr, "true", StringComparison.OrdinalIgnoreCase);
        });

        var combinedProvider = context.CompilationProvider
            .Combine(provider.Collect())
            .Combine(disableSignal);

        context.RegisterSourceOutput(
            combinedProvider,
            static (spc, source) =>
            {
                var isDisabled = source.Right;

                if (isDisabled)
                {
                    return;
                }

                var compilationAndCalls = source.Left;
                var calls = compilationAndCalls.Right;

                ExecuteGeneration(spc, calls!);
            });
    }

    private static InterceptedCall? GetInterceptedCall(GeneratorSyntaxContext ctx, System.Threading.CancellationToken ct)
    {
        var invocation = (InvocationExpressionSyntax)ctx.Node;
        var semanticModel = ctx.SemanticModel;

        // Resolve Symbols for the current compilation
        var symbols = SnapTraceSymbols.Load(semanticModel.Compilation);
        if (!symbols.IsValid) return null;

        if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
            return null;

        bool hasAttribute = HasAttribute(methodSymbol, symbols.TraceAttribute) ||
                            HasAttribute(methodSymbol.ContainingType, symbols.TraceAttribute);

        if (!hasAttribute)
            return null;

#pragma warning disable RSEXPERIMENTAL002

        var interceptableLocation = semanticModel.GetInterceptableLocation(invocation, ct);
        if (interceptableLocation is null) return null;

        string interceptorAttributeString = interceptableLocation.GetInterceptsLocationAttributeSyntax();

#pragma warning restore RSEXPERIMENTAL002

        // ----------------------------------------

        // Build Metadata
        var classData = ExtractClassData(methodSymbol.ContainingType, symbols);
        var methodData = ExtractMethodData(methodSymbol, symbols);

        return new InterceptedCall(
            interceptorAttributeString,
            methodData,
            classData
        );
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
                                methodBuilder.WithReturn(
                                    methodInfo.ReturnType,
                                    methodInfo.DeepCopyReturn,
                                    methodInfo.RedactedReturn
                                );

                                if (!string.IsNullOrEmpty(methodInfo.TypeParameters))
                                {
                                    methodBuilder.WithGenerics(methodInfo.TypeParameters, methodInfo.WhereConstraints);
                                }

                                foreach (var p in methodInfo.Parameters)
                                {
                                    methodBuilder.WithParameter(p.Name, p.Type, p.Modifier, p.IsParams, p.DeepCopy, p.Redacted, p.IsNonNullable);
                                }

                                foreach (var call in methodGroup)
                                {
                                    methodBuilder.AddLocation(call.InterceptorAttributeString);
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

    private static ClassData ExtractClassData(INamedTypeSymbol typeSymbol, SnapTraceSymbols symbols)
    {
        var situation = ClassSituation.None;
        if (typeSymbol.IsStatic) situation |= ClassSituation.Static;
        if (typeSymbol.IsValueType) situation |= ClassSituation.IsStruct;
        if (typeSymbol.IsRefLikeType) situation |= ClassSituation.IsRefStruct;

        var ctxMembers = new List<ContextMemberData>();
        foreach (var member in typeSymbol.GetMembers())
        {
            if (member is IFieldSymbol or IPropertySymbol && HasAttribute(member, symbols.ContextAttribute))
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

    private static MethodData ExtractMethodData(IMethodSymbol methodSymbol, SnapTraceSymbols symbols)
    {
        var situation = MethodSituation.None;
        if (methodSymbol.IsStatic) situation |= MethodSituation.Static;
        if (methodSymbol.IsAsync) situation |= MethodSituation.Async;
        if (methodSymbol.ReturnsByRef) situation |= MethodSituation.ReturnsRef;
        if (methodSymbol.ReturnsByRefReadonly) situation |= MethodSituation.ReturnsRefReadonly;

        // 1. Check Return Attributes
        bool returnRedacted = HasAttribute(methodSymbol, symbols.IgnoreAttribute) ||
                             methodSymbol.GetReturnTypeAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, symbols.IgnoreAttribute));

        // 2. Determine if the return value should be DeepCopied
        bool returnDeepCopy = !methodSymbol.ReturnsVoid &&
                             !methodSymbol.ReturnType.IsValueType &&
                             methodSymbol.ReturnType.SpecialType != SpecialType.System_String;

        var parameters = new List<ParameterData>();
        foreach (var param in methodSymbol.Parameters)
        {
            // Check for SnapTraceDeep attribute
            bool paramDeepCopy = param.GetAttributes().Any(attr => 
                SymbolEqualityComparer.Default.Equals(attr.AttributeClass, symbols.DeepAttribute));
            
            // Check for SnapTraceIgnore attribute
            bool paramRedacted = param.GetAttributes().Any(attr => 
                SymbolEqualityComparer.Default.Equals(attr.AttributeClass, symbols.IgnoreAttribute));
            bool isNonNullable = param.NullableAnnotation == NullableAnnotation.NotAnnotated;

            string paramType = param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            string modifier = param.RefKind switch
            {
                RefKind.Out => "out ",
                RefKind.Ref => "ref ",
                RefKind.RefReadOnly => "ref readonly ",
                _ => ""
            };

            parameters.Add(new ParameterData(
                param.Name,
                paramType,
                modifier,
                param.IsParams,
                paramDeepCopy,
                paramRedacted,
                isNonNullable
            ));
        }

        string typeParams = methodSymbol.TypeParameters.Any()
            ? $"<{string.Join(", ", methodSymbol.TypeParameters.Select(t => t.Name))}>"
            : "";

        return new MethodData(
            Name: methodSymbol.Name,
            ReturnType: methodSymbol.ReturnsVoid ? "void" : methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            IsVoid: methodSymbol.ReturnsVoid,
            Situation: situation,
            TypeParameters: typeParams,
            WhereConstraints: "",
            Parameters: parameters,
            DeepCopyReturn: returnDeepCopy,
            RedactedReturn: returnRedacted
        );
    }

    private static bool HasAttribute(ISymbol symbol, INamedTypeSymbol? attributeSymbol)
    {
        if (attributeSymbol is null) return false;

        return symbol.GetAttributes().Any(a =>
            SymbolEqualityComparer.Default.Equals(a.AttributeClass, attributeSymbol));
    }
}
