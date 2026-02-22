namespace SnapTrace.Generators.Models;

internal record InterceptedCall(
    string FilePath,
    int Line,
    int Column,
    MethodData Method,
    ClassData Class);
