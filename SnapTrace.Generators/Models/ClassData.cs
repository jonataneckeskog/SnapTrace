using System.Collections.Generic;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Models;

internal record ClassData(
    string Namespace,
    string Name,
    string FullyQualifiedName,
    ClassSituation Situation,
    string TypeParameters,
    string WhereConstraints,
    IReadOnlyList<ContextMemberData> ContextMembers);
