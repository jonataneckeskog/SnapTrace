using System.Collections.Generic;
using System.Linq;
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
    IReadOnlyList<ContextMemberData> ContextMembers)
{
    public virtual bool Equals(ClassData? other)
    {
        if (other is null) return false;

        return Namespace == other.Namespace &&
               Name == other.Name &&
               FullyQualifiedName == other.FullyQualifiedName &&
               Situation == other.Situation &&
               TypeParameters == other.TypeParameters &&
               WhereConstraints == other.WhereConstraints &&
               ContextMembers.SequenceEqual(other.ContextMembers);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 23) + (Namespace?.GetHashCode() ?? 0);
            hash = (hash * 23) + (Name?.GetHashCode() ?? 0);
            hash = (hash * 23) + (FullyQualifiedName?.GetHashCode() ?? 0);
            hash = (hash * 23) + Situation.GetHashCode();
            hash = (hash * 23) + (TypeParameters?.GetHashCode() ?? 0);
            hash = (hash * 23) + (WhereConstraints?.GetHashCode() ?? 0);

            if (ContextMembers != null)
            {
                foreach (var member in ContextMembers)
                {
                    hash = (hash * 23) + (member?.GetHashCode() ?? 0);
                }
            }

            return hash;
        }
    }
}
