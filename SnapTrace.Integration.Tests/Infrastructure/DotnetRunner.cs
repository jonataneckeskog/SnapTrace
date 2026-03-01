using System.Diagnostics;

namespace SnapTrace.Integration.Tests.Infrastructure;

public record ProcessResult(int ExitCode, string Stdout, string Stderr);

public static class DotnetRunner
{
    private static readonly string RepoRoot = FindRepoRoot();

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir, "SnapTrace.slnx")))
                return dir;
            dir = Path.GetDirectoryName(dir);
        }

        throw new InvalidOperationException(
            "Could not find repository root (SnapTrace.slnx) from " + AppContext.BaseDirectory);
    }

    public static string GetFixturePath(string fixtureName)
        => Path.Combine(RepoRoot, "SnapTrace.Integration.Tests", "Fixtures", fixtureName);

    public static async Task<ProcessResult> BuildAsync(string fixturePath, int timeoutMs = 30_000)
        => await RunDotnetAsync("build --configuration Release", fixturePath, timeoutMs);

    public static async Task<ProcessResult> RunAsync(string fixturePath, int timeoutMs = 15_000)
        => await RunDotnetAsync("run --configuration Release --no-build", fixturePath, timeoutMs);

    private static async Task<ProcessResult> RunDotnetAsync(string args, string workingDir, int timeoutMs)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = args,
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        using var cts = new CancellationTokenSource(timeoutMs);
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException(
                $"dotnet {args} in {workingDir} timed out after {timeoutMs}ms");
        }

        return new ProcessResult(process.ExitCode, await stdoutTask, await stderrTask);
    }
}
