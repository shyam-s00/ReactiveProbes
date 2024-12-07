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
         var probeConfig = new ProbeConfig();
         config.GetSection("ProbeConfig").Bind(probeConfig);
         services.AddSingleton(probeConfig);
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
                    Console.WriteLine("Service is ready");
                }
            }, () => Console.WriteLine("Readiness check completed"));
    }
    //
    // public static void RegisterReactiveProbes(this IServiceProvider serviceProvider, Action<HealthReport> onChanged,
    //     Action completed)
    // {
    //     var observableProbe = serviceProvider.GetRequiredService<IObservableProbe>();
    //     observableProbe.WhenHealthCheckChanged()
    //         .Subscribe(onChanged, completed);
    // }
    //
    // public static IObservable<HealthReport> StartReactiveProbes(this IServiceProvider serviceProvider)
    // {
    //     var probe = serviceProvider.GetRequiredService<IObservableProbe>();
    //     return probe.WhenHealthCheckChanged();
    // }
}