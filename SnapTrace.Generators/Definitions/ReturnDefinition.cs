namespace SnapTrace.Generators.Definitions;

/// <summary>
/// Represents what the builders need to know for the return type of a method.
/// </summary>
/// <param name="Type"></param>
/// <param name="IsVoid"></param>
/// <param name="DeepCopy"></param>
/// <param name="Redacted"></param>
public record ReturnDefinition(
    string Type,
    bool IsVoid,
    bool DeepCopy,
    bool Redacted);
