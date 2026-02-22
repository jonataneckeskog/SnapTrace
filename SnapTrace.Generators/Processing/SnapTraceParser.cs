using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SnapTrace.Generators.Constants;
using SnapTrace.Generators.Definitions;
using SnapTrace.Generators.Extensions;

namespace SnapTrace.Generators.Processing;

internal static class SnapTraceParser
{
    public static ClassToGenerate? GetSemanticTargetForGeneration(ClassDeclarationSyntax classDeclaration, Compilation compilation, CancellationToken cancellationToken)
    {
        var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
        if (semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) is not INamedTypeSymbol classSymbol)
        {
            return null;
        }

        if (!classSymbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceConstants.SnapTraceAttributeName))
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
            .Where(m => !m.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceConstants.SnapTraceIgnoreAttributeName))
            .Select(m => GetMethodToGenerate(m, compilation))
            .ToImmutableArray();

        var contextMembers = classSymbol.GetMembers()
            .Where(m => m.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceConstants.SnapTraceContextAttributeName))
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
            var deepCopy = p.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceConstants.SnapTraceDeepAttributeName);
            var redacted = p.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceConstants.SnapTraceIgnoreAttributeName);

            return new ParameterDefinition(p.Name, p.Type.ToDisplayString(), modifier, p.IsParams, deepCopy, redacted);
        }).ToImmutableArray();

        var returnTypeAttributes = methodSymbol.GetReturnTypeAttributes();
        var returnsDeepCopy = returnTypeAttributes.Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceConstants.SnapTraceDeepAttributeName);
        var returnsRedacted = returnTypeAttributes.Any(a => a.AttributeClass?.ToDisplayString() == SnapTraceConstants.SnapTraceIgnoreAttributeName);

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
        if (classSymbol.IsUnsafe()) situation |= ClassSituation.Unsafe;
        return situation;
    }

    private static MethodSituation GetMethodSituation(IMethodSymbol methodSymbol)
    {
        var situation = MethodSituation.None;
        if (methodSymbol.IsStatic) situation |= MethodSituation.Static;
        if (methodSymbol.IsAsync) situation |= MethodSituation.Async;
        if (methodSymbol.IsUnsafe()) situation |= MethodSituation.Unsafe;
        if (methodSymbol.ReturnsByRef) situation |= MethodSituation.ReturnsRef;
        if (methodSymbol.ReturnsByRefReadonly) situation |= MethodSituation.ReturnsRefReadonly;
        return situation;
    }
}
