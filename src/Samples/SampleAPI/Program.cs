using ReactiveProbes;

namespace SampleAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // APIs health checks
        builder.Services.AddHealthChecks()
            .AddRestApiCheck("Google", "https://www.google.com", ["health"])
            .AddRestApiCheck("ReqRes", "https://reqres.in/api/unknown/", ["health"])
            //.AddRestApiCheck("SlowAPI", "https://reqres.in/api/users?delay=10", ["health", "startup"])
            //.AddCheck<CustomHealthCheck>("CustomHealthCheck", tags: ["custom"])
            //.AddCheck<ApiHealthCheck>(name: "ApiHealthCheck", tags: ["custom"])
            //.AddCheck<DbHealthCheck>(name: "DbHealthCheck", tags: ["custom"])
            .AddCheck<ExternalServiceHealthCheck>(name: "ExternalServiceHealthCheck", tags: ["custom"]);
        
        builder.Services.AddReactiveProbes(builder.Configuration);

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        
        app.MapHealthCheckEndpoint();
        app.RegisterReactiveStartupProbe();

        app.MapGet("/weatherforecast", (HttpContext httpContext) =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                        new WeatherForecast
                        {
                            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                            TemperatureC = Random.Shared.Next(-20, 55),
                            Summary = summaries[Random.Shared.Next(summaries.Length)]
                        })
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast")
            .WithOpenApi();

        app.Run();
    }
}