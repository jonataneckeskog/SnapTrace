using SnapTrace.Core.Runtime;

namespace SnapTrace.Core.Tests.Unit;

public class RingBufferTests
{
    [Fact]
    public void Append_ShouldIncreaseCount_WhenBelowCapacity()
    {
        // Arrange
        var buffer = new RingBuffer<int>(3);

        // Act
        buffer.Append(1);
        buffer.Append(2);

        // Assert
        Assert.Equal(2, buffer.Count);
    }

    [Fact]
    public void Append_ShouldNotExceedCapacity_WhenBufferIsFull()
    {
        // Arrange
        var buffer = new RingBuffer<int>(2);

        // Act
        buffer.Append(1);
        buffer.Append(2);
        buffer.Append(3);

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
        var logs = buffer.GetLogs();

        // Assert
        Assert.Equal(["A", "B", "C"], logs);
    }

    [Fact]
    public void GetLogs_ShouldHandleWrapAround_Correctly()
    {
        // Arrange
        var buffer = new RingBuffer<int>(3);
        buffer.Append(1);
        buffer.Append(2);
        buffer.Append(3);
        buffer.Append(4); // Overwrites 1

        // Act
        var logs = buffer.GetLogs();

        // Assert
        // Expected: 2, 3, 4
        Assert.Equal([2, 3, 4], logs);
    }

    [Fact]
    public void GetLogsReversed_ShouldReturnItemsInReverseInsertionOrder()
    {
        // Arrange
        var buffer = new RingBuffer<int>(3);
        buffer.Append(1);
        buffer.Append(2);
        buffer.Append(3);

        // Act
        var logs = buffer.GetLogsReversed();

        // Assert
        Assert.Equal([3, 2, 1], logs);
    }

    [Fact]
    public void GetLogsReversed_ShouldHandleWrapAround_Correctly()
    {
        // Arrange
        var buffer = new RingBuffer<int>(3);
        buffer.Append(1);
        buffer.Append(2);
        buffer.Append(3);
        buffer.Append(4); // Overwrites 1. Buffer state: [4, 2, 3] (indices 0, 1, 2). Next index: 1.

        // Act
        var logs = buffer.GetLogsReversed();

        // Assert
        // Expected: 4, 3, 2
        Assert.Equal([4, 3, 2], logs);
    }

    [Fact]
    public void Clear_ShouldResetBuffer()
    {
        // Arrange
        var buffer = new RingBuffer<int>(3);
        buffer.Append(1);
        buffer.Append(2);

        // Act
        buffer.Clear();

        // Assert
        Assert.Equal(0, buffer.Count);
        Assert.Empty(buffer.GetLogs());
    }

    [Fact]
    public void Append_AfterClear_ShouldWorkCorrectly()
    {
        // Arrange
        var buffer = new RingBuffer<int>(3);
        buffer.Append(1);
        buffer.Append(2);
        buffer.Clear();

        // Act
        buffer.Append(3);
        buffer.Append(4);

        // Assert
        Assert.Equal(2, buffer.Count);
        Assert.Equal([3, 4], buffer.GetLogs());
    }
}
