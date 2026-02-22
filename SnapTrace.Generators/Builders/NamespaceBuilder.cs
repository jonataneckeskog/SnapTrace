using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Builders;

public class NamespaceBuilder
{
    private readonly List<ClassInterceptorBuilder> _classes = new();
    private readonly string _namespaceName;

    public NamespaceBuilder(string namespaceName)
    {
        _namespaceName = namespaceName;
    }

    public NamespaceBuilder WithClass(string className, ClassSituation situation, Action<ClassInterceptorBuilder> config)
    {
        var mb = new ClassInterceptorBuilder($"global::{_namespaceName}", className, situation);
        config(mb);
        _classes.Add(mb);
        return this;
    }

    internal void InternalBuild(IndentedTextWriter writer)
    {
        bool isGlobalNamespace = string.IsNullOrWhiteSpace(_namespaceName);

        // 1. Open the namespace block
        if (!isGlobalNamespace)
        {
            writer.WriteLine($"namespace {_namespaceName}");
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
