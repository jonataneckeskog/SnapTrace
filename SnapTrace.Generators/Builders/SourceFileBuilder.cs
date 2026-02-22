using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Builders;

public class SourceFileBuilder
{
    private readonly Dictionary<string, NamespaceBuilder> _namespaces = new();

    public SourceFileBuilder WithNamespace(string namespaceName, Action<NamespaceBuilder> config)
    {
        if (!_namespaces.TryGetValue(namespaceName, out var nsBuilder))
        {
            nsBuilder = new NamespaceBuilder(namespaceName);
            _namespaces[namespaceName] = nsBuilder;
        }
        config(nsBuilder);

        return this;
    }

    public string Build()
    {
        StringBuilder sb = new();

        sb.AppendLine("using global::SnapTrace;");
        sb.AppendLine();

        foreach (var (__namespace, i) in _namespaces.Select((value, index) => (value, index)))
        {
            __namespace.Value.InternalBuild(sb);
            if (i < _namespaces.Count - 1) sb.AppendLine();
        }

        return sb.ToString();
    }
}
