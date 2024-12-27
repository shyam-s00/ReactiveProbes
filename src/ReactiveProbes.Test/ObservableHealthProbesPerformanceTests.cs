using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Moq;
using ReactiveProbes.Configuration;
using ReactiveProbes.Probes;
using Xunit;

namespace ReactiveProbes.Test;

public class ObservableHealthProbesPerformanceTests
    {
        private readonly ProbeConfig _config = new ProbeConfig { Intervals = new IntervalsConfig() { HealthChecks = 0 } };

        [Fact]
        public async Task WhenChanged_PerformanceUnderLoad()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(_config);

            var healthReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);
            healthCheckServiceMock.Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var probes = new ObservableHealthProbes(healthCheckServiceMock.Object, configMock.Object);

            var stopwatch = Stopwatch.StartNew();
            var results = await probes.WhenChanged().Take(1000).ToList().ToTask();
            stopwatch.Stop();

            Assert.Equal(1000, results.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 5000, "Performance test failed: took too long to emit 1000 health reports.");
        }

        [Fact]
        public async Task WhenChanged_PerformanceWithRapidChanges()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(_config);

            var healthyReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);
            var unhealthyReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Unhealthy, TimeSpan.Zero);

            healthCheckServiceMock.SetupSequence(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(unhealthyReport)
                .ReturnsAsync(healthyReport)
                .ReturnsAsync(unhealthyReport)
                .ReturnsAsync(healthyReport);

            var probes = new ObservableHealthProbes(healthCheckServiceMock.Object, configMock.Object);

            var stopwatch = Stopwatch.StartNew();
            var results = await probes.WhenChanged().Take(4).ToList().ToTask();
            stopwatch.Stop();

            Assert.Equal(4, results.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, "Performance test failed: took too long to handle rapid health status changes.");
        }
        
        [Fact]
        public async Task WhenChanged_PerformanceWithMixedHealthStatusChanges()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(_config);

            var healthyReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);
            var unhealthyReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Unhealthy, TimeSpan.Zero);
            var degradedReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Degraded, TimeSpan.Zero);

            healthCheckServiceMock.SetupSequence(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthyReport)
                .ReturnsAsync(unhealthyReport)
                .ReturnsAsync(degradedReport)
                .ReturnsAsync(healthyReport)
                .ReturnsAsync(unhealthyReport);

            var probes = new ObservableHealthProbes(healthCheckServiceMock.Object, configMock.Object);

            var stopwatch = Stopwatch.StartNew();
            var results = await probes.WhenChanged().Take(5).ToList().ToTask();
            stopwatch.Stop();

            Assert.Equal(5, results.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 2000, "Performance test failed: took too long to handle mixed health status changes.");
        }

        [Fact]
        public async Task WhenChanged_PerformanceWithHighFrequencyHealthChecks()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(new ProbeConfig { Intervals = new IntervalsConfig() { HealthChecks = 1 } });

            var healthReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);
            healthCheckServiceMock.Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var probes = new ObservableHealthProbes(healthCheckServiceMock.Object, configMock.Object);

            var stopwatch = Stopwatch.StartNew();
            var results = await probes.WhenChanged().Take(10).ToList().ToTask();
            stopwatch.Stop();

            Assert.Equal(10, results.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 10500, "Performance test failed: took too long to emit 100 health reports with high frequency.");
        }

        [Fact]
        public async Task WhenChanged_PerformanceWithLargeNumberOfHealthChecks()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(_config);

            var healthReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);
            healthCheckServiceMock.Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(healthReport);

            var probes = new ObservableHealthProbes(healthCheckServiceMock.Object, configMock.Object);

            var stopwatch = Stopwatch.StartNew();
            var results = await probes.WhenChanged().Take(10000).ToList().ToTask();
            stopwatch.Stop();

            Assert.Equal(10000, results.Count);
            Assert.True(stopwatch.ElapsedMilliseconds < 10000, "Performance test failed: took too long to emit 10000 health reports.");
        }
    }