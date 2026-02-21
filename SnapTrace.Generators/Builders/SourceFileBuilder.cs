using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SnapTrace.Generators.Definitions;

namespace SnapTrace.Generators.Builders;

public class SourceFileBuilder
{
    private readonly List<ClassInterceptorBuilder> _classes = new();

    public SourceFileBuilder WithClass(string name, ClassSituation situation, Action<ClassInterceptorBuilder> config)
    {
        var mb = new ClassInterceptorBuilder(name, situation);
        config(mb);
        _classes.Add(mb);
        return this;
    }

    public string Build()
    {
        StringBuilder sb = new();

        sb.AppendLine("    using global::SnapTrace;");
        sb.AppendLine();

        return sb.ToString();
    }
}
