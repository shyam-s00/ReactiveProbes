using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ReactiveProbes.Configuration;
using ReactiveProbes.HealthChecks;
using ReactiveProbes.Probes;
using ReactiveProbes.Shared;
using ReactiveProbes.Streaming;

namespace ReactiveProbes;

public static class ReactiveProbeInstaller
{
    /// <summary>
    /// Adds reactive startup probes to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the probes to.</param>
    /// <param name="config">The configuration to bind the ProbeConfig section.</param>
    public static IServiceCollection AddReactiveProbes(this IServiceCollection services, IConfiguration config)
    {
         services.Configure<ProbeConfig>(config.GetSection("ProbeConfig"));
         services.AddSingleton<IObservableProbe, ObservableProbe>();
         
         services.AddHealthChecks()
             .AddCheck<StartupCheck>("StartupCheck", tags: ["startup"]);

         return services;
    }

    public static IServiceCollection AddReactiveHealthStream(this IServiceCollection services)
    {
        // add config settings here for health stream.
        
        services.AddSingleton<ReactiveHealthStream>();
        services.AddHostedService(s => s.GetRequiredService<ReactiveHealthStream>());
        
        return services;
    }
    
    /// <param name="app">The application builder to configure the middleware.</param>
    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Registers the reactive startup probe sequence and sets up the  <c>/ready</c> endpoint.
        /// </summary>
        public void RegisterReactiveStartupProbe()
        {
            app.UseHealthChecks("/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("startup"),
                ResponseWriter = ResponseWriters.ReadinessWriterAsync
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

        /// <summary>
        /// Maps the health check to <c>/live</c> endpoint or with the specified pattern and tag. The endpoint returns the 
        /// extended health check status as a JSON response.
        /// </summary>
        /// <param name="pattern">The URL pattern for the health check endpoint.  Defaults to <c>/live</c></param>
        public void AddLiveStatusEndpoint(string pattern = "/live")
        {   
            app.UseHealthChecks(pattern, new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("live"),
                ResponseWriter = ResponseWriters.LivenessWriterAsync
            });
        }
    }
    
    // ReSharper disable once MemberCanBePrivate.Global

    /// <param name="builder">The health checks builder to add the services to.</param>
    extension(IHealthChecksBuilder builder)
    {
        /// <summary>
        /// Adds reactive health probes to the health checks builder. Doesn't start probes automatically until
        /// registration is started <see cref="RegisterReactiveHealthProbe"/>
        /// </summary>
        /// <returns>The updated health checks builder.</returns>
        public IHealthChecksBuilder AddReactiveHealthProbes()
        {
            builder.Services.AddSingleton<IObservableHealthProbes, ObservableHealthProbes>();
            builder.Services.AddHttpClient();
            builder.AddCheck<LivenessCheck>("LivenessCheck", tags: ["live"]);
            return builder;
        }

        /// <summary>
        /// Adds a REST API health check to the health checks builder.
        /// </summary>
        /// <param name="name">The name of the health check.</param>
        /// <param name="url">The URL of the REST API to check.</param>
        /// <param name="tags">Optional tags to filter the health check.</param>
        /// <returns>The updated health checks builder.</returns>
        public IHealthChecksBuilder AddRestApiCheck(string name, string url, 
            string[]? tags = null)
        {
            builder.AddTypeActivatedCheck<RestApiHealthCheck>(name, null, tags, [url]);
            return builder;
        }

        /// <summary>
        /// Adds a SQL Server health check to the health checks builder using a connection string from the configuration.
        /// </summary>
        /// <param name="connectionName">The name of the connection string in the configuration.</param>
        /// <param name="name">The name of the health check. Defaults to "SqlServer".</param>
        /// <param name="tags">Optional tags to filter the health check.</param>
        /// <returns>The updated health checks builder.</returns>
        public IHealthChecksBuilder AddSqlServerCheck(string connectionName, string name = "SqlServer", string[]? tags = null)
        {
            builder.AddTypeActivatedCheck<SqlServerHealthCheck>(name, null, tags, [connectionName]);
            return builder;
        }
        
