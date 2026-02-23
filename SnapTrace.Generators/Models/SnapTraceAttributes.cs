using Microsoft.CodeAnalysis;

namespace SnapTrace.Generators.Models;

internal record struct SnapTraceSymbols(
    INamedTypeSymbol? TraceAttribute,
    INamedTypeSymbol? ContextAttribute,
    INamedTypeSymbol? IgnoreAttribute,
    INamedTypeSymbol? DeepAttribute)
{
    public static SnapTraceSymbols Load(Compilation compilation)
    {
        return new SnapTraceSymbols(
            compilation.GetTypeByMetadataName("SnapTrace.SnapTraceAttribute"),
            compilation.GetTypeByMetadataName("SnapTrace.SnapTraceContextAttribute"),
            compilation.GetTypeByMetadataName("SnapTrace.SnapTraceIgnoreAttribute"),
            compilation.GetTypeByMetadataName("SnapTrace.SnapTraceDeepAttribute")
        );
    }

    // A helper to check if any of these failed to load
    public bool IsValid => TraceAttribute is not null;
}