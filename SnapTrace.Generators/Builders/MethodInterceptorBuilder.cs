using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Builders;

public class MethodInterceptorBuilder
{
    // Builder data
    private ReturnDefinition _return = new("void", true, false, false);
    private readonly List<ParameterDefinition> _params = new();
    private readonly List<string> _locations = new();

    // Method Metadata
    private readonly string _methodName;
    private readonly string _className;
    private string? _typeParameters;
    private string? _whereConstraints;
    private MethodSituation _situation;

    public MethodInterceptorBuilder(string className, string methodName, MethodSituation situation)
    {
        _className = className;
        _methodName = methodName;
        _situation = situation;
    }

    // --- Standard additions ---

    public MethodInterceptorBuilder WithReturn(string type, bool deepCopy = false, bool redacted = false)
    {
        bool isVoid = type == "void";
        _return = new ReturnDefinition(type, isVoid, deepCopy, redacted);
        return this;
    }

    public MethodInterceptorBuilder WithTypeParameters(string typeParameters)
    {
        _typeParameters = typeParameters;
        return this;
    }

    public MethodInterceptorBuilder WithWhereConstraints(string whereConstraints)
    {
        _whereConstraints = whereConstraints;
        return this;
    }

    public MethodInterceptorBuilder WithParameter(string name, string type, string modifier = "", bool isParams = false, bool deepCopy = false, bool redacted = false)
    {
        _params.Add(new ParameterDefinition(name, type, modifier, isParams, deepCopy, redacted));
        return this;
    }

    public MethodInterceptorBuilder AddLocation(string path, int line, int col)
    {
        var safePath = path.Replace("\\", "\\\\");
        _locations.Add($@"[global::System.Runtime.CompilerServices.InterceptsLocation(@""{safePath}"", {line}, {col})]");
        return this;
    }

    // --- The Build Engine ---

    internal void InternalBuild(StringBuilder sb)
    {
        sb.AppendLine();

        // 1. Append InterceptsLocation
        foreach (var loc in _locations)
        {
            sb.AppendLine($"    {loc}");
        }

        // 2. Append method options
        sb.AppendLine("    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");

        // 3. Construct the strict interceptor method name
        var safeReturnType = GetSafeTypeName(_return.Type);
        var interceptorName = $"{_methodName}Intercept_{safeReturnType}";

        if (_params.Count > 0)
        {
            var paramPart = string.Join("_", _params.Select(p => GetSafeTypeName(p.Type)));
            interceptorName += $"_{paramPart}";
        }

        // 4. Resolve MethodSituation (Modifiers & Ref Returns)
        var modifiers = "public static";
        if (_situation.HasFlag(MethodSituation.Async)) modifiers += " async";
        if (_situation.HasFlag(MethodSituation.Unsafe)) modifiers += " unsafe";

        var returnStr = _return.Type;
        if (_situation.HasFlag(MethodSituation.ReturnsRef)) returnStr = "ref " + returnStr;
        else if (_situation.HasFlag(MethodSituation.ReturnsRefReadonly)) returnStr = "ref readonly " + returnStr;

        // 5. Construct Method Parameters
        var methodParams = new List<string>();
        if (!_situation.HasFlag(MethodSituation.Static))
        {
            // Inject 'this' for instance methods
            methodParams.Add($"this global::{_className} @this");
        }

        foreach (var p in _params)
        {
            var prefix = string.IsNullOrEmpty(p.Modifier) ? "" : $"{p.Modifier} ";
            if (p.IsParams) prefix += "params ";
            methodParams.Add($"{prefix}{p.Type} {p.Name}");
        }

        // 6. Write Method Signature
        sb.Append($"    {modifiers} {returnStr} {interceptorName}");
        if (!string.IsNullOrEmpty(_typeParameters)) sb.Append($"<{_typeParameters}>");
        sb.Append($"({string.Join(", ", methodParams)})");
        if (!string.IsNullOrEmpty(_whereConstraints)) sb.Append($" {_whereConstraints}");
        sb.AppendLine();
        sb.AppendLine("    {");

        // 7. Save method parameters to a tuple
        sb.Append("        object? data = ");
        if (_params.Count == 0)
        {
            sb.AppendLine("null;");
        }
        else
        {
            sb.Append("(");
            var tupleParts = _params.Select(p =>
            {
                if (p.Redacted) return $"{p.Name}: \"[REDACTED]\"";
                if (p.DeepCopy) return $"{p.Name}: {p.Name} is null ? default : global::System.Text.Json.JsonSerializer.Deserialize<{p.Type}>(global::System.Text.Json.JsonSerializer.SerializeToUtf8Bytes({p.Name}))";
                return $"{p.Name}: {p.Name}";
            });
            sb.Append(string.Join(", ", tupleParts));
            sb.AppendLine(");");
        }

        // 8. Save the context
        sb.AppendLine("        var context = GetClassContext_SnapTrace();");
        sb.AppendLine();

        // 8. Record the Entry
        sb.AppendLine($"        CallRecord_SnapTrace(null!, \"{_methodName}\", data, context, global::SnapTrace.SnapStatus.Call);");

        // 9. EXECUTE ORIGINAL AND CAPTURE RETURN
        if (_return.IsVoid)
        {
            // Call the original method on the '@this' instance or statically
            var target = _situation.HasFlag(MethodSituation.Static) ? $"global::{_className}" : "@this";
            sb.AppendLine($"        {target}.{_methodName}({string.Join(", ", _params.Select(p => p.Name))});");

            // Record the exit
            sb.AppendLine($"        CallRecord_SnapTrace(null!, \"{_methodName}\", null, context, global::SnapTrace.SnapStatus.Return);");
        }
        else
        {
            var target = _situation.HasFlag(MethodSituation.Static) ? $"global::{_className}" : "@this";
            sb.AppendLine($"        var result = {target}.{_methodName}({string.Join(", ", _params.Select(p => p.Name))});");

            // Record the result
            sb.AppendLine($"        CallRecord_SnapTrace(null!, \"{_methodName}\", result, context, global::SnapTrace.SnapStatus.Return);");
            sb.AppendLine("        return result;");
        }

        // 10. Close the method
        sb.AppendLine("    }");
    }

    private static string GetSafeTypeName(string type)
    {
        return type
            .Replace("global::", "")
            .Replace("[]", "Array")
            .Replace("<", "_")
            .Replace(">", "_")
            .Replace(",", "_")
            .Replace(" ", "")
            .Replace(".", "_")
            .TrimEnd('_');
    }
}
