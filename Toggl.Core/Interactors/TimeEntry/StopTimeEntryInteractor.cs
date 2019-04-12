﻿using System;
using System.Linq;
using System.Reactive.Linq;
using Toggl.Core.Analytics;
using Toggl.Core.DataSources;
using Toggl.Core.DataSources.Interfaces;
using Toggl.Core.Exceptions;
using Toggl.Core.Extensions;
using Toggl.Core.Models;
using Toggl.Core.Models.Interfaces;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Storage.Models;

namespace Toggl.Core.Interactors
{
    internal class StopTimeEntryInteractor : IInteractor<IObservable<IThreadSafeTimeEntry>>
    {
        private readonly DateTimeOffset stopTime;
        private readonly TimeEntryStopOrigin origin;
        private readonly ITimeService timeService;
        private readonly IAnalyticsService analyticsService;
        private readonly IObservableDataSource<IThreadSafeTimeEntry, IDatabaseTimeEntry> dataSource;

        public StopTimeEntryInteractor(
            ITimeService timeService,
            IObservableDataSource<IThreadSafeTimeEntry, IDatabaseTimeEntry> dataSource,
            DateTimeOffset stopTime,
            IAnalyticsService analyticsService,
            TimeEntryStopOrigin origin)
        {
            Ensure.Argument.IsNotNull(stopTime, nameof(stopTime));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(origin, nameof(origin));

            this.stopTime = stopTime;
            this.dataSource = dataSource;
            this.timeService = timeService;
            this.analyticsService = analyticsService;
            this.origin = origin;
        }

        public IObservable<IThreadSafeTimeEntry> Execute()
            => dataSource.GetAll(te => te.IsDeleted == false && te.Duration == null, includeInaccessibleEntities: true)
                .Select(timeEntries => timeEntries.SingleOrDefault() ?? throw new NoRunningTimeEntryException())
                .SelectMany(timeEntry => timeEntry
                    .With((long)(stopTime - timeEntry.Start).TotalSeconds)
                    .UpdatedAt(timeService.CurrentDateTime)
                    .Apply(dataSource.Update))
                .Do(notifyTimeEntryStopped)
                .Do(() => { analyticsService.TimeEntryStopped.Track(origin); });

        private void notifyTimeEntryStopped(IThreadSafeTimeEntry timeEntry)
        {
            if (dataSource is TimeEntriesDataSource timeEntriesDataSource)
                timeEntriesDataSource.OnTimeEntryStopped(timeEntry);
        }
    }
}