namespace SnapTrace.Generators.Definitions;

public record ParameterDefinition(
    string Name,
    string Type,
    string Modifier = "",
    bool IsParams = false,
    bool DeepCopy = false,
    bool Redacted = false);
