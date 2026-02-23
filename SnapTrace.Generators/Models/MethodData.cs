using System.Collections.Generic;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Models;

/// <summary>
/// Model 'collected' by the generator and later send to Builders
/// to handle building the syntax.
/// </summary>
/// <param name="Name"></param>
/// <param name="ReturnType"></param>
/// <param name="IsVoid"></param>
/// <param name="Situation"></param>
/// <param name="TypeParameters"></param>
/// <param name="WhereConstraints"></param>
/// <param name="Parameters"></param>
internal record MethodData(
    string Name,
    string ReturnType,
    bool IsVoid,
    MethodSituation Situation,
    string TypeParameters,
    string WhereConstraints,
    IReadOnlyList<ParameterData> Parameters);
