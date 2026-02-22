using System.Collections.Generic;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Models;

internal record MethodData(
    string Name,
    string ReturnType,
    bool IsVoid,
    MethodSituation Situation,
    string TypeParameters,
    string WhereConstraints,
    IReadOnlyList<ParameterData> Parameters);
