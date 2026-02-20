namespace SnapTrace.Core.Runtime;

/// <summary>
/// A logged entry for the trace-buffer. Saves only references, so
/// no serialization is required.
/// </summary>
/// <param name="Method"></param>
/// <param name="Args"></param>
/// <param name="Context"></param>
public readonly record struct SnapEntry(string Method, object? Args, object? Context)
{
    /// <summary>
    /// The timestamp at which the method was logged
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
