using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        var mb = new ClassInterceptorBuilder(_namespaceName, className, situation);
        config(mb);
        _classes.Add(mb);
        return this;
    }

    internal void InternalBuild(StringBuilder sb)
    {
        bool isGlobalNamespace = string.IsNullOrWhiteSpace(_namespaceName);

        // 1. Open the namespace block if it's not the global namespace
        if (!isGlobalNamespace)
        {
            sb.AppendLine($"namespace {_namespaceName}");
            sb.AppendLine("{");
        }

        // 2. Build all the classes inside this namespace
        for (int i = 0; i < _classes.Count; i++)
        {
            _classes[i].InternalBuild(sb);

            // Add an empty line between classes, but not after the last one
            if (i < _classes.Count - 1)
            {
                sb.AppendLine();
            }
        }

        // 3. Close the namespace block
        if (!isGlobalNamespace)
        {
            sb.AppendLine("}");
        }
    }
}
