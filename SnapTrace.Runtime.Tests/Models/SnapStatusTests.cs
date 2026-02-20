using SnapTrace.Runtime.Models;

namespace SnapTrace.Runtime.Tests.Models;

public class SnapStatusTests
{
    [Fact]
    public void SnapStatus_ShouldHaveCorrectUnderlyingValues()
    {
        // Assert
        Assert.Equal(0, (byte)SnapStatus.Call);
        Assert.Equal(1, (byte)SnapStatus.Return);
        Assert.Equal(2, (byte)SnapStatus.Error);
    }
}
