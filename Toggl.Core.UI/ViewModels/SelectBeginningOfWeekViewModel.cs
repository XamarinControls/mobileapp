﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Toggl.Core.UI.Navigation;
using Toggl.Core.Services;
using Toggl.Shared;
using Toggl.Shared.Extensions;

namespace Toggl.Core.UI.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SelectBeginningOfWeekViewModel : ViewModel<BeginningOfWeek, BeginningOfWeek>
    {
        private readonly INavigationService navigationService;

        private BeginningOfWeek defaultResult;

        public SelectableBeginningOfWeekViewModel[] BeginningOfWeekCollection { get; }

        public UIAction Close { get; }
        public InputAction<SelectableBeginningOfWeekViewModel> SelectBeginningOfWeek { get; }

        public SelectBeginningOfWeekViewModel(INavigationService navigationService, IRxActionFactory rxActionFactory)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));

            this.navigationService = navigationService;

            Close = rxActionFactory.FromAsync(close);
            SelectBeginningOfWeek = rxActionFactory.FromAsync<SelectableBeginningOfWeekViewModel>(selectFormat);

            BeginningOfWeekCollection = Enum.GetValues(typeof(BeginningOfWeek))
                            .Cast<BeginningOfWeek>()
                            .Select(beginningOfWeek => new SelectableBeginningOfWeekViewModel(beginningOfWeek, false))
                            .ToArray();
        }

        public override void Prepare(BeginningOfWeek parameter)
        {
            defaultResult = parameter;
            updateSelectedFormat(parameter);
        }

        private Task close() => navigationService.Close(this, defaultResult);

        private Task selectFormat(SelectableBeginningOfWeekViewModel viewModel)
            => navigationService.Close(this, viewModel.BeginningOfWeek);

        private void updateSelectedFormat(BeginningOfWeek selected)
            => BeginningOfWeekCollection.ForEach(viewModel
                => viewModel.Selected = viewModel.BeginningOfWeek == selected);
    }
}
