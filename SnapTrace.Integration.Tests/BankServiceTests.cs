using System.Text.Json;
using SnapTrace.Integration.Tests.Infrastructure;

namespace SnapTrace.Integration.Tests;

public class BankServiceFixture : IAsyncLifetime
{
    public string FixturePath { get; } = DotnetRunner.GetFixturePath("BankService");

    public async Task InitializeAsync()
    {
        var build = await DotnetRunner.BuildAsync(FixturePath);
        Assert.True(build.ExitCode == 0, $"Build failed:\n{build.Stderr}");
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

public class BankServiceTests : IClassFixture<BankServiceFixture>
{
    private readonly BankServiceFixture _fixture;

    public BankServiceTests(BankServiceFixture fixture) => _fixture = fixture;

    private async Task<List<SnapTraceEntry>> GetEntriesAsync()
    {
        var result = await DotnetRunner.RunAsync(_fixture.FixturePath);
        return SnapTraceOutput.Parse(result.Stdout);
    }

    [Fact]
    public async Task SnapTraceIgnore_RedactsParameter()
    {
        var entries = await GetEntriesAsync();

        var call = Assert.Single(entries, e => e.Status == "Call" && e.Method == "Deposit");
        Assert.NotNull(call.Data);

        var dataArray = call.Data.Value;
        Assert.Equal(JsonValueKind.Array, dataArray.ValueKind);

        var firstParam = dataArray[0].GetString();
        Assert.Equal("[REDACTED]", firstParam);
    }

    [Fact]
    public async Task SnapTraceIgnore_DoesNotRedactOtherParameters()
    {
        var entries = await GetEntriesAsync();

        var call = Assert.Single(entries, e => e.Status == "Call" && e.Method == "Deposit");
        Assert.NotNull(call.Data);

        var secondParam = decimal.Parse(call.Data.Value[1].GetString()!);
        Assert.Equal(500.00m, secondParam);
    }

    [Fact]
    public async Task SnapTraceContext_CapturesBalanceBeforeCall()
    {
        var entries = await GetEntriesAsync();

        var call = Assert.Single(entries, e => e.Status == "Call" && e.Method == "Deposit");
        Assert.NotNull(call.Context);

        var balance = call.Context.Value.GetProperty("_currentBalance").GetDecimal();
        Assert.Equal(1000.00m, balance);
    }

    [Fact]
    public async Task SnapTraceContext_CapturesBalanceAfterCall()
    {
        var entries = await GetEntriesAsync();

        var ret = Assert.Single(entries, e => e.Status == "Return" && e.Method == "Deposit");
        Assert.NotNull(ret.Context);

        var balance = ret.Context.Value.GetProperty("_currentBalance").GetDecimal();
        Assert.Equal(1500.00m, balance);
    }

    [Fact]
    public async Task Output_ContainsErrorEntry()
    {
        var entries = await GetEntriesAsync();

        var error = Assert.Single(entries, e => e.Status == "Error");
        Assert.NotNull(error.Data);
        Assert.Contains("Something went wrong!", error.Data.Value.ToString());
    }
}
