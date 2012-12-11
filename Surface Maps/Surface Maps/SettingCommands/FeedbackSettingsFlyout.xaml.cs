using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Surface_Maps.SettingCommands
{
    public sealed partial class FeedbackSettingsFlyout : Common.LayoutAwarePage
    {
        // The guidelines recommend using 100px offset for the content animation.
        const int ContentAnimationOffset = 100;

        public FeedbackSettingsFlyout()
        {
            this.InitializeComponent();

            FlyoutContent.Transitions = new TransitionCollection();
            FlyoutContent.Transitions.Add(new EntranceThemeTransition()
            {
                FromHorizontalOffset = (SettingsPane.Edge == SettingsEdgeLocation.Right) ? ContentAnimationOffset : (ContentAnimationOffset * -1)
            });
        }

        private void MySettingsBackClicked(object sender, RoutedEventArgs e)
        {
            // First close our Flyout.
            Popup parent = this.Parent as Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }

            // If the app is not snapped, then the back button shows the Settings pane again.
            if (Windows.UI.ViewManagement.ApplicationView.Value != Windows.UI.ViewManagement.ApplicationViewState.Snapped)
            {
                SettingsPane.Show();
            }
        }

        private async void TextBlock_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            var mailto = new Uri("mailto:?to=comiscience@hotmail.fr&subject=I want to tell you...&body=Send from my Windows RT device");
            await Windows.System.Launcher.LaunchUriAsync(mailto);
        }
    }
}
