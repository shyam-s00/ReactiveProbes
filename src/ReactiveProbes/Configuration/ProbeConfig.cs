using System.Diagnostics.CodeAnalysis;

namespace ReactiveProbes.Configuration;

[ExcludeFromCodeCoverage]
public class ProbeConfig
{
    public IntervalsConfig Intervals { get; set; } = new IntervalsConfig();
}

public class IntervalsConfig
{
    public int Startup { get; set; } = 10;
    public int HealthChecks { get; set; } = 60;
}