namespace SnapTrace.Generators.Models;

internal record ParameterData(
    string Name,
    string Type,
    string Modifier,
    bool IsParams,
    bool DeepCopy,
    bool Redacted);
