﻿using NSubstitute;
using Toggl.Core.Analytics;
using Toggl.Core.DataSources;
using Toggl.Core.Interactors;
using Toggl.Core.Services;
using Toggl.Core.Shortcuts;
using Toggl.Storage;
using Toggl.Storage.Settings;
using Toggl.Networking.Network;
using Toggl.Core.Sync;
using Toggl.Core.Diagnostics;
using Toggl.Networking;
using Toggl.Core.Login;
using System;

namespace Toggl.Core.Tests
{
    public abstract class BaseInteractorTests
    {
        protected IIdProvider IdProvider { get; } = Substitute.For<IIdProvider>();
        protected ITimeService TimeService { get; } = Substitute.For<ITimeService>();
        protected ITogglDataSource DataSource { get; } = Substitute.For<ITogglDataSource>();
        protected ITogglApi Api { get; } = Substitute.For<ITogglApi>();
        protected IUserPreferences UserPreferences { get; } = Substitute.For<IUserPreferences>();
        protected IAnalyticsService AnalyticsService { get; } = Substitute.For<IAnalyticsService>();
        protected IIntentDonationService IntentDonationService { get; } = Substitute.For<IIntentDonationService>();
        protected IPlatformInfo PlatformInfo { get; } = Substitute.For<IPlatformInfo>();
        protected INotificationService NotificationService { get; } = Substitute.For<INotificationService>();
        protected ILastTimeUsageStorage LastTimeUsageStorage { get; } = Substitute.For<ILastTimeUsageStorage>();
        protected IApplicationShortcutCreator ApplicationShortcutCreator { get; }
            = Substitute.For<IApplicationShortcutCreator>();
        protected UserAgent UserAgent { get; } = new UserAgent("Tests", "0.0");
        protected ICalendarService CalendarService { get; } = Substitute.For<ICalendarService>();
        protected ISyncManager SyncManager { get; } = Substitute.For<ISyncManager>();
        protected IStopwatchProvider StopwatchProvider { get; } = Substitute.For<IStopwatchProvider>();
        protected ITogglDatabase Database { get; } = Substitute.For<ITogglDatabase>();
        protected IPrivateSharedStorageService PrivateSharedStorageService { get; } =
            Substitute.For<IPrivateSharedStorageService>();
        protected IUserAccessManager UserAccessManager { get; } = Substitute.For<IUserAccessManager>();

        protected IInteractorFactory InteractorFactory { get; }

        protected BaseInteractorTests()
        {
            InteractorFactory = new InteractorFactory(
                Api,
                UserAccessManager,
                new Lazy<IIdProvider>(() => IdProvider),
                new Lazy<ITogglDatabase>(() => Database),
                new Lazy<ITimeService>(() => TimeService),
                new Lazy<ISyncManager>(() => SyncManager),
                new Lazy<IPlatformInfo>(() => PlatformInfo),
                new Lazy<ITogglDataSource>(() => DataSource),
                new Lazy<ICalendarService>(() => CalendarService),
                new Lazy<IUserPreferences>(() => UserPreferences),
                new Lazy<IAnalyticsService>(() => AnalyticsService),
                new Lazy<IStopwatchProvider>(() => StopwatchProvider),
                new Lazy<INotificationService>(() => NotificationService),
                new Lazy<ILastTimeUsageStorage>(() => LastTimeUsageStorage),
                new Lazy<IApplicationShortcutCreator>(() => ApplicationShortcutCreator),
                new Lazy<IIntentDonationService>(() => IntentDonationService),
                new Lazy<IPrivateSharedStorageService>(() => PrivateSharedStorageService)
            );
        }
    }
}
