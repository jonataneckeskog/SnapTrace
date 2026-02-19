namespace SnapTrace.Core.Attributes;

/// <summary>
/// Excludes a specific target from the trace.
/// <para>
/// When applied to a method, it opts out of a class-wide <see cref="SnapTraceAttribute"/>.
/// When applied to a parameter, it redacts the value from the trace log (e.g., for PII).
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public class SnapTraceIgnoreAttribute : Attribute { }
