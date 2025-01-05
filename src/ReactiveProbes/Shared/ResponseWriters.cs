using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ReactiveProbes.HealthChecks;

namespace ReactiveProbes.Shared;

internal static class ResponseWriters
{
    async internal static Task ReadinessWriterAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        if (report.Status == HealthStatus.Healthy)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsync("Ready");
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("Not Ready");
        }
    }
    
    async internal static Task LivenessWriterAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
                
        var json = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            lastUpdated = LivenessCheck.LastUpdated,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data
            })
        });

        await context.Response.WriteAsync(json);
    }
    
    async internal static Task GenericStatusWriterAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = report.Status == HealthStatus.Healthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsync(report.Status.ToString());
    }
}