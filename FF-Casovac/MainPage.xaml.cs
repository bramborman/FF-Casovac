using System;
using System.Linq;
using UWPHelper.UI;
using UWPHelper.Utilities;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace FF_Casovac
{
    public sealed partial class MainPage : Page
    {
        private readonly ThreadPoolTimer timer      = new ThreadPoolTimer(TimeSpan.FromSeconds(1));
        private readonly TimeSpan[] gongTimeSpans   = { TimeSpan.FromHours(1), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(3), TimeSpan.Zero };
        private readonly MediaElement gongSound     = new MediaElement { Volume = 1.0 };
        private readonly MediaElement beepSound     = new MediaElement { Volume = 0.2 };
        private readonly SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();

        private TimeSpan time;

        private AppData AppData
        {
            get { return AppData.Current; }
        }

        public MainPage()
        {
            InitializeSoundAsset(gongSound, "Gong.mp3");
            InitializeSoundAsset(beepSound, "Beep.mp3");

            timer.Tick += Timer_Tick;
            KeyboardHelper.CoreKeyDown += KeyboardHelper_CoreKeyDown;

            InitializeComponent();
        }

        private async void InitializeSoundAsset(MediaElement mediaElement, string fileName)
        {
            mediaElement.AutoPlay = false;

            StorageFolder folder = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            StorageFile file     = await folder.GetFileAsync(fileName);
            mediaElement.SetSource(await file.OpenAsync(FileAccessMode.Read), "");
        }

        private async void ShowAboutAppDialogAsync(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            AboutAppDialog aboutAppDialog = new AboutAppDialog();

            aboutAppDialog.AboutApp.AppStoreId          = "9n2kkzgfn9ks";
            aboutAppDialog.AboutApp.AppUri              = "md-ff-casovac:";
            aboutAppDialog.AboutApp.AppDeveloperMail    = "mariandolinsky@outlook.com";
            aboutAppDialog.AboutApp.IsGitHubLinkEnabled = true;
            aboutAppDialog.AboutApp.GitHubProjectName   = "FF Časovač";
            aboutAppDialog.AboutApp.GitHubLinkUrl       = "https://github.com/bramborman/FF-Casovac";

            await aboutAppDialog.ShowAsync();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (AppData.IsSoundEnabled == true)
            {
                if (gongTimeSpans.Any(t => time == t))
                {
                    gongSound.Play();
                }
                else if (time <= TimeSpan.FromSeconds(3))
                {
                    beepSound.Play();
                }
            }

            if (time <= gongTimeSpans[gongTimeSpans.Length - 2] && Sb_Blinking.GetCurrentState() == ClockState.Stopped)
            {
                Sb_Blinking.Begin();
            }

            if (time == TimeSpan.Zero)
            {
                timer.Stop();
            }
            else if (time <= TimeSpan.Zero)
            {
                timer.Stop();
                return;
            }

            TB_Hours.Text   = time.Hours.ToString("D2");
            TB_Minutes.Text = time.Minutes.ToString("D2");
            TB_Seconds.Text = time.Seconds.ToString("D2");

            time -= TimeSpan.FromSeconds(1);
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            SwitchInitializationUI();
        }

        private async void SwitchInitializationUI()
        {
            if (Bo_InitializationUI.Visibility == Visibility.Visible)
            {
                time = TP_Input.Time;

                if (AppData.IsAutomaticFullScreenModeEnabled == true)
                {
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                }

                systemNavigationManager.BackRequested += SystemNavigationManager_BackRequested;

                if (AppData.IsTimerStoppingEnabled == true)
                {
                    systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                }

                Bo_InitializationUI.Visibility = Visibility.Collapsed;
                timer.Start();
            }
            else
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();

                AdvancedContentDialog dialog = new AdvancedContentDialog
                {
                    Title               = "Přejete si pokračovat?",
                    Content             = "Budete-li pokračovat, odpočítávání bude přerušeno.",
                    PrimaryButtonText   = "Ano",
                    SecondaryButtonText = "Ne"
                };

                if (await dialog.ShowAsync() != ContentDialogResult.Primary)
                {
                    if (AppData.IsAutomaticFullScreenModeEnabled == true)
                    {
                        ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                    }
                }
                else
                {
                    timer.Stop();
                    Sb_Blinking.Stop();

                    if (time < TimeSpan.Zero)
                    {
                        time = TimeSpan.Zero;
                    }
                    else
                    {
                        time -= TimeSpan.FromSeconds(time.Seconds);
                    }

                    TP_Input.Time = time;
                    systemNavigationManager.BackRequested -= SystemNavigationManager_BackRequested;
                    systemNavigationManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                    
                    Bo_InitializationUI.Visibility = Visibility.Visible;
                }
            }
        }

        private void SwitchFullScreenMode()
        {
            ApplicationView applicationView = ApplicationView.GetForCurrentView();

            if (applicationView.IsFullScreenMode)
            {
                applicationView.ExitFullScreenMode();
            }
            else
            {
                applicationView.TryEnterFullScreenMode();
            }
        }

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;

            if (AppData.IsTimerStoppingEnabled == true)
            {
                SwitchInitializationUI();
            }
        }

        private void KeyboardHelper_CoreKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.Handled)
            {
                return;
            }

            switch (args.VirtualKey)
            {
                case VirtualKey.F11:
                    args.Handled = true;
                    SwitchFullScreenMode();
                    break;
            }
        }

        private void Page_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            SwitchFullScreenMode();
        }
    }
}
