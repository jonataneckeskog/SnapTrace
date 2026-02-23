using Microsoft.CodeAnalysis;

namespace SnapTrace.Generators.Models;

/// <summary>
/// Provides the attribute names for the generator to search for.
/// </summary>
/// <param name="TraceAttribute"></param>
/// <param name="ContextAttribute"></param>
/// <param name="IgnoreAttribute"></param>
/// <param name="DeepAttribute"></param>
internal record struct SnapTraceSymbols(
    INamedTypeSymbol? TraceAttribute,
    INamedTypeSymbol? ContextAttribute,
    INamedTypeSymbol? IgnoreAttribute,
    INamedTypeSymbol? DeepAttribute)
{
    /// <summary>
    /// Loads the symbols from the current compilation. These symbols stem from then
    /// string appended at the beginning of the generator.
    /// </summary>
    /// <param name="compilation"></param>
    /// <returns></returns>
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