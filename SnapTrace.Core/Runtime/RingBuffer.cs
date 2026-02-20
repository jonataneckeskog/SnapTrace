namespace SnapTrace.Core.Runtime;

/// <summary>
/// A thread-safe, lock-free implementation of a fix-sized FIFO-queue.
/// Must be used with reference types (class) to ensure atomic writes.
/// </summary>
public class RingBuffer<T> where T : class
{
    private readonly int _capacity;
    private readonly T[] _buffer;

    private long _counter = 0;

    public RingBuffer(int capacity)
    {
        _capacity = capacity;
        _buffer = new T[_capacity];
    }

    public void Append(T item)
    {
        long ticket = Interlocked.Increment(ref _counter) - 1;
        int index = (int)(ticket % _capacity);
        _buffer[index] = item;
    }

    /// <summary>
    /// Oldest -> Newest (Standard timeline)
    /// </summary>
    public IEnumerable<T> GetLogs()
    {
        long currentCounter = Interlocked.Read(ref _counter);
        int count = (int)Math.Min(currentCounter, _capacity);

        long startTicket = currentCounter > _capacity ? currentCounter - _capacity : 0;

        for (int i = 0; i < count; i++)
        {
            int index = (int)((startTicket + i) % _capacity);
            var item = _buffer[index];
            if (item != null) yield return item;
        }
    }

    /// <summary>
    /// Newest -> Oldest
    /// </summary>
    public IEnumerable<T> GetLogsReversed()
    {
        long currentCounter = Interlocked.Read(ref _counter);
        int count = (int)Math.Min(currentCounter, _capacity);

        long startTicket = currentCounter - 1;

        for (int i = 0; i < count; i++)
        {
            int index = (int)((startTicket - i) % _capacity);
            var item = _buffer[index];
            if (item != null) yield return item;
        }
    }

    /// <summary>
    /// Returns the current size of the buffer.
    /// </summary>
    public int Count
    {
        get
        {
            long currentCounter = Interlocked.Read(ref _counter);
            return (int)Math.Min(currentCounter, _capacity);
        }
    }

    /// <summary>
    /// Clears the buffer. Note: In a highly concurrent lock-free system, 
    /// clearing while active writers are running is inherently a "best effort".
    /// </summary>
    public void Clear()
    {
        Array.Clear(_buffer, 0, _capacity);
        Interlocked.Exchange(ref _counter, 0);
    }
}
