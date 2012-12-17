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
    public sealed partial class AlbumMediasCommentModifyFlyout : Common.LayoutAwarePage
    {
        const int ContentAnimationOffset = 100;

        List<PhotoDataStructure> listSelectedPhotos = new List<PhotoDataStructure>();
        public AlbumMediasCommentModifyFlyout(List<PhotoDataStructure> listSelectedPhotos)
        {
            this.InitializeComponent();

            FlyoutContent.Transitions = new TransitionCollection();
            FlyoutContent.Transitions.Add(new EntranceThemeTransition()
            {
                FromHorizontalOffset = (SettingsPane.Edge == SettingsEdgeLocation.Right) ? ContentAnimationOffset : (ContentAnimationOffset * -1)
            });

            TextBox_NeedChangedMediasComment.Text = "";
            this.listSelectedPhotos = listSelectedPhotos;
        }

        List<VideoDataStructure> listSelectedVideos = new List<VideoDataStructure>();
        public AlbumMediasCommentModifyFlyout(List<VideoDataStructure> listSelectedVideos)
        {
            this.InitializeComponent();

            FlyoutContent.Transitions = new TransitionCollection();
            FlyoutContent.Transitions.Add(new EntranceThemeTransition()
            {
                FromHorizontalOffset = (SettingsPane.Edge == SettingsEdgeLocation.Right) ? ContentAnimationOffset : (ContentAnimationOffset * -1)
            });

            TextBox_NeedChangedMediasComment.Text = "";
            this.listSelectedVideos = listSelectedVideos;
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

        private void Button_ChangeSelectedPhotosComment_Click(object sender, RoutedEventArgs e)
        {
            if (listSelectedPhotos.Count > 0)
            {
                for (int i = listSelectedPhotos.Count - 1; i >= 0; i--)
                {
                    listSelectedPhotos[i].PhotoData.Comment = TextBox_NeedChangedMediasComment.Text;
                }
            }
            if (listSelectedVideos.Count > 0)
            {
                for (int i = listSelectedVideos.Count - 1; i >= 0; i--)
                {
                    listSelectedVideos[i].VideoData.Comment = TextBox_NeedChangedMediasComment.Text;
                }
            }
            hideThisPopup();
        }

        private void hideThisPopup()
        {
            Popup parent = this.Parent as Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }
        }
    }
}
