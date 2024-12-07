using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Moq;
using ReactiveProbes.Configuration;

namespace ReactiveProbes.Test
{
    public class ObservableProbeTests
    {
        [Fact]
        public async Task WhenHealthCheckChanged_EmitsHealthReportOnInterval()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(new ProbeConfig { Interval = 1 });

            var healthReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);
            healthCheckServiceMock.Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), default))
                .ReturnsAsync(healthReport);

            var probe = new ObservableProbe(healthCheckServiceMock.Object, configMock.Object);

            var result = await probe.WhenHealthCheckChanged().Take(1).ToTask();

            Assert.Equal(healthReport, result);
        }

        [Fact]
        public async Task WhenHealthCheckChanged_StopsEmittingWhenHealthy()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(new ProbeConfig { Interval = 1 });

            var unhealthyReport = new HealthReport(new Dictionary<string, HealthReportEntry>(),  HealthStatus.Unhealthy, TimeSpan.Zero);
            var healthyReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);

            healthCheckServiceMock.SetupSequence(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), default))
                .ReturnsAsync(unhealthyReport)
                .ReturnsAsync(healthyReport);

            var probe = new ObservableProbe(healthCheckServiceMock.Object, configMock.Object);

            var results = await probe.WhenHealthCheckChanged().ToList().ToTask();

            Assert.Contains(unhealthyReport, results);
            Assert.Contains(healthyReport, results);
            Assert.Equal(2, results.Count);
        }

        [Fact]
        public async Task WhenHealthCheckChanged_DoesNotEmitDuplicateStatus()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(new ProbeConfig { Interval = 1 });

            var healthReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);

            healthCheckServiceMock.Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), default))
                .ReturnsAsync(healthReport);

            var probe = new ObservableProbe(healthCheckServiceMock.Object, configMock.Object);

            var results = await probe.WhenHealthCheckChanged().Take(2).ToList().ToTask();

            Assert.Single(results);
        }
        
        [Fact]
        public async Task WhenHealthCheckChanged_EmitsDegradedHealthReport()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(new ProbeConfig { Interval = 1 });

            var degradedReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Degraded, TimeSpan.Zero);
            healthCheckServiceMock.Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), default))
                .ReturnsAsync(degradedReport);

            var probe = new ObservableProbe(healthCheckServiceMock.Object, configMock.Object);

            var result = await probe.WhenHealthCheckChanged().Take(1).ToTask();

            Assert.Equal(degradedReport, result);
        }
        
        [Fact]
        public async Task WhenHealthCheckChanged_HandlesRapidHealthStatusChanges()
        {
            var healthCheckServiceMock = new Mock<HealthCheckService>();
            var configMock = new Mock<IOptions<ProbeConfig>>();
            configMock.Setup(c => c.Value).Returns(new ProbeConfig { Interval = 1 });

            var healthyReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.Zero);
            var unhealthyReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Unhealthy, TimeSpan.Zero);
            var degradedReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Degraded, TimeSpan.Zero);

            healthCheckServiceMock.SetupSequence(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), default))
                .ReturnsAsync(unhealthyReport)
                .ReturnsAsync(degradedReport)
                .ReturnsAsync(healthyReport);

            var probe = new ObservableProbe(healthCheckServiceMock.Object, configMock.Object);

            var results = await probe.WhenHealthCheckChanged().Take(4).ToList().ToTask();

            Assert.Contains(healthyReport, results);
            Assert.Contains(unhealthyReport, results);
            Assert.Contains(degradedReport, results);
            Assert.Equal(3, results.Count);
        }
    }
}