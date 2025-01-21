using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ReactiveProbes.HealthChecks;

public class LivenessCheck : IHealthCheck
{
    private const string LastReportKey = "LastReport";
    // ReSharper disable once InconsistentNaming
    private static readonly ConcurrentDictionary<string, HealthReport> _lastReport = new ConcurrentDictionary<string, HealthReport>();

    public static HealthReport LastReport
    {
        get => _lastReport.GetValueOrDefault(LastReportKey)!;
        protected internal set
        {
            _lastReport.AddOrUpdate(LastReportKey, _ => value, (_, _) => value);
            LastUpdated = DateTime.UtcNow;
        }
    }

    public static DateTime LastUpdated { get; private set; }
    
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        if (LastReport?.Status != HealthStatus.Healthy)
            return Task.FromResult(HealthCheckResult.Unhealthy("Service is unhealthy"));
        var data = LastReport.Entries.ToDictionary(
            entry => entry.Key, object (entry) => entry.Value
        );
        
        return Task.FromResult(HealthCheckResult.Healthy("Service is healthy", data));
    }
}