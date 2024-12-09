using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/ready", new HealthCheckOptions
            {
                Predicate = (check) => check.Tags.Contains("startup"),
            });
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
}