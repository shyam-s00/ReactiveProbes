using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SampleAPI;

// Bunch of health checks that returns statuses in different timelines. 

public class CustomHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
        //await Task.CompletedTask;
        return  HealthCheckResult.Healthy("Service is healthy");
    }
}

public class ApiHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        return HealthCheckResult.Healthy("API is healthy");
    }
}

public class DbHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        // Return random health status of healthy or unhealthy
        
        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        var random = new Random();
        var isHealthy = random.Next(0, 2) == 0;
        return isHealthy ? HealthCheckResult.Healthy("DB is Healthy") : HealthCheckResult.Unhealthy("DB is Unhealthy");
    }
}

public class ExternalServiceHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        await Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
        return HealthCheckResult.Healthy("External service is healthy");
    }
}