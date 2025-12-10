using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace ReactiveProbes.HealthChecks;

public class SqlServerHealthCheck(IConfiguration config, string name) : IHealthCheck
{
    private readonly string _connectionString = config[name] ?? throw new ArgumentNullException(nameof(name));

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy("SQL Server is healthy");
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy("SQL Server is unhealthy", e);   
        }
    }
}

public class SqlServerHealthCheck<TContext>(TContext dbContext) : IHealthCheck
    where TContext : DbContext
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            var dbStatus = await dbContext.Database.CanConnectAsync(cancellationToken);
            return dbStatus ? HealthCheckResult.Healthy("SQL Server is healthy") 
                : HealthCheckResult.Degraded("SQL Server is unhealthy");
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy("SQL Server is unhealthy", e);
        }
    }
}