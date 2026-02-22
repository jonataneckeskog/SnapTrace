using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Builders;

public class ClassInterceptorBuilder
{
    private readonly string _fullyQualifiedName;
    private readonly string _className;
    private readonly ClassSituation _situation;
    private string? _typeParameters;
    private string? _whereConstraints;

    private readonly HashSet<(string Name, string Type)> _contextMembers = new();
    private readonly List<MethodInterceptorBuilder> _methods = new();

    public ClassInterceptorBuilder(string fullyQualifiedName, string className, ClassSituation situation)
    {
        _fullyQualifiedName = fullyQualifiedName;
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
        // 1. Ensure we have Namespace.ClassName
        string baseTypeName = $"{_fullyQualifiedName}.{_className}";

        // 2. Add generics only if they exist, and handle the brackets cleanly
        string typeWithGenerics = !string.IsNullOrWhiteSpace(_typeParameters)
            ? $"{baseTypeName}<{_typeParameters}>"
            : baseTypeName;

        // 3. Ensure "global::" is only at the very start if not already present
        var fullName = typeWithGenerics.StartsWith("global::")
            ? typeWithGenerics
            : $"global::{typeWithGenerics}";

        var mb = new MethodInterceptorBuilder(fullName, name, situation, _situation);

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
        // 1. Evaluate situations
        bool isStatic = _situation.HasFlag(ClassSituation.Static);
        bool isStruct = _situation.HasFlag(ClassSituation.IsStruct) || _situation.HasFlag(ClassSituation.IsRefStruct);
        bool isGeneric = _situation.HasFlag(ClassSituation.IsGeneric);

        // 2. Build the target type, appending generics if present
        string targetType = $"{_fullyQualifiedName}.{_className}"; // e.g., "global::MyNamespace.MyClass"
        if (isGeneric && !string.IsNullOrWhiteSpace(_typeParameters))
        {
            targetType += _typeParameters; // Result: "global::MyNamespace.MyClass<T>"
        }

        // 3. Class declaration with generics and constraints
        string classDecl = $"internal static class {_className}_SnapTrace";
        if (isGeneric && !string.IsNullOrWhiteSpace(_typeParameters))
        {
            classDecl += _typeParameters;
        }

        sb.AppendLine(classDecl);
        if (isGeneric && !string.IsNullOrWhiteSpace(_whereConstraints))
        {
            sb.AppendLine($"    {_whereConstraints}");
        }
        sb.AppendLine("{");

        // 4. Static Record Accessor
        sb.AppendLine("    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod, Name = \"Record\")]");
        sb.AppendLine("    extern static void CallRecord_SnapTrace(global::SnapTrace.SnapTracer? target, string method, object? data, object? context, global::SnapTrace.SnapStatus status);");
        sb.AppendLine();

        // Determine parameters based on the class situation
        // Static classes have no 'this'. Structs pass 'this' by ref.
        string thisParam = isStatic ? "" : (isStruct ? $"ref {targetType} @this" : $"{targetType} @this");
        string thisArg = isStatic ? "" : (isStruct ? "ref @this" : "@this");

        // 5. Generate Accessors
        foreach (var member in _contextMembers)
        {
            string accessorKind = isStatic ? "StaticField" : "Field";

            sb.AppendLine($"    [global::System.Runtime.CompilerServices.UnsafeAccessor(global::System.Runtime.CompilerServices.UnsafeAccessorKind.{accessorKind}, Name = \"{member.Name}\")]");
            sb.AppendLine($"    extern static ref {member.Type} Get_{member.Name}_SnapTrace({thisParam});");
            sb.AppendLine();
        }

        // 6. The Updated Context Method
        sb.AppendLine("    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine($"    private static object? GetClassContext_SnapTrace({thisParam})");
        sb.AppendLine("    {");

        if (_contextMembers.Count == 0)
        {
            sb.AppendLine("        return null;");
        }
        else
        {
            var joinedArgs = string.Join(", ", _contextMembers.Select(m => $"{m.Name} = (object?)Get_{m.Name}_SnapTrace({thisArg})"));
            sb.AppendLine($"        return new {{ {joinedArgs} }};");
        }

        sb.AppendLine("    }");
        sb.AppendLine();

        // 7. Build the individual method interceptors
        foreach (var (method, i) in _methods.Select((value, index) => (value, index)))
        {
            method.InternalBuild(sb);
            if (i < _methods.Count - 1) sb.AppendLine();
        }

        sb.AppendLine("}");
    }
}
