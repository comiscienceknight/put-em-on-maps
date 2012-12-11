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

namespace Surface_Maps.Pages
{
    public sealed partial class ChangeCollectionNameFlyout : Common.LayoutAwarePage
    {
        const int ContentAnimationOffset = 100;
        TextBlock collectionNameTextBlock;

        public ChangeCollectionNameFlyout(TextBlock collectionNameTextBlock)
        {
            this.InitializeComponent();

            FlyoutContent.Transitions = new TransitionCollection();
            FlyoutContent.Transitions.Add(new EntranceThemeTransition()
            {
                FromHorizontalOffset = (SettingsPane.Edge == SettingsEdgeLocation.Right) ? ContentAnimationOffset : (ContentAnimationOffset * -1)
            });

            this.collectionNameTextBlock = collectionNameTextBlock;
            TextBox_NeedChangedCollectionName.Text = collectionNameTextBlock.Text;
        }

        private void MySettingsBackClicked(object sender, RoutedEventArgs e)
        {
            // First close our Flyout.
            Popup parent = this.Parent as Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }
        }

        private void Button_ChangeCollectionName_Click(object sender, RoutedEventArgs e)
        {
            collectionNameTextBlock.Text = TextBox_NeedChangedCollectionName.Text;
            Popup parent = this.Parent as Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }
        }
    }
}
