using Microsoft.CodeAnalysis;

namespace SnapTrace.Generators;

[Generator(LanguageNames.CSharp)]
public class SnapTraceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Your logic to find method calls and generate the interceptor code
    }
}
