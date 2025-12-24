using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ReactiveProbes.Streaming;

public class ReactiveHealthStream : IHostedService, IDisposable
{
    private readonly HealthCheckService _healthCheckService;
    private readonly IConnectableObservable<HealthReport> _healthReportStream;
    private IDisposable _streamDisposable;
    private bool _disposedValue;

    public ReactiveHealthStream(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
        
        _healthReportStream = Observable.Interval(TimeSpan.FromSeconds(5))
            .SelectMany(_ => Observable.FromAsync(ct => _healthCheckService.CheckHealthAsync(ct)))
            .DistinctUntilChanged(report => report.Status)
            .Multicast(new ReplaySubject<HealthReport>(1));
        
    }

    public IObservable<HealthReport> Stream => _healthReportStream;
    
    public Task StartAsync(CancellationToken ct)
    {
        _streamDisposable = _healthReportStream.Connect();
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