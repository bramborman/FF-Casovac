using System;
using UWPHelper.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace FF_Casovac
{
    public sealed partial class MainPage : PageBase
    {
        private AppData AppData
        {
            get { return AppData.Current; }
        }

        public MainPage()
        {
            InitializeComponent();

            Loaded += async (sender, e) =>
            {
                if (AppData.ShowLoadingError)
                {
                    AppData.ShowLoadingError = false;

                    if (await new LoadingErrorDialog("nastavení", "s výchozím nastavením").ShowAsync() == ContentDialogResult.Primary)
                    {
                        Application.Current.Exit();
                    }
                    else
                    {
                        await AppData.Current.SaveAsync();
                    }
                }
            };
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

        private void Start(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Timer));
        }
    }
}
