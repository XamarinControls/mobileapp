﻿using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Toggl.Core.Analytics;
using Toggl.Core.DataSources;
using Toggl.Core.Diagnostics;
using Toggl.Core.Interactors;
using Toggl.Core.Login;
using Toggl.Core.Services;
using Toggl.Core.Shortcuts;
using Toggl.Core.Suggestions;
using Toggl.Core.Sync;
using Toggl.Shared;
using Toggl.Shared.Extensions;
using Toggl.Storage;
using Toggl.Networking;
using Toggl.Networking.Network;
using Toggl.Storage.Settings;

namespace Toggl.Core
{
    public abstract class DependencyContainer
    {
        private readonly UserAgent userAgent;
        private readonly CompositeDisposable disposeBag = new CompositeDisposable();

        // Require recreation during login/logout
        private Lazy<ITogglApi> api;
        private Lazy<ISyncManager> syncManager;
        private Lazy<IInteractorFactory> interactorFactory;

        // Normal dependencies
        private readonly Lazy<IApiFactory> apiFactory;
        private readonly Lazy<ITogglDatabase> database;
        private readonly Lazy<ITimeService> timeService;
        private readonly Lazy<IPlatformInfo> platformInfo;
        private readonly Lazy<ITogglDataSource> dataSource;
        private readonly Lazy<IGoogleService> googleService;
        private readonly Lazy<IRatingService> ratingService;
        private readonly Lazy<ICalendarService> calendarService;
        private readonly Lazy<IKeyValueStorage> keyValueStorage;
        private readonly Lazy<ILicenseProvider> licenseProvider;
        private readonly Lazy<IUserPreferences> userPreferences;
        private readonly Lazy<IRxActionFactory> rxActionFactory;
        private readonly Lazy<IAnalyticsService> analyticsService;
        private readonly Lazy<IBackgroundService> backgroundService;
        private readonly Lazy<IOnboardingStorage> onboardingStorage;
        private readonly Lazy<ISchedulerProvider> schedulerProvider;
        private readonly Lazy<IStopwatchProvider> stopwatchProvider;
        private readonly Lazy<INotificationService> notificationService;
        private readonly Lazy<IRemoteConfigService> remoteConfigService;
        private readonly Lazy<IErrorHandlingService> errorHandlingService;
        private readonly Lazy<ILastTimeUsageStorage> lastTimeUsageStorage;
        private readonly Lazy<IApplicationShortcutCreator> shortcutCreator;
        private readonly Lazy<IBackgroundSyncService> backgroundSyncService;
        private readonly Lazy<IIntentDonationService> intentDonationService;
        private readonly Lazy<IAutomaticSyncingService> automaticSyncingService;
        private readonly Lazy<IAccessRestrictionStorage> accessRestrictionStorage;
        private readonly Lazy<ISyncErrorHandlingService> syncErrorHandlingService;
        private readonly Lazy<IPrivateSharedStorageService> privateSharedStorageService;
        private readonly Lazy<ISuggestionProviderContainer> suggestionProviderContainer;
        
        // Non lazy
        public virtual IUserAccessManager UserAccessManager { get; }
        public ApiEnvironment ApiEnvironment { get; }

        public ISyncManager SyncManager => syncManager.Value;
        public IInteractorFactory InteractorFactory => interactorFactory.Value;

        public IApiFactory ApiFactory => apiFactory.Value;
        public ITogglDatabase Database => database.Value;
        public ITimeService TimeService => timeService.Value;
        public IPlatformInfo PlatformInfo => platformInfo.Value;
        public ITogglDataSource DataSource => dataSource.Value;
        public IRatingService RatingService => ratingService.Value;
        public IKeyValueStorage KeyValueStorage => keyValueStorage.Value;
        public ILicenseProvider LicenseProvider => licenseProvider.Value;
        public IUserPreferences UserPreferences => userPreferences.Value;
        public IRxActionFactory RxActionFactory => rxActionFactory.Value;
        public IAnalyticsService AnalyticsService => analyticsService.Value;
        public IBackgroundService BackgroundService => backgroundService.Value;
        public IOnboardingStorage OnboardingStorage => onboardingStorage.Value;
        public IStopwatchProvider StopwatchProvider => stopwatchProvider.Value;
        public ISchedulerProvider SchedulerProvider => schedulerProvider.Value;
        public IRemoteConfigService RemoteConfigService => remoteConfigService.Value;
        public IErrorHandlingService ErrorHandlingService => errorHandlingService.Value;
        public ILastTimeUsageStorage LastTimeUsageStorage => lastTimeUsageStorage.Value;
        public IBackgroundSyncService BackgroundSyncService => backgroundSyncService.Value;
        public IIntentDonationService IntentDonationService => intentDonationService.Value;
        public IAutomaticSyncingService AutomaticSyncingService => automaticSyncingService.Value;
        public IAccessRestrictionStorage AccessRestrictionStorage => accessRestrictionStorage.Value;
        public ISyncErrorHandlingService SyncErrorHandlingService => syncErrorHandlingService.Value;
        public IPrivateSharedStorageService PrivateSharedStorageService => privateSharedStorageService.Value;
        public ISuggestionProviderContainer SuggestionProviderContainer => suggestionProviderContainer.Value;

