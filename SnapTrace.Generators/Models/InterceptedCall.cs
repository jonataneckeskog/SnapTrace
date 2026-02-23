namespace SnapTrace.Generators.Models;

/// <summary>
/// Model 'collected' by the generator and later send to Builders
/// to handle building the syntax.
/// </summary>
/// <param name="FilePath"></param>
/// <param name="Line"></param>
/// <param name="Column"></param>
/// <param name="Method"></param>
/// <param name="Class"></param>
internal record InterceptedCall(
    string FilePath,
    int Line,
    int Column,
    MethodData Method,
    ClassData Class);
