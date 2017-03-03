using UWPHelper.Utilities;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace FF_Casovac
{
    public abstract class PageBase : Page
    {
        protected readonly ApplicationView applicationView = ApplicationView.GetForCurrentView();

        public PageBase()
        {
            NavigationCacheMode = NavigationCacheMode.Required;

            Transitions = new TransitionCollection()
            {
                new NavigationThemeTransition()
                {
                    DefaultNavigationTransitionInfo = new DrillInNavigationTransitionInfo()
                }
            };

            DoubleTapped += (sender, e) =>
            {
                if (e.Handled)
                {
                    return;
                }

                e.Handled = true;
                SwitchFullScreenMode();
            };

            KeyboardHelper.CoreKeyDown += (sender, args) =>
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
            };
        }
        
        private void SwitchFullScreenMode()
        {
            if (applicationView.IsFullScreenMode)
            {
                applicationView.ExitFullScreenMode();
            }
            else
            {
                applicationView.TryEnterFullScreenMode();
            }
        }
    }
}
