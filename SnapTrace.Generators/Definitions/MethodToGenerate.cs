using System.Collections.Immutable;

namespace SnapTrace.Generators.Definitions;

internal record MethodToGenerate(
    string Name,
    MethodSituation Situation,
    ImmutableArray<ParameterDefinition> Parameters,
    ReturnDefinition ReturnDefinition,
    string? GenericConstraints);
