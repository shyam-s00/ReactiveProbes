using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ReactiveProbes.Configuration;
using ReactiveProbes.HealthChecks;

namespace ReactiveProbes;

public static class ReactiveProbeInstaller
{
    public static void AddReactiveProbes(this IServiceCollection services, IConfiguration config)
    {
         services.Configure<ProbeConfig>(config.GetSection("ProbeConfig"));
         services.AddSingleton<IObservableProbe, ObservableProbe>();
         
         services.AddHealthChecks()
             .AddCheck<StartupCheck>("StartupCheck", tags: ["startup"]);
    }

    public static void RegisterReactiveStartupProbe(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/ready", new HealthCheckOptions()
        {
            Predicate = (check) => check.Tags.Contains("startup")
        });
        
        var observableProbe = app.ApplicationServices.GetRequiredService<IObservableProbe>();
        observableProbe.WhenHealthCheckChanged()
            .Subscribe(report =>
            {
                if (report.Status == HealthStatus.Healthy)
                {
                    StartupCheck.IsStarted = true;
                    Console.WriteLine("Application started");
                }
            }, () => Console.WriteLine("Readiness check(s) completed"));
    }
    
    public static void MapHealthCheckEndpoint(this IApplicationBuilder app, string pattern = "/health", string tag = "health")
    {
        app.UseHealthChecks(pattern, new HealthCheckOptions()
        {
            Predicate = (check) => check.Tags.Contains(tag),
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json; charset=utf-8";

                var json = JsonSerializer.Serialize(new
                {
                    status = report.Status.ToString(),
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
        });
    }
    
    public static IHealthChecksBuilder AddRestApiCheck(this IHealthChecksBuilder builder, string name, string url, 
        string[]? tags = null)
    {
        builder.Services.AddHttpClient();
        builder.AddTypeActivatedCheck<RestApiHealthCheck>(name, null, tags, [url]);
        
        return builder;
    }
}