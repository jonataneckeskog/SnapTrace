using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Builders;

public class ClassInterceptorBuilder
{
    private readonly string _className;
    private readonly ClassSituation _situation;
    private string? _typeParameters;
    private string? _whereConstraints;

    private readonly HashSet<(string Name, string Type)> _contextMembers = new();
    private readonly List<MethodInterceptorBuilder> _methods = new();

    public ClassInterceptorBuilder(string className, ClassSituation situation)
    {
        _className = className;
        _situation = situation;
    }

    public ClassInterceptorBuilder WithTypeParameters(string typeParameters)
    {
        _typeParameters = typeParameters;
        return this;
    }

    public ClassInterceptorBuilder WithWhereConstraints(string whereConstraints)
    {
        _whereConstraints = whereConstraints;
        return this;
    }

    public ClassInterceptorBuilder WithMethod(string name, MethodSituation situation, Action<MethodInterceptorBuilder> config)
    {
        var mb = new MethodInterceptorBuilder(_className, name, situation);

        config(mb);
        _methods.Add(mb);
        return this;
    }

    public ClassInterceptorBuilder AddContextMember(string memberName, string type = "object?")
    {
        _contextMembers.Add((memberName, type));
        return this;
    }

    internal void InternalBuild(StringBuilder sb)
    {
        // Ensure we have a clean reference to the target class
        string targetType = $"global::{_className}";

        sb.AppendLine($"internal static class {_className}_SnapTrace");
        sb.AppendLine("{");

        // 1. Static Record Accessor
        sb.AppendLine("    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = \"Record\")]");
        sb.AppendLine("    extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTracer? target, string method, object? data, object? context, global::SnapTrace.SnapStatus status);");
        sb.AppendLine();

        // 2. Generate Accessors (Only if there are members)
        foreach (var member in _contextMembers)
        {
            // NOTE: 'member' needs to be a type that contains .Name and .Type for this to compile
            sb.AppendLine($"    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.Field, Name = \"{member.Name}\")]");
            sb.AppendLine($"    extern static ref {member.Type} Get_{member.Name}_SnapTrace({targetType} @this);");
            sb.AppendLine();
        }

        // 3. The Updated Context Method
        sb.AppendLine("    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        // We ALWAYS take @this now, so the call site in MethodInterceptorBuilder can remain consistent
        sb.AppendLine($"    private static string GetClassContext_SnapTrace({targetType} @this)");
        sb.AppendLine("    {");

        if (_contextMembers.Count == 0)
        {
            sb.AppendLine("        return string.Empty;");
        }
        else
        {
            // Use the generated accessors inside the string interpolation
            var joined = string.Join(", ", _contextMembers.Select(m => $"{{Get_{m.Name}_SnapTrace(@this)}}"));
            sb.AppendLine($"        return $\"({joined})\";");
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        // 4. Build the individual method interceptors
        foreach (var (method, i) in _methods.Select((value, index) => (value, index)))
        {
            method.InternalBuild(sb);
            if (i < _methods.Count - 1) sb.AppendLine();
        }

        sb.AppendLine("}");
    }
}
