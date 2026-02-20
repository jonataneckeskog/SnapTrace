using SnapTrace.Core.Runtime;

namespace SnapTrace.Core.Tests.Unit;

public class SnapEntryTests
{
    [Fact]
    public void SnapEntry_ShouldInitializeCorrectly()
    {
        // Arrange
        var methodName = "MyMethod";
        var data = new object();
        var context = new object();
        var status = SnapStatus.Call;

        // Act
        var entry = new SnapEntry(methodName, data, context, status);

        // Assert
        Assert.Equal(methodName, entry.Method);
        Assert.Equal(data, entry.Data);
        Assert.Equal(context, entry.Context);
        Assert.Equal(status, entry.Status);
    }

    [Fact]
    public void SnapEntry_ShouldHaveTimestamp()
    {
        // Arrange
        var entry = new SnapEntry("Method", null, null, SnapStatus.Call);

        // Act
        var timestamp = entry.Timestamp;

        // Assert
        Assert.NotEqual(default, timestamp);
        Assert.True(timestamp < DateTime.UtcNow.AddSeconds(1));
        Assert.True(timestamp > DateTime.UtcNow.AddSeconds(-1));
    }
}
