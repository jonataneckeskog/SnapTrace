namespace SnapTrace.Core.Attributes;

/// <summary>
/// Marks a field or property to be captured as context with every intercepted method call in the containing class.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class SnapTraceContextAttribute : Attribute { }
