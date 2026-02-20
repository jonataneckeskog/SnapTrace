using SnapTrace.Runtime.Configuration;

namespace SnapTrace.Runtime.Tests.Configuration;

public class SnapOptionsTests
{
    [Fact]
    public void SnapOptions_ShouldInitializeCorrectly()
    {
        // Arrange
        const int bufferSize = 100;
        const bool recordTimestamp = true;
        Action<string> outputAction = (s) => { };

        // Act
        var options = new SnapOptions(bufferSize, recordTimestamp, outputAction);

        // Assert
        Assert.Equal(bufferSize, options.BufferSize);
        Assert.Equal(recordTimestamp, options.RecordTimestamp);
        Assert.Equal(outputAction, options.Output);
    }

    [Fact]
    public void SnapOptions_ShouldBeStructAndEquatable()
    {
        // Arrange
        Action<string> outputAction1 = (s) => { };
        Action<string> outputAction2 = (s) => { };

        var options1 = new SnapOptions(100, true, outputAction1);
        var options2 = new SnapOptions(100, true, outputAction1);
        var options3 = new SnapOptions(200, true, outputAction1);
        var options4 = new SnapOptions(100, false, outputAction1);
        var options5 = new SnapOptions(100, true, outputAction2);

        // Assert
        Assert.Equal(options1, options2);
        Assert.NotEqual(options1, options3);
        Assert.NotEqual(options1, options4);
        Assert.NotEqual(options1, options5);
    }
}
