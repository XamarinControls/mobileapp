﻿using Android.App;
using Android.Content;
using MvvmCross;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Navigation;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Presenters;
using MvvmCross.Platforms.Android.Views;
using MvvmCross.Plugin;
using MvvmCross.ViewModels;
using Toggl.Core;
using Toggl.Core.UI;
using Toggl.Core.UI.ViewModels;
using Toggl.Core.Services;
using Toggl.Droid.BroadcastReceivers;
using Toggl.Droid.Presentation;
using Toggl.Droid.Startup;
using Toggl.Networking;
using Toggl.Core.UI.Navigation;

namespace Toggl.Droid
{
    public sealed partial class Setup : MvxAppCompatSetup<App<LoginViewModel>>
    {
        private const ApiEnvironment environment =
            #if USE_PRODUCTION_API
                        ApiEnvironment.Production;
            #else
                        ApiEnvironment.Staging;
            #endif

        public Setup()
        {
            #if !USE_PRODUCTION_API
            System.Net.ServicePointManager.ServerCertificateValidationCallback
                  += (sender, certificate, chain, sslPolicyErrors) => true;
            #endif

            var applicationContext = Application.Context;
            var packageInfo = applicationContext.PackageManager.GetPackageInfo(applicationContext.PackageName, 0);

            AndroidDependencyContainer.EnsureInitialized(environment, Platform.Giskard, packageInfo.VersionName);
        }

        protected override IMvxNavigationService InitializeNavigationService(IMvxViewModelLocatorCollection collection)
        {
            var loader = CreateViewModelLoader(collection);
            Mvx.RegisterSingleton(loader);

            var container = AndroidDependencyContainer.Instance;
            container.MvxNavigationService =
                new NavigationService(null, loader, container.AnalyticsService);

            Mvx.RegisterSingleton<IMvxNavigationService>(container.MvxNavigationService);
            return container.MvxNavigationService;
        }

        protected override IMvxApplication CreateApp()
            => new App<LoginViewModel>(AndroidDependencyContainer.Instance);

        protected override IMvxAndroidViewPresenter CreateViewPresenter()
            => new TogglPresenter(AndroidViewAssemblies);

        protected override void InitializeApp(IMvxPluginManager pluginManager, IMvxApplication app)
        {
            var dependencyContainer = AndroidDependencyContainer.Instance;
            ApplicationContext.RegisterReceiver(new TimezoneChangedBroadcastReceiver(dependencyContainer.TimeService),
                new IntentFilter(Intent.ActionTimezoneChanged));
            
            createApplicationLifecycleObserver(dependencyContainer.BackgroundService);

            base.InitializeApp(pluginManager, app);
        }

        protected override IMvxAndroidCurrentTopActivity CreateAndroidCurrentTopActivity()
        {
            var mvxApplication = MvxAndroidApplication.Instance;
            var activityLifecycleCallbacksManager = new QueryableMvxLifecycleMonitorCurrentTopActivity();
            mvxApplication.RegisterActivityLifecycleCallbacks(activityLifecycleCallbacksManager);
            return activityLifecycleCallbacksManager;
        }

        // Skip the sluggish and reflection-based manager and load our plugins by hand
        protected override IMvxPluginManager InitializePluginFramework()
        {
            LoadPlugins(null);
            return null;
        }

        public override void LoadPlugins(IMvxPluginManager pluginManager)
        {
        }

        protected override void PerformBootstrapActions()
        {
            // This method uses reflection to find classes that inherit from
            // IMvxBootstrapAction, creates instances of these classes and then
            // calls their Run method. We can skip it since we don't have such classes.
        }

        private void createApplicationLifecycleObserver(IBackgroundService backgroundService)
        {
            var mvxApplication = MvxAndroidApplication.Instance;
            var appLifecycleObserver = new ApplicationLifecycleObserver(backgroundService);
            mvxApplication.RegisterActivityLifecycleCallbacks(appLifecycleObserver);
            mvxApplication.RegisterComponentCallbacks(appLifecycleObserver);
        }
    }
}
