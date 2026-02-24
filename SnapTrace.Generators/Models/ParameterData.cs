namespace SnapTrace.Generators.Models;

/// <summary>
/// Model 'collected' by the generator and later send to Builders
/// to handle building the syntax.
/// </summary>
/// <param name="Name"></param>
/// <param name="Type"></param>
/// <param name="Modifier"></param>
/// <param name="IsParams"></param>
/// <param name="DeepCopy"></param>
/// <param name="Redacted"></param>
internal record ParameterData(
    string Name,
    string Type,
    string Modifier,
    bool IsParams,
    bool DeepCopy,
    bool Redacted,
    bool IsNonNullable);
