using System.Text.Json;
using SnapTrace.Integration.Tests.Infrastructure;

namespace SnapTrace.Integration.Tests;

public class ReturnAttributesFixture : IAsyncLifetime
{
    public string FixturePath { get; } = DotnetRunner.GetFixturePath("ReturnAttributes");

    public async Task InitializeAsync()
    {
        var build = await DotnetRunner.BuildAsync(FixturePath);
        Assert.True(build.ExitCode == 0, $"Build failed:\n{build.Stderr}");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class ReturnAttributeTests : IClassFixture<ReturnAttributesFixture>
{
    private readonly ReturnAttributesFixture _fixture;

    public ReturnAttributeTests(ReturnAttributesFixture fixture) => _fixture = fixture;

    private async Task<List<SnapTraceEntry>> GetEntriesAsync()
    {
        var result = await DotnetRunner.RunAsync(_fixture.FixturePath);
        return SnapTraceOutput.Parse(result.Stdout);
    }

    [Fact]
    public async Task ReturnIgnore_RedactsReturnValue()
    {
        var entries = await GetEntriesAsync();

        var ret = Assert.Single(entries, e => e.Status == "Return" && e.Method == "GetSecret");
        Assert.NotNull(ret.Data);
        Assert.Equal("[REDACTED]", ret.Data.Value.GetString());
    }

    [Fact]
    public async Task ReturnDeep_DeepCopiesReturnValue()
    {
        var entries = await GetEntriesAsync();

        var ret = Assert.Single(entries, e => e.Status == "Return" && e.Method == "GetCoords");
        Assert.NotNull(ret.Data);

        // Deep copy captures object properties as JSON, not just ToString()
        Assert.Equal(JsonValueKind.Object, ret.Data.Value.ValueKind);
        Assert.Equal(1, ret.Data.Value.GetProperty("X").GetInt32());
        Assert.Equal(2, ret.Data.Value.GetProperty("Y").GetInt32());
    }

    [Fact]
    public async Task NoReturnAttribute_UsesToString()
    {
        var entries = await GetEntriesAsync();

        var ret = Assert.Single(entries, e => e.Status == "Return" && e.Method == "GetLabel");
        Assert.NotNull(ret.Data);

        // Without [return: SnapTraceDeep], value types use ToString()
        Assert.Equal("(3, 4)", ret.Data.Value.GetString());
    }
}
