using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ReactiveProbes.Configuration;

namespace ReactiveProbes.Probes;

public class ObservableProbe(HealthCheckService healthCheckService, IOptions<ProbeConfig> config) : IObservableProbe
{
    private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
    private const int DefaultStartupInterval = 10;
    
    /// <summary>
    /// Observes health check changes at specified intervals.
    /// </summary>
    /// <returns>Observable of type <see cref="IObservable{HealthReport}"/></returns>
    public IObservable<HealthReport> WhenHealthCheckChanged()
    {
        var probeConfig = config.Value;
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        return Observable.Interval(TimeSpan.FromSeconds(probeConfig?.Intervals.Startup ?? DefaultStartupInterval))
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
    /// <returns>An Observable of type <see cref="IObservable{HealthReport}"/></returns>
    IObservable<HealthReport> WhenHealthCheckChanged();
}