﻿using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Core.UI.Navigation;
using Toggl.Core.UI.Services;
using Toggl.Core.Services;
using Toggl.Shared;
using Toggl.Shared.Extensions;

namespace Toggl.Core.UI.ViewModels.Calendar
{
    [Preserve(AllMembers = true)]
    public sealed class CalendarPermissionDeniedViewModel : ViewModelWithOutput<Unit>
    {
        private readonly INavigationService navigationService;
        private readonly IPermissionsService permissionsService;
        private readonly IRxActionFactory rxActionFactory;

        public UIAction EnableAccess { get; }
        public UIAction Close { get; }

        public CalendarPermissionDeniedViewModel(INavigationService navigationService, IPermissionsService permissionsService, IRxActionFactory rxActionFactory)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(permissionsService, nameof(permissionsService));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));

            this.navigationService = navigationService;
            this.permissionsService = permissionsService;

            EnableAccess = rxActionFactory.FromAction(enableAccess);
            Close = rxActionFactory.FromAsync(close);
        }

        private Task close()
            => navigationService.Close(this, Unit.Default);

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            closeIfPermissionIsGranted();
        }

        private void enableAccess()
        {
            permissionsService.OpenAppSettings();
        }

        private async Task closeIfPermissionIsGranted()
        {
            var authorized = await permissionsService.CalendarPermissionGranted;
            if (authorized)
                navigationService.Close(this, Unit.Default);
        }
    }
}
