using System.Runtime.CompilerServices;

namespace SnapTrace.Generators.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // This tells Verify to put all snapshots in a "Snapshots" 
        // folder relative to the test file.
        UseSourceFileRelativeDirectory("Snapshots");
    }
}
