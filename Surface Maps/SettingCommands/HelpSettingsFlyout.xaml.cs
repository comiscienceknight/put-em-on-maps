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
using System.ComponentModel;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Surface_Maps.SettingCommands
{
    public sealed partial class HelpSettingsFlyout : Common.LayoutAwarePage, INotifyPropertyChanged
    {
        // The guidelines recommend using 100px offset for the content animation.
        const int ContentAnimationOffset = 100;

        private double imagesHeight;
        public double ImagesHeight
        {
            get { return imagesHeight;}
            set
            {
                imagesHeight = value;
                NotifyPropertyChanged("ImagesHeight");
            }
        }

        private double imagesTextWidth;
        public double ImagesTextWidth
        {
            get { return imagesTextWidth; }
            set
            {
                imagesTextWidth = value;
                NotifyPropertyChanged("ImagesTextWidth");
            }
        }


        public HelpSettingsFlyout()
        {
            this.InitializeComponent();

            FlyoutContent.Transitions = new TransitionCollection();
            FlyoutContent.Transitions.Add(new EntranceThemeTransition()
            {
                FromHorizontalOffset = (SettingsPane.Edge == SettingsEdgeLocation.Right) ? ContentAnimationOffset : (ContentAnimationOffset * -1)
            });

            DataContext = this;

            ImagesHeight = Utils.Constants.ScreenHeight / 1.5 - 20;
            ImagesTextWidth = Utils.Constants.ScreenWidth / 1.5 - 30;

            TextBlock_HelpPage1Title.Text = Utils.Constants.ResourceLoader.GetString("2howtousemainpagetitle");
            Image_Guide1.Source = new BitmapImage(new Uri(Utils.Constants.ResourceLoader.GetString("2howtousemainpagepicuri"), UriKind.RelativeOrAbsolute));
            TextBlock_HelpPage1Text.Text = Utils.Constants.ResourceLoader.GetString("2howtousemainpage");

            TextBlock_HelpPage2Title.Text = Utils.Constants.ResourceLoader.GetString("2howtousemapviewtitle");
            Image_Guide2.Source = new BitmapImage(new Uri(Utils.Constants.ResourceLoader.GetString("2howtousemapviewpicuri"), UriKind.RelativeOrAbsolute));
            TextBlock_HelpPage2Text.Text = Utils.Constants.ResourceLoader.GetString("2howtousemapview");

            TextBlock_HelpPage3Title.Text = Utils.Constants.ResourceLoader.GetString("2howtousefolderpagetitle");
            Image_Guide3.Source = new BitmapImage(new Uri(Utils.Constants.ResourceLoader.GetString("2howtousefolderpagepicuri"), UriKind.RelativeOrAbsolute));
            TextBlock_HelpPage3Text.Text = Utils.Constants.ResourceLoader.GetString("2howtousefolderpage");

            TextBlock_HelpPage4Title.Text = Utils.Constants.ResourceLoader.GetString("2howtouseeditalbumpagetitle");
            Image_Guide4.Source = new BitmapImage(new Uri(Utils.Constants.ResourceLoader.GetString("2howtouseeditalbumpagepicuri"), UriKind.RelativeOrAbsolute));
            TextBlock_HelpPage4Text.Text = Utils.Constants.ResourceLoader.GetString("2howtouseeditalbumpage");

            TextBlock_HelpPage7Title.Text = Utils.Constants.ResourceLoader.GetString("2howtousesmalltrickpagetitle");
            Image_Guide7.Source = new BitmapImage(new Uri(Utils.Constants.ResourceLoader.GetString("2howtousesmalltrickpagepicuri"), UriKind.RelativeOrAbsolute));
            TextBlock_HelpPage7Text.Text = Utils.Constants.ResourceLoader.GetString("2howtousesmalltrickpage");

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

        #region Property Changed
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String changedPropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(changedPropertyName));
            }
        }
        #endregion
    }
}
