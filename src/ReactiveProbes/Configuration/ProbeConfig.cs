using System.Diagnostics.CodeAnalysis;

namespace ReactiveProbes.Configuration;

[ExcludeFromCodeCoverage]
public class ProbeConfig
{
    public int Interval { get; set; } = 10;
}