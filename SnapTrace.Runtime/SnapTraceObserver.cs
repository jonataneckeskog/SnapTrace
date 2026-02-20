using System.Diagnostics;
using SnapTrace.Runtime.Configuration;
using SnapTrace.Runtime.Internal;
using SnapTrace.Runtime.Models;

namespace SnapTrace.Runtime;

/// <summary>
/// The main entrypoint of SnapTrace. Handles the trace data and provides
/// methods to update it.
/// </summary>
public static class SnapTraceObserver
{
    private static RingBuffer<SnapEntry>? _buffer;
    private static SnapOptions? _options;
    private static SnapEntrySerializer? _snapSerializer;

    private static readonly object _lock = new();
    private static int _isInitializedInt = 0;

    /// <summary>
    /// Initializes the SnapTrace observer. This is the only public API.
    /// </summary>
    [Conditional("SNAPTRACE")]
    public static void Initialize(SnapOptions settings)
    {
        // Use a standard double-checked locking pattern to ensure thread-safe initialization.
        if (_isInitializedInt == 1)
        {
            return;
        }

        lock (_lock)
        {
            if (_isInitializedInt == 1)
            {
                return;
            }

            _options = settings;
            _buffer = new RingBuffer<SnapEntry>(settings.BufferSize);
            _snapSerializer = new SnapEntrySerializer(settings.RecordTimestamp);

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            // The lock provides a memory barrier, ensuring all field assignments above are
            // visible to other threads before the flag is set.
            _isInitializedInt = 1;
        }
    }

    /// <summary>
    /// Dump the trace to the output provided in the options.
    /// </summary>
    private static void Dump()
    {
        if (_isInitializedInt == 0 || _buffer == null || _snapSerializer == null || _options == null) return;

        IEnumerable<string> logs = _buffer.GetLogs().Select(_snapSerializer.Serialize);
        _options?.Output(string.Join(Environment.NewLine, logs));
    }

    /// <summary>
    /// Record a SnapEntry to the buffer safely. Exposed to the generated code via UnsafeAccessor,
    /// while the user cannot directly call it.
    /// </summary>
    /// <param name="entry">The entry to record.</param>
    private static void Record(SnapEntry entry)
    {
        if (_isInitializedInt == 0) return;

        _buffer?.Append(entry);
    }

    /// <summary>
    /// Intercept normal unhandled exceptions. Special error cases (which are unlikely to every occur),
    /// are handled with a 'Fatal' flag.
    /// </summary>
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Record(new SnapEntry(ex.TargetSite?.Name ?? "Unknown", ex.Message, ex.StackTrace, SnapStatus.Error));
        }
        else
        {
            Record(new SnapEntry("Fatal", e.ExceptionObject?.ToString(), null, SnapStatus.Error));
        }

        Dump();
    }

    /// <summary>
    /// Intercept errors that stem from async code.
    /// </summary>
    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        if (e.Exception is Exception ex)
        {
            Record(new SnapEntry(ex.TargetSite?.Name ?? "Unknown", ex.Message, ex.StackTrace, SnapStatus.Error));
        }
        else
        {
            Record(new SnapEntry("Fatal", e.Exception?.ToString(), null, SnapStatus.Error));
        }

        Dump();
    }
}
