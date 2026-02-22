using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SnapTrace.Generators.Extensions;

internal static class SymbolExtensions
{
    public static bool IsUnsafe(this ISymbol symbol)
    {
        return symbol.DeclaringSyntaxReferences
            .Select(syntaxRef => syntaxRef.GetSyntax())
            .Any(syntaxNode => syntaxNode.ChildTokens().Any(token => token.IsKind(SyntaxKind.UnsafeKeyword)));
    }
}
