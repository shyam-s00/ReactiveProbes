using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Moq;
using ReactiveProbes.Configuration;
using ReactiveProbes.Probes;

namespace ReactiveProbes.Test
{
    public class ObservableHealthProbesTests
    {
        private readonly ProbeConfig _config = new ProbeConfig { Intervals = new IntervalsConfig() { HealthChecks = 1 } };

        [Fact]
        public async Task WhenChanged_EmitsHealthReportOnInterval()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(_config);

            var healthReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);
            healthCheckServiceMock.Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var probes = new ObservableHealthProbes(healthCheckServiceMock.Object, configMock.Object);

            var result = await probes.WhenChanged().Take(1).ToTask();

            Assert.NotNull(result);
            Assert.Equal(healthReport, result);
        }
      
        [Fact]
        public async Task WhenChanged_EmitsHealthReportsContinuously()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(_config);

            var unhealthyReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Unhealthy, TimeSpan.Zero);
            var healthyReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);

            healthCheckServiceMock.SetupSequence(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(unhealthyReport)
                .ReturnsAsync(healthyReport)
                .ReturnsAsync(healthyReport);

            var probes = new ObservableHealthProbes(healthCheckServiceMock.Object, configMock.Object);

            var results = await probes.WhenChanged().Take(3).ToList().ToTask();

            Assert.Contains(unhealthyReport, results);
            Assert.Equal(2, results.Count(r => r.Status == HealthStatus.Healthy));
        }
        
        [Fact]
        public async Task WhenChanged_EmitsDegradedHealthReport()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(_config);

            var degradedReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Degraded, TimeSpan.Zero);
            healthCheckServiceMock.Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(degradedReport);

            var probes = new ObservableHealthProbes(healthCheckServiceMock.Object, configMock.Object);

            var result = await probes.WhenChanged().Take(1).ToTask();

            Assert.NotNull(result);
            Assert.Equal(degradedReport, result);
        }

        [Fact]
        public async Task WhenChanged_HandlesRapidHealthStatusChanges()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(_config);

            var healthyReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);
            var unhealthyReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Unhealthy, TimeSpan.Zero);
            var degradedReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Degraded, TimeSpan.Zero);

            healthCheckServiceMock.SetupSequence(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(unhealthyReport)
                .ReturnsAsync(degradedReport)
                .ReturnsAsync(healthyReport);

            var probes = new ObservableHealthProbes(healthCheckServiceMock.Object, configMock.Object);

            var results = await probes.WhenChanged().Take(3).ToList().ToTask();

            Assert.Contains(healthyReport, results);
            Assert.Contains(unhealthyReport, results);
            Assert.Contains(degradedReport, results);
            Assert.Equal(3, results.Count);
        }
        
        [Fact]
        public async Task WhenChanged_EmitsValuesOnlyWhenStarted()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(_config);

            var healthReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);
            healthCheckServiceMock.Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var probes = new ObservableHealthProbes(healthCheckServiceMock.Object, configMock.Object);
            
            var emittedValues = new List<HealthReport>();
            var subscription = probes.WhenChanged().Subscribe(report => emittedValues.Add(report));

            // Start the probes
            probes.Start();

            // Wait for a short interval to ensure the probes have started
            await Task.Delay(2000);

            // Dispose the subscription
            subscription.Dispose();

            // Verify at least one emission after starting
            Assert.NotEmpty(emittedValues);
        }

        [Fact]
        public async Task WhenChanged_StopsEmittingValuesWhenStopped()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(_config);

            var healthReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);
            healthCheckServiceMock.Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var probes = new ObservableHealthProbes(healthCheckServiceMock.Object, configMock.Object);
            
            
            var emittedValues = new List<HealthReport>();
            var subscription = probes.WhenChanged().Subscribe(report => emittedValues.Add(report));

            // Start the probes
            probes.Start();

            // Wait for a short interval to ensure the probes have started
            await Task.Delay(1000);

            // Stop the probes
            probes.Stop();

            // Wait for a short interval to ensure no more values are emitted 
            // and clear any values that may have been emitted
            emittedValues.Clear();
            await Task.Delay(1000);

            // Dispose the subscription
            subscription.Dispose();

            // Verify no emissions after stopping
            Assert.Empty(emittedValues);
        }

    }
}