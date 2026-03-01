namespace SnapTrace.Generators.Models;

internal record ParameterModel(
    string Name,
    string Type,
    string Modifier,
    bool IsParams,
    bool DeepCopy,
    bool Redacted,
    bool IsNonNullable);
