using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using ReactiveProbes.Configuration;
using ReactiveProbes.Probes;

namespace ReactiveProbes.Test;

public class ReactiveProbeInstallerTests
{
    [Fact]
    public void AddReactiveProbes_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging services
        var configMock = new Mock<IConfiguration>();
        var sectionMock = new Mock<IConfigurationSection>();
        sectionMock.Setup(s => s["Intervals:HealthChecks"]).Returns("1");
        configMock.Setup(c => c.GetSection("ProbeConfig")).Returns(sectionMock.Object);

        // Act
        services.AddReactiveProbes(configMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var probeConfig = serviceProvider.GetService<IOptions<ProbeConfig>>();
        Assert.NotNull(probeConfig);
        var observableProbe = serviceProvider.GetService<IObservableProbe>();
        Assert.NotNull(observableProbe);
    }

    [Fact]
    public void RegisterReactiveStartupProbe_SetsUpHealthCheckEndpoint()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHealthChecks();
        services.AddSingleton<IObservableProbe, ObservableProbe>();
        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        appBuilder.RegisterReactiveStartupProbe();

        // Assert
        var observableProbe = serviceProvider.GetService<IObservableProbe>();
        Assert.NotNull(observableProbe);
    }

    [Fact]
    public void AddLiveStatusEndpoint_SetsUpHealthCheckEndpoint()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHealthChecks();
        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        appBuilder.AddLiveStatusEndpoint();

        // Assert
        // No exception means the endpoint was set up correctly
    }

    [Fact]
    public void AddReactiveHealthProbes_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var healthChecksBuilder = services.AddHealthChecks();

        // Act
        healthChecksBuilder.AddReactiveHealthProbes();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var observableHealthProbes = serviceProvider.GetService<IObservableHealthProbes>();
        Assert.NotNull(observableHealthProbes);
    }

    [Fact]
    public void RegisterReactiveHealthProbe_SetsUpHealthCheckEndpoints()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHealthChecks();
        services.AddSingleton<IObservableHealthProbes, ObservableHealthProbes>();
        var serviceProvider = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(serviceProvider);

        // Act
        appBuilder.RegisterReactiveHealthProbe();

        // Assert
        var observableHealthProbes = serviceProvider.GetService<IObservableHealthProbes>();
        Assert.NotNull(observableHealthProbes);
    }
}