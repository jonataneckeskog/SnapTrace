using System.Collections.Immutable;

namespace SnapTrace.Generators.Definitions;

internal record ClassToGenerate(
    string fullyQualifiedName,
    string className,
    string @namespace,
    ClassSituation situation,
    string? typeParameters,
    string? whereConstraints,
    ImmutableArray<MethodToGenerate> methods,
    ImmutableArray<ContextMemberToGenerate> contextMembers);
