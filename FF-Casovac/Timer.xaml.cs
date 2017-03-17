using System;
using System.Linq;
using UWPHelper.UI;
using UWPHelper.Utilities;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace FF_Casovac
{
    public sealed partial class Timer : PageBase
    {
        private readonly ThreadPoolTimer timer      = new ThreadPoolTimer(TimeSpan.FromSeconds(1));
        private readonly TimeSpan[] gongTimeSpans   = { TimeSpan.FromHours(1), TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(3), TimeSpan.Zero };

        private bool cancelGoingBack;

        private AppData AppData
        {
            get { return AppData.Current; }
        }

        public Timer()
        {
            timer.Tick += Timer_Tick;
            InitializeComponent();
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (AppData.IsSoundEnabled == true)
            {
                if (gongTimeSpans.Any(t => AppData.ActualTimeSpan == t))
                {
                    App.Current.GongSound.Play();
                }
                else if (AppData.ActualTimeSpan <= TimeSpan.FromSeconds(3))
                {
                    App.Current.BeepSound.Play();
                }
            }

            if (AppData.ActualTimeSpan <= gongTimeSpans[gongTimeSpans.Length - 2] && Sb_Blinking.GetCurrentState() == ClockState.Stopped)
            {
                Sb_Blinking.Begin();
            }

            if (AppData.ActualTimeSpan == TimeSpan.Zero)
            {
                timer.Stop();
            }
            else if (AppData.ActualTimeSpan <= TimeSpan.Zero)
            {
                timer.Stop();
                return;
            }

            TB_Hours.Text   = AppData.ActualTimeSpan.Hours.ToString("D2");
            TB_Minutes.Text = AppData.ActualTimeSpan.Minutes.ToString("D2");
            TB_Seconds.Text = AppData.ActualTimeSpan.Seconds.ToString("D2");

            if (AppData.ActualTimeSpan != TimeSpan.Zero)
            {
                AppData.ActualTimeSpan -= TimeSpan.FromSeconds(1);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            cancelGoingBack = true;
            DisplayRequestHelper.IsActive = true;
            BarsHelper.Current.RequestedThemeGetter = () => ElementTheme.Light;

            if (AppData.IsAutomaticFullScreenModeEnabled == true)
            {
                applicationView.TryEnterFullScreenMode();
            }

            timer.Start();
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            BarsHelper.Current.RequestedThemeGetter = () => ElementTheme.Dark;
            applicationView.ExitFullScreenMode();

            if (cancelGoingBack)
            {
                e.Cancel = true;
                
                AdvancedContentDialog dialog = new AdvancedContentDialog
                {
                    Title               = "Přejete si pokračovat?",
                    Content             = "Budete-li pokračovat, odpočítávání bude přerušeno.",
                    PrimaryButtonText   = "Ano",
                    SecondaryButtonText = "Ne"
                };
                
                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    cancelGoingBack = false;
                    Frame.GoBack();
                }
                else
                {
                    BarsHelper.Current.RequestedThemeGetter = () => ElementTheme.Light;

                    if (AppData.IsAutomaticFullScreenModeEnabled == true)
                    {
                        applicationView.TryEnterFullScreenMode();
                    }
                }

                return;
            }


            DisplayRequestHelper.IsActive = false;

            timer.Stop();
            Sb_Blinking.Stop();
            App.Current.GongSound.Stop();
            App.Current.BeepSound.Stop();

            if (AppData.ActualTimeSpan < TimeSpan.Zero)
            {
                AppData.ActualTimeSpan = TimeSpan.Zero;
            }
            else
            {
                AppData.ActualTimeSpan -= TimeSpan.FromSeconds(AppData.ActualTimeSpan.Seconds);
            }

            base.OnNavigatingFrom(e);
        }
    }
}
