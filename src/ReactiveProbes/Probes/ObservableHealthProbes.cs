using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ReactiveProbes.Configuration;
using ReactiveProbes.Extensions;

namespace ReactiveProbes.Probes;

public class ObservableHealthProbes(HealthCheckService healthCheckService, IOptions<ProbeConfig> config) : IObservableHealthProbes
{
    private readonly Subject<bool> _gateController = new();
    private const int DefaultHealthCheckInterval = 60;
    
    /// <summary>
    /// Observes health check changes at specified intervals.
    /// </summary>
    /// <returns>An observable sequence of health reports of type <see cref="IObservable{HealthReport}"/></returns>
    public IObservable<HealthReport> WhenChanged()
    {
        var probeConfig = config.Value;
        
        return Observable.Create<HealthReport>(observer =>
            {
                // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                var subscription = Observable
                    .Interval(TimeSpan.FromSeconds(probeConfig?.Intervals.HealthChecks ?? DefaultHealthCheckInterval))
                    .Gate(_gateController)
                    .Select(_ => healthCheckService.CheckHealthAsync(checks =>
                        checks.Tags.Contains("health")).ToObservable())
                    .Merge()
                    .Subscribe(observer);

                return subscription;
            }
        );
    }

    /// <summary>
    /// Stops the health checks by closing the gate.
    /// </summary>
    public void Stop()
    {
        _gateController.OnNext(true);
    }

    /// <summary>
    /// Resumes the health checks by opening the gate.
    /// </summary>
    public void Start()
    {
        _gateController.OnNext(false);
    }
}


public interface IObservableHealthProbes
{
    /// <summary>
    /// Observes health check changes at specified intervals.
    /// </summary>
    /// <returns>An observable sequence of health reports of type <see cref="IObservable{HealthReport}"/></returns>
    IObservable<HealthReport> WhenChanged();
    
    /// <summary>
    /// Stops the health checks by closing the gate.
    /// </summary>
    void Stop();
    
    /// <summary>
    /// Resumes the health checks by opening the gate.
    /// </summary>
    void Start();
}