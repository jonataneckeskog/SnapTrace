using SnapTrace.Core.Runtime;
using System.Linq;
using System.Threading.Tasks;

namespace SnapTrace.Core.Tests.Unit;

public class RingBufferTests
{
    private record TraceItem(string Value);

    [Fact]
    public void Append_ShouldIncreaseCount_WhenBelowCapacity()
    {
        // Arrange
        var buffer = new RingBuffer<TraceItem>(3);

        // Act
        buffer.Append(new("1"));
        buffer.Append(new("2"));

        // Assert
        Assert.Equal(2, buffer.Count);
    }

    [Fact]
    public void Append_ShouldNotExceedCapacity_WhenBufferIsFull()
    {
        // Arrange
        var buffer = new RingBuffer<TraceItem>(2);

        // Act
        buffer.Append(new("1"));
        buffer.Append(new("2"));
        buffer.Append(new("3"));

        // Assert
        Assert.Equal(2, buffer.Count);
    }

    [Fact]
    public void GetLogs_ShouldReturnItemsInInsertionOrder()
    {
        // Arrange
        var buffer = new RingBuffer<string>(3);
        buffer.Append("A");
        buffer.Append("B");
        buffer.Append("C");

        // Act
        var logs = buffer.GetLogs().ToList();

        // Assert
        Assert.Equal(new[] { "A", "B", "C" }, logs);
    }

    [Fact]
    public void GetLogs_ShouldHandleWrapAround_Correctly()
    {
        // Arrange
        var buffer = new RingBuffer<string>(3);
        buffer.Append("1");
        buffer.Append("2");
        buffer.Append("3");
        buffer.Append("4"); // Overwrites "1"

        // Act
        var logs = buffer.GetLogs().ToList();

        // Assert
        Assert.Equal(new[] { "2", "3", "4" }, logs);
    }

    [Fact]
    public void GetLogsReversed_ShouldReturnItemsInReverseInsertionOrder()
    {
        // Arrange
        var buffer = new RingBuffer<string>(3);
        buffer.Append("1");
        buffer.Append("2");
        buffer.Append("3");

        // Act
        var logs = buffer.GetLogsReversed().ToList();

        // Assert
        Assert.Equal(new[] { "3", "2", "1" }, logs);
    }

    [Fact]
    public void GetLogsReversed_ShouldHandleWrapAround_Correctly()
    {
        // Arrange
        var buffer = new RingBuffer<string>(3);
        buffer.Append("1");
        buffer.Append("2");
        buffer.Append("3");
        buffer.Append("4"); // Overwrites "1"

        // Act
        var logs = buffer.GetLogsReversed().ToList();

        // Assert
        Assert.Equal(new[] { "4", "3", "2" }, logs);
    }

    [Fact]
    public void Clear_ShouldResetBuffer()
    {
        // Arrange
        var buffer = new RingBuffer<string>(3);
        buffer.Append("1");
        buffer.Append("2");

        // Act
        buffer.Clear();

        // Assert
        Assert.Equal(0, buffer.Count);
        Assert.Empty(buffer.GetLogs());
    }

    [Fact]
    public void Concurrency_ShouldNotCorruptState_UnderHeavyLoad()
    {
        // Arrange
        const int capacity = 100;
        const int iterations = 1000;
        var buffer = new RingBuffer<string>(capacity);

        // Act
        // Simulate multiple threads
        Parallel.For(0, iterations, i =>
        {
            buffer.Append($"Trace {i}");
        });

        // Assert
        // 1. The count should be capped at capacity
        Assert.Equal(capacity, buffer.Count);

        // 2. We should be able to iterate without any NullReferenceExceptions or corruption
        var logs = buffer.GetLogs().ToList();
        Assert.Equal(capacity, logs.Count);
        Assert.All(logs, item => Assert.NotNull(item));
    }
}
