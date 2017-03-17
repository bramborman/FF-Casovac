using System;
using System.Threading.Tasks;
using UWPHelper.UI;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FF_Casovac
{
    public sealed partial class App : Application
    {
        public static new App Current { get; private set; }

        private SystemNavigationManager systemNavigationManager;
        private Frame rootFrame;

        public MediaElement GongSound { get; private set; }
        public MediaElement BeepSound { get; private set; }

        private bool CanGoBack
        {
            get
            {
                return rootFrame.CanGoBack && AppData.Current.IsTimerStoppingEnabled == true;
            }
        }

        public App()
        {
            Current = this;

            InitializeComponent();
            Suspending += OnSuspending;
        }
        
        private async Task<MediaElement> InitializeMediaElement(string fileName)
        {
            MediaElement output  = new MediaElement { AutoPlay = false };

            StorageFolder folder = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile file     = await folder.GetFileAsync(fileName + ".mp3");
            output.SetSource(await file.OpenAsync(FileAccessMode.Read), "audio/mpeg");

            return output;
        }
        
#pragma warning disable IDE1006 // Naming Styles
        protected override async void OnActivated(IActivatedEventArgs args)
#pragma warning restore IDE1006 // Naming Styles
        {
            bool loadAppData                        = AppData.Current == null;
            bool loadGongSound                      = GongSound == null;
            bool loadBeepSound                      = BeepSound == null;
            Task loadAppDataTask                    = null;
            Task<MediaElement> loadGongSoundTask    = null;
            Task<MediaElement> loadBeepSoundTask    = null;

            if (loadAppData)
            {
                loadAppDataTask = AppData.LoadAsync();
            }

            if (loadGongSound)
            {
                loadGongSoundTask = InitializeMediaElement("Gong");
            }

            if (loadBeepSound)
            {
                loadBeepSoundTask = InitializeMediaElement("Beep");
            }

            rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.Navigated += OnNavigated;
                rootFrame.NavigationFailed += OnNavigationFailed;

                systemNavigationManager = SystemNavigationManager.GetForCurrentView();
                systemNavigationManager.BackRequested += SystemNavigationManager_BackRequested;

                Window.Current.Content = rootFrame;
                ApplicationView.GetForCurrentView().FullScreenSystemOverlayMode = FullScreenSystemOverlayMode.Minimal;

                await BarsHelper.Current.InitializeForCurrentViewAsync();
            }

            LaunchActivatedEventArgs launchArgs = args as LaunchActivatedEventArgs;

            if (loadAppData)
            {
                await loadAppDataTask;
            }

            if (loadGongSound)
            {
                GongSound = await loadGongSoundTask;
                GongSound.Volume = 1.0;
            }

            if (loadBeepSound)
            {
                BeepSound = await loadBeepSoundTask;
                BeepSound.Volume = 0.2;
            }

            if (launchArgs?.PrelaunchActivated != true)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), launchArgs?.Arguments);
                }
                
                Window.Current.Activate();
            }
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            OnActivated(e);
        }

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            // Prevent switching to another app on phones
            if (rootFrame.Content.GetType() == typeof(Timer))
            {
                e.Handled = true;
            }

            if (CanGoBack)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            systemNavigationManager.AppViewBackButtonVisibility = CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

#pragma warning disable IDE1006 // Naming Styles
        private async void OnSuspending(object sender, SuspendingEventArgs e)
#pragma warning restore IDE1006 // Naming Styles
        {
            SuspendingDeferral deferral = e.SuspendingOperation.GetDeferral();

            await AppData.Current.SaveAsync();
            deferral.Complete();
        }
    }
}