        /// <summary>
        /// Adds a SQL Server health check to the health checks builder using an EF Core DbContext.
        /// </summary>
        /// <typeparam name="T">The type of the DbContext.</typeparam>
        /// <param name="name">The name of the health check. Defaults to "SqlServer".</param>
        /// <param name="tags">Optional tags to filter the health check.</param>
        /// <returns>The updated health checks builder.</returns>
        public IHealthChecksBuilder AddSqlServerCheck<T>(string name = "SqlServer", string[]? tags = null)
            where T : DbContext
        {
            builder.AddCheck<SqlServerHealthCheck<T>>(name, null, tags: tags!);
            return builder;
        }
    }

    /// <param name="app">The application builder to configure the middleware.</param>
    extension(IApplicationBuilder app)
    {
        /// <summary>
        /// Registers the reactive health probe by starting the subscription and sets up the health check endpoints.
        /// Sets up <c>/health/stop</c> that stops the health checks and <c>/health/start</c> that starts the health checks.
        /// Also sets up the health check endpoint <c>/health</c> to display the health check status
        /// by calling <see cref="MapHealthCheckEndpoint"/>
        /// </summary>
        /// <param name="pattern">The URL pattern for the health check endpoint.  Defaults to <c>/health</c></param>
        public IApplicationBuilder RegisterReactiveHealthProbe(string pattern = "/health")
        {
            app.MapHealthCheckEndpoint(pattern);
        
            var observableHealthChecks = app.ApplicationServices.GetRequiredService<IObservableHealthProbes>();
            observableHealthChecks.WhenChanged()
                .Subscribe(report =>
                {
                    LivenessCheck.LastReport = report;
                    Console.WriteLine($"Health Check status: {report.Status}");
                }, () => Console.WriteLine("Health check(s) completed"));
        
            app.Map($"{pattern}/stop", builder =>
            {
                builder.Run(async context =>
                {
                    observableHealthChecks.Stop();
                    await context.Response.WriteAsync("Health checks stopped");
                });
            });
        
            app.Map($"{pattern}/start", builder =>
            {
                builder.Run(async context =>
                {
                    observableHealthChecks.Start();
                    await context.Response.WriteAsync("Health checks started");
                });
            });

            return app;
        }

        private void MapHealthCheckEndpoint(string pattern = "/health")
        {
            app.UseHealthChecks(pattern, new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("health"),
                ResponseWriter = ResponseWriters.GenericStatusWriterAsync
            });
        }
    }

    public static IEndpointRouteBuilder MapReactiveHealthStream(this IEndpointRouteBuilder app,
        string pattern = "/health/stream")
    {
        app.MapGet(pattern, async (HttpContext ctx, ReactiveHealthStream healthStream, CancellationToken ct) =>
        {
            ctx.Response.Headers.Append("Content-Type", "text/event-stream");
            ctx.Response.Headers.Append("Cache-Control", "no-cache");
            ctx.Response.Headers.Append("Connection", "keep-alive");

            var tcs = new TaskCompletionSource();

            using var subscription = healthStream.WhenHealthChanged().Subscribe(async void (report) =>
            {
                try
                {
                    var stream = HealthStream.From(report);
                    var healthJson = JsonSerializer.Serialize(stream);

                    await ctx.Response.WriteAsync($"data: {healthJson}\n\n", ct);
                    await ctx.Response.Body.FlushAsync(ct);

                }
                catch
                {
                    tcs.TrySetResult();
                }
            });

            await using (ctx.RequestAborted.Register(() => tcs.TrySetResult()))
            {
                await tcs.Task;
            }
        }).WithDisplayName("Reactive health stream")
            .ExcludeFromDescription();

        return app;
    }
}