namespace SnapTrace.Core.Attributes;

/// <summary>
/// Forces a deep copy of a reference type to be stored in the trace buffer. Each value
/// which is logged with SnapDeep is required to implement .Clone().
/// <para>
/// When applied to a method, it saves the whole method using a deep copy.
/// When applied to a parameter or return value, it saves only those specific values.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public class SnapTraceDeepAttribute : Attribute { }
