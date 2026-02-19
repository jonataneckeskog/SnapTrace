namespace SnapTrace.Core.Runtime;

/// <summary>
/// A logged entry for the trace-buffer. Saves only references, so
/// no serialization is required.
/// </summary>
/// <param name="method"></param>
/// <param name="args"></param>
/// <param name="context"></param>
/// <param name="result"></param>
public readonly struct SnapEntry(string method, object? args, object? context, object? result)
{
    /// <summary>
    /// The name of the traced method
    /// </summary>
    public readonly string Method = method;

    /// <summary>
    /// The arguments provided to the method, if provided. Could point to
    /// an array of many objects if there are several parameters.
    /// </summary>
    public readonly object? Args = args;

    /// <summary>
    /// The context (fields and properties) that is 'Snap-Recorded' alongside
    /// other data
    /// </summary>
    public readonly object? Context = context;

    /// <summary>
    /// The return value of the method
    /// </summary>
    public readonly object? Result = result;

    /// <summary>
    /// The timestamp at which the method was logged
    /// </summary>
    public readonly DateTime Timestamp = DateTime.UtcNow;
}
