using System;
using System.Collections.Generic;
using System.Linq;
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
    IReadOnlyList<ParameterData> Parameters,
    bool DeepCopyReturn,
    bool RedactedReturn)
{
    public virtual bool Equals(MethodData? other)
    {
        if (other is null) return false;
        return Name == other.Name &&
               ReturnType == other.ReturnType &&
               IsVoid == other.IsVoid &&
               Situation == other.Situation &&
               TypeParameters == other.TypeParameters &&
               WhereConstraints == other.WhereConstraints &&
               Parameters.SequenceEqual(other.Parameters) &&
               DeepCopyReturn == other.DeepCopyReturn &&
               RedactedReturn == other.RedactedReturn;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 23) + (Name?.GetHashCode() ?? 0);
            hash = (hash * 23) + (ReturnType?.GetHashCode() ?? 0);
            hash = (hash * 23) + IsVoid.GetHashCode();
            hash = (hash * 23) + Situation.GetHashCode();
            hash = (hash * 23) + (TypeParameters?.GetHashCode() ?? 0);
            hash = (hash * 23) + (WhereConstraints?.GetHashCode() ?? 0);
            hash = (hash * 23) + DeepCopyReturn.GetHashCode();
            hash = (hash * 23) + RedactedReturn.GetHashCode();

            if (Parameters != null)
            {
                foreach (var p in Parameters)
                {
                    hash = (hash * 23) + (p?.GetHashCode() ?? 0);
                }
            }

            return hash;
        }
    }
}