        protected DependencyContainer(ApiEnvironment apiEnvironment, UserAgent userAgent)
        {
            this.userAgent = userAgent;

            ApiEnvironment = apiEnvironment;

            database = new Lazy<ITogglDatabase>(CreateDatabase);
            apiFactory = new Lazy<IApiFactory>(CreateApiFactory);
            syncManager = new Lazy<ISyncManager>(CreateSyncManager);
            timeService = new Lazy<ITimeService>(CreateTimeService);
            dataSource = new Lazy<ITogglDataSource>(CreateDataSource);
            platformInfo = new Lazy<IPlatformInfo>(CreatePlatformInfo);
            googleService = new Lazy<IGoogleService>(CreateGoogleService);
            ratingService = new Lazy<IRatingService>(CreateRatingService);
            calendarService = new Lazy<ICalendarService>(CreateCalendarService);
            keyValueStorage = new Lazy<IKeyValueStorage>(CreateKeyValueStorage);
            licenseProvider = new Lazy<ILicenseProvider>(CreateLicenseProvider);
            rxActionFactory = new Lazy<IRxActionFactory>(CreateRxActionFactory);
            userPreferences = new Lazy<IUserPreferences>(CreateUserPreferences);
            analyticsService = new Lazy<IAnalyticsService>(CreateAnalyticsService);
            backgroundService = new Lazy<IBackgroundService>(CreateBackgroundService);
            interactorFactory = new Lazy<IInteractorFactory>(CreateInteractorFactory);
            onboardingStorage = new Lazy<IOnboardingStorage>(CreateOnboardingStorage);
            schedulerProvider = new Lazy<ISchedulerProvider>(CreateSchedulerProvider);
            stopwatchProvider = new Lazy<IStopwatchProvider>(CreateStopwatchProvider);
            shortcutCreator = new Lazy<IApplicationShortcutCreator>(CreateShortcutCreator);
            notificationService = new Lazy<INotificationService>(CreateNotificationService);
            remoteConfigService = new Lazy<IRemoteConfigService>(CreateRemoteConfigService);
            errorHandlingService = new Lazy<IErrorHandlingService>(CreateErrorHandlingService);
            lastTimeUsageStorage = new Lazy<ILastTimeUsageStorage>(CreateLastTimeUsageStorage);
            backgroundSyncService = new Lazy<IBackgroundSyncService>(CreateBackgroundSyncService);
            intentDonationService = new Lazy<IIntentDonationService>(CreateIntentDonationService);
            automaticSyncingService = new Lazy<IAutomaticSyncingService>(CreateAutomaticSyncingService);
            accessRestrictionStorage = new Lazy<IAccessRestrictionStorage>(CreateAccessRestrictionStorage);
            syncErrorHandlingService = new Lazy<ISyncErrorHandlingService>(CreateSyncErrorHandlingService);
            privateSharedStorageService = new Lazy<IPrivateSharedStorageService>(CreatePrivateSharedStorageService);
            suggestionProviderContainer = new Lazy<ISuggestionProviderContainer>(CreateSuggestionProviderContainer);

            api = apiFactory.Select(factory => factory.CreateApiWith(Credentials.None));
            UserAccessManager = new UserAccessManager(
                apiFactory,
                database,
                googleService,
                privateSharedStorageService);

            UserAccessManager
                .UserLoggedIn
                .Subscribe(recreateLazyDependenciesForLogin)
                .DisposedBy(disposeBag);

            UserAccessManager
                .UserLoggedOut
                .Subscribe(_ => recreateLazyDependenciesForLogout())
                .DisposedBy(disposeBag);
        }
        
