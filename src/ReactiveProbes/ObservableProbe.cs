using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ReactiveProbes.Configuration;

namespace ReactiveProbes;

public class ObservableProbe(HealthCheckService healthCheckService, IOptions<ProbeConfig> config) : IObservableProbe
{
    private readonly HealthCheckService _healthCheckService = healthCheckService;
    private readonly IOptions<ProbeConfig> _config = config;

    public IObservable<HealthReport> WhenHealthCheckChanged()
    {
        return Observable.Interval(TimeSpan.FromSeconds(_config.Value.Interval))
            .Select(_ => _healthCheckService.CheckHealthAsync(
                x => !x.Tags.Contains("startup")).ToObservable())
            .Merge()
            .DistinctUntilChanged(x => x.Status)
            .TakeUntil(x => x.Status == HealthStatus.Healthy)
            .Publish()
            .RefCount();
    }
}

public interface IObservableProbe
{
    IObservable<HealthReport> WhenHealthCheckChanged();
}