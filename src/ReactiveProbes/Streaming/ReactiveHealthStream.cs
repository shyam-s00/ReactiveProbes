using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ReactiveProbes.Streaming;

public class ReactiveHealthStream : IHostedService, IDisposable
{
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly HealthCheckService _healthCheckService;
    private readonly IConnectableObservable<HealthReport> _healthReportWhenHealthChanged;
    private IDisposable? _streamDisposable;
    private bool _disposedValue;

    public ReactiveHealthStream(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        
        _healthReportWhenHealthChanged = Observable.Interval(TimeSpan.FromSeconds(5))
            .SelectMany(_ => Observable.FromAsync(ct => _healthCheckService.CheckHealthAsync(ct)))
            .DistinctUntilChanged(report => report.Status)
            .Multicast(new ReplaySubject<HealthReport>(1));
        
    }

    public IObservable<HealthReport> WhenHealthChanged() => _healthReportWhenHealthChanged;
    
    public Task StartAsync(CancellationToken ct)
    {
        _streamDisposable = _healthReportWhenHealthChanged.Connect();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct)
    {
        _streamDisposable?.Dispose();
        return Task.CompletedTask;
    }

    protected virtual void DisposeStream(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _streamDisposable?.Dispose();
            }
            _disposedValue = true;
        }
    }
    
    public void Dispose()
    {
        DisposeStream(disposing: true);
        GC.SuppressFinalize(this);
    }
    
}