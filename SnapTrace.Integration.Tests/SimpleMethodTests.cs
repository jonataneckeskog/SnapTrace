using SnapTrace.Integration.Tests.Infrastructure;

namespace SnapTrace.Integration.Tests;

public class SimpleMethodFixture : IAsyncLifetime
{
    public string FixturePath { get; } = DotnetRunner.GetFixturePath("SimpleMethod");

    public async Task InitializeAsync()
    {
        var build = await DotnetRunner.BuildAsync(FixturePath);
        Assert.True(build.ExitCode == 0, $"Build failed:\n{build.Stderr}");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class SimpleMethodTests : IClassFixture<SimpleMethodFixture>
{
    private readonly SimpleMethodFixture _fixture;

    public SimpleMethodTests(SimpleMethodFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Output_ContainsCallEntry_WithCorrectMethodAndData()
    {
        var result = await DotnetRunner.RunAsync(_fixture.FixturePath);
        var entries = SnapTraceOutput.Parse(result.Stdout);

        var call = Assert.Single(entries, e => e.Status == "Call" && e.Method == "Add");
        Assert.NotNull(call.Data);
        Assert.Equal("[\n    \"3\",\n    \"5\"\n  ]", call.Data.Value.ToString());
    }

    [Fact]
    public async Task Output_ContainsReturnEntry_WithResult()
    {
        var result = await DotnetRunner.RunAsync(_fixture.FixturePath);
        var entries = SnapTraceOutput.Parse(result.Stdout);

        var ret = Assert.Single(entries, e => e.Status == "Return" && e.Method == "Add");
        Assert.NotNull(ret.Data);
        Assert.Equal("8", ret.Data.Value.ToString());
    }

    [Fact]
    public async Task Output_ContainsErrorEntry_WithExceptionMessage()
    {
        var result = await DotnetRunner.RunAsync(_fixture.FixturePath);
        var entries = SnapTraceOutput.Parse(result.Stdout);

        var error = Assert.Single(entries, e => e.Status == "Error");
        Assert.NotNull(error.Data);
        Assert.Contains("Test crash", error.Data.Value.ToString());
    }
}
