using System.Collections.Generic;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Models;

/// <summary>
/// Model 'collected' by the generator and later send to Builders
/// to handle building the syntax.
/// </summary>
/// <param name="Namespace"></param>
/// <param name="Name"></param>
/// <param name="FullyQualifiedName"></param>
/// <param name="Situation"></param>
/// <param name="TypeParameters"></param>
/// <param name="WhereConstraints"></param>
/// <param name="ContextMembers"></param>
internal record ClassData(
    string Namespace,
    string Name,
    string FullyQualifiedName,
    ClassSituation Situation,
    string TypeParameters,
    string WhereConstraints,
    IReadOnlyList<ContextMemberData> ContextMembers);
