using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ReactiveProbes.Configuration;

namespace ReactiveProbes;

public class ObservableProbe(HealthCheckService healthCheckService, IOptions<ProbeConfig> config) : IObservableProbe
{
    private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
    
    /// <summary>
    /// Observes health check changes at specified intervals.
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// Observes health check changes at specified intervals.
    /// </summary>
    /// <returns>An Observable of type IObservable</returns>
    IObservable<HealthReport> WhenHealthCheckChanged();
}