        protected abstract ITogglDatabase CreateDatabase();
        protected abstract IPlatformInfo CreatePlatformInfo();
        protected abstract IGoogleService CreateGoogleService();
        protected abstract IRatingService CreateRatingService();
        protected abstract ICalendarService CreateCalendarService();
        protected abstract IKeyValueStorage CreateKeyValueStorage();
        protected abstract ILicenseProvider CreateLicenseProvider();
        protected abstract IUserPreferences CreateUserPreferences();
        protected abstract IAnalyticsService CreateAnalyticsService();
        protected abstract IOnboardingStorage CreateOnboardingStorage();
        protected abstract IStopwatchProvider CreateStopwatchProvider();
        protected abstract ISchedulerProvider CreateSchedulerProvider();
        protected abstract INotificationService CreateNotificationService();
        protected abstract IRemoteConfigService CreateRemoteConfigService();
        protected abstract IErrorHandlingService CreateErrorHandlingService();
        protected abstract ILastTimeUsageStorage CreateLastTimeUsageStorage();
        protected abstract IApplicationShortcutCreator CreateShortcutCreator();
        protected abstract IBackgroundSyncService CreateBackgroundSyncService();
        protected abstract IIntentDonationService CreateIntentDonationService();
        protected abstract IAccessRestrictionStorage CreateAccessRestrictionStorage();
        protected abstract IPrivateSharedStorageService CreatePrivateSharedStorageService();
        protected abstract ISuggestionProviderContainer CreateSuggestionProviderContainer();

        protected virtual ITimeService CreateTimeService()
            => new TimeService(SchedulerProvider.DefaultScheduler);

        protected virtual IBackgroundService CreateBackgroundService()
            => new BackgroundService(TimeService, AnalyticsService);

        protected virtual IAutomaticSyncingService CreateAutomaticSyncingService()
            => new AutomaticSyncingService(BackgroundService, TimeService);

        protected virtual ISyncErrorHandlingService CreateSyncErrorHandlingService()
            => new SyncErrorHandlingService(ErrorHandlingService);

        protected virtual ITogglDataSource CreateDataSource()
            => new TogglDataSource(Database, TimeService, AnalyticsService);

        protected virtual IRxActionFactory CreateRxActionFactory()
            => new RxActionFactory(SchedulerProvider);

        protected virtual IApiFactory CreateApiFactory()
            => new ApiFactory(ApiEnvironment, userAgent);

        protected virtual ISyncManager CreateSyncManager()
        {
            var syncManager = TogglSyncManager.CreateSyncManager(
                Database,
                api.Value,
                DataSource,
                TimeService,
                AnalyticsService,
                LastTimeUsageStorage,
                SchedulerProvider.DefaultScheduler,
                StopwatchProvider,
                AutomaticSyncingService
            );
            SyncErrorHandlingService.HandleErrorsOf(syncManager);

            return syncManager;
        }

        protected virtual IInteractorFactory CreateInteractorFactory() => new InteractorFactory(
            api.Value,
            UserAccessManager,
            database.Select(database => database.IdProvider),
            database,
            timeService,
            syncManager,
            platformInfo,
            dataSource,
            calendarService,
            userPreferences,
            analyticsService,
            stopwatchProvider,
            notificationService,
            lastTimeUsageStorage,
            shortcutCreator,
            intentDonationService,
            privateSharedStorageService
        );

        private void recreateLazyDependenciesForLogin(ITogglApi api)
        {
            this.api = new Lazy<ITogglApi>(() => api);

            syncManager = new Lazy<ISyncManager>(CreateSyncManager);
            interactorFactory = new Lazy<IInteractorFactory>(CreateInteractorFactory);
        }

        private void recreateLazyDependenciesForLogout()
        {
            api = apiFactory.Select(factory => factory.CreateApiWith(Credentials.None));

            syncManager = new Lazy<ISyncManager>(CreateSyncManager);
            interactorFactory = new Lazy<IInteractorFactory>(CreateInteractorFactory);
        }
    }
}
