using System;
using System.Collections.Generic;
using System.Reactive;
using Toggl.Core.Interactors.Changes;
using Toggl.Core.Models.Interfaces;

namespace Toggl.Core.Interactors
{
    public sealed partial class InteractorFactory : IInteractorFactory
    {
        public IInteractor<IObservable<Unit>> ObserveWorkspaceOrTimeEntriesChanges()
            => new ObserveWorkspaceOrTimeEntriesChangesInteractor(this);
    }
}
