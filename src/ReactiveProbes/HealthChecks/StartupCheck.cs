using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ReactiveProbes.HealthChecks;

public class StartupCheck : IHealthCheck
{
    public static bool IsStarted { get; protected internal set; }
    
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(IsStarted ? 
                HealthCheckResult.Healthy("Service is Ready") : 
                HealthCheckResult.Unhealthy("Service is not Ready"));
    }
}