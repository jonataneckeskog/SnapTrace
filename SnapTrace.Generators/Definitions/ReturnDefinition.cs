namespace SnapTrace.Generators.Definitions;

public record ReturnDefinition(
    string Type,
    bool IsVoid,
    bool DeepCopy,
    bool Redacted);
