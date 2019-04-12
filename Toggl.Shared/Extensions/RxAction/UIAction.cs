using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Toggl.Shared.Extensions
{
    public sealed class UIAction : RxAction<Unit, Unit>
    {
        private UIAction(Func<IObservable<Unit>> workFactory, IScheduler mainScheduler, IObservable<bool> enabledIf)
            : base(_ => workFactory(), mainScheduler, enabledIf)
        {
        }

        public void Execute()
            => Execute(Unit.Default);

        public IObservable<Unit> ExecuteWithCompletion()
            => ExecuteWithCompletion(Unit.Default);

        public static UIAction FromAction(Action action, IScheduler mainScheduler, IObservable<bool> enabledIf = null)
        {
            IObservable<Unit> workFactory()
                => Observable.Create<Unit>(observer =>
                {
                    action();
                    observer.CompleteWith(Unit.Default);
                    return Disposable.Empty;
                });

            return new UIAction(workFactory, mainScheduler, enabledIf);
        }

        public static UIAction FromAsync(Func<Task> asyncAction, IScheduler mainScheduler, IObservable<bool> enabledIf = null)
        {
            IObservable<Unit> workFactory()
                => asyncAction().ToObservable();

            return new UIAction(workFactory, mainScheduler, enabledIf);
        }

        public static UIAction FromObservable(Func<IObservable<Unit>> workFactory, IScheduler mainScheduler, IObservable<bool> enabledIf = null)
            => new UIAction(workFactory, mainScheduler, enabledIf);
    }

    public static class RxActionExtensions
    {
        public static void Execute(this UIAction action)
        {
            action.Execute(Unit.Default);
        }
    }
}
