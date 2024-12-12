using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ReactiveProbes.Configuration;

namespace ReactiveProbes;

public class ObservableProbe(HealthCheckService healthCheckService, IOptions<ProbeConfig> config) : IObservableProbe
{
    private readonly CancellationTokenSource _cancellation = new();

    public IObservable<HealthReport> WhenHealthCheckChanged()
    {
        return Observable.Interval(TimeSpan.FromSeconds(config.Value.Interval))
            .Select(_ => healthCheckService.CheckHealthAsync(
                x => !x.Tags.Contains("startup"), _cancellation.Token).ToObservable())
            .Merge()
            .DistinctUntilChanged(x => x.Status)
            .TakeUntil(x => x.Status == HealthStatus.Healthy)
            .Do(_ => {}, () => _cancellation.Cancel())
            .Publish()
            .RefCount();
    }
}

public interface IObservableProbe
{
    IObservable<HealthReport> WhenHealthCheckChanged();
}