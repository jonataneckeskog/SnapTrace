namespace SnapTrace.Core.Runtime;

/// <summary>
/// A simple generic implementation of a fix-sized FIFO-queue, here
/// called RingBuffer.
/// </summary>
/// <typeparam name="T"></typeparam>
public class RingBuffer<T>
{
    private readonly int _capacity;
    private readonly T[] _buffer;
    private int _nextIndex = 0;
    private int _count = 0;

    public RingBuffer(int capacity)
    {
        _capacity = capacity;
        _buffer = new T[_capacity];
    }

    public void Append(T item)
    {
        _buffer[_nextIndex] = item;
        _nextIndex = (_nextIndex + 1) % _capacity;

        if (_count < _capacity)
        {
            _count++;
        }
    }

    /// <summary>
    /// Oldest -> Newest (Standard timeline)
    /// </summary>
    public IEnumerable<T> GetLogs()
    {
        int start = _count < _capacity ? 0 : _nextIndex;
        for (int i = 0; i < _count; i++)
        {
            yield return _buffer[(start + i) % _capacity];
        }
    }

    /// <summary>
    /// Newest -> Oldest
    /// </summary>
    public IEnumerable<T> GetLogsReversed()
    {
        for (int i = 1; i <= _count; i++)
        {
            int index = (_nextIndex - i + _capacity) % _capacity;
            yield return _buffer[index];
        }
    }

    /// <summary>
    /// Returns the current size of the buffer.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Clears all elements in the RingBuffer.
    /// </summary>
    public void Clear()
    {
        Array.Clear(_buffer, 0, _capacity);
        _nextIndex = 0;
        _count = 0;
    }
}
