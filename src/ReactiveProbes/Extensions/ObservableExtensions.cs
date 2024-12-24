using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ReactiveProbes.Extensions;

public static class ObservableExtensions
{
    public static IObservable<T> Gate<T>(this IObservable<T> source, IObservable<bool> controller)
    {
        return Observable.Create<T>(observer =>
        {
            var gateObj = new object();
            var isClosed = false;

            var subscription = source
                .Synchronize(gateObj)
                .Where(_ => !isClosed)
                .Subscribe(observer);

            var gateSubscription = controller
                .Synchronize(gateObj)
                .Subscribe(p => isClosed = p);

            return new CompositeDisposable(subscription, gateSubscription);
        });
    }
}