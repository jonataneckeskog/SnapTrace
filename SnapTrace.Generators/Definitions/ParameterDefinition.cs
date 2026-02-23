namespace SnapTrace.Generators.Definitions;

/// <summary>
/// Represends what the builders need to know for a parameter in a method.
/// </summary>
/// <param name="Name"></param>
/// <param name="Type"></param>
/// <param name="Modifier"></param>
/// <param name="IsParams"></param>
/// <param name="DeepCopy"></param>
/// <param name="Redacted"></param>
public record ParameterDefinition(
    string Name,
    string Type,
    string Modifier = "",
    bool IsParams = false,
    bool DeepCopy = false,
    bool Redacted = false);
