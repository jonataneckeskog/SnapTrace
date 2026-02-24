using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Builders;

/// <summary>
/// Handles building the syntax for namespaces in SnapTrace. NameSpaceBuilder uses the old syntax style
/// with brackets.
/// </summary>
public class NamespaceBuilder
{
    private readonly List<ClassInterceptorBuilder> _classes = new();
    private readonly string _originalNamespaceName;
    private readonly string _generatedNamespaceName;

    public NamespaceBuilder(string namespaceName)
    {
        _originalNamespaceName = namespaceName;
        _generatedNamespaceName = string.IsNullOrWhiteSpace(namespaceName)
            ? "SnapTrace.Generated"
            : $"SnapTrace.Generated.{namespaceName}";
    }

    public NamespaceBuilder WithClass(string className, ClassSituation situation, Action<ClassInterceptorBuilder> config)
    {
        var mb = new ClassInterceptorBuilder(_originalNamespaceName, className, situation);
        config(mb);
        _classes.Add(mb);
        return this;
    }

    internal void InternalBuild(IndentedTextWriter writer)
    {
        bool isGlobalNamespace = string.IsNullOrWhiteSpace(_generatedNamespaceName);

        // 1. Open the namespace block
        if (!isGlobalNamespace)
        {
            writer.WriteLine($"namespace {_generatedNamespaceName}");
            writer.Write("{");
            writer.Indent++; // Everything inside this namespace is now indented
        }

        // 2. Build all the classes inside this namespace
        foreach (var __class in _classes)
        {
            writer.WriteLine();
            __class.InternalBuild(writer);
        }

        // 3. Close the namespace block
        if (!isGlobalNamespace)
        {
            writer.Indent--; // Step back out
            writer.WriteLine("}");
        }
    }
}
