namespace SnapTrace.Core.Configuration;

/// <summary>
/// Configuration options for the SnapTrace ring buffer and output handling.
/// </summary>
/// <param name="BufferSize">The maximum number of method calls to retain in the ring buffer.</param>
/// <param name="RecordTimestamp">If true, prepends a timestamp to each recorded frame.</param>
/// <param name="Output">The action to execute when dumping the trace (e.g., writing to a log or console).</param>
public record struct SnapOptions(int BufferSize, bool RecordTimestamp, Action<string> Output);
