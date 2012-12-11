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
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage.FileProperties;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.System;
using Surface_Maps.Utils;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Surface_Maps.Pages
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class VideoExposition : Common.LayoutAwarePage, INotifyPropertyChanged
    {
        private DataModel.AlbumDataStructure albumInfo = new DataModel.AlbumDataStructure();
        private FromMapToCollectioin navigateParameter = new FromMapToCollectioin();

        private ObservableCollection<Pages.VideoDataStructure> listVideo = new ObservableCollection<Pages.VideoDataStructure>();
        public ObservableCollection<Pages.VideoDataStructure> ListVideo
        {
            get
            {
                return listVideo;
            }
            set
            {
                listVideo = value;
                NotifyPropertyChanged("ListPhoto");
            }
        }

        public VideoExposition()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            DataContext = this;
            this.Loaded += VideoAlbumModifyView_Loaded;
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Assign a collection of bindable groups to this.DefaultViewModel["Groups"]
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        private async void VideoAlbumModifyView_Loaded(object sender, RoutedEventArgs e)
        {
            albumInfo = navigateParameter.slectedAlbum;
            if (albumInfo.AlbumName != "")
                pageTitle.Text = albumInfo.AlbumName;
            else pageTitle.Text = "New Album";
            if (navigateParameter.pushpin != null && navigateParameter.pushpin.BackgroundPhotoPath != "")
                fillBackgroundImage(navigateParameter.pushpin.BackgroundPhotoPath);
            await LoadData();
            itemGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Constants.StopLoadingAnimation(MainGrid);
        }

        private async void fillBackgroundImage(string path)
        {
            if (path != null && path != "")
            {
                try
                {
                    StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(path);
                    StorageItemThumbnail fileThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, (uint)Constants.ScreenHeight, ThumbnailOptions.UseCurrentScale);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(fileThumbnail);
                    Image_Background.Source = bitmapImage;
                }
                catch
                {
                    Utils.Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("cannotreadfilepossiblereason") + "\n\r" +
                                                      Constants.ResourceLoader.GetString("documentlibararycannotaccess") + "\n\r" +
                                                      Constants.ResourceLoader.GetString("pathfilechanged"));
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                itemGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                Constants.StartLoadingAnimation(MainGrid);
                if (e.Parameter != null)
                {
                    navigateParameter = e.Parameter as FromMapToCollectioin;
                }
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "VideoPlayPage - VolumeSlider_ValueChanged"); }
        }

        public async Task LoadData()
        {
            if (albumInfo.AlbumName != "")
            {
                ListVideo = new ObservableCollection<Pages.VideoDataStructure>();
                GC.Collect();
                var result = from datas in Utils.LifeMapManager.GetInstance().ListOfAllVideos
                             where datas.AlbumId == albumInfo.Id &&
                                   datas.Latitude == albumInfo.Latitude &&
                                   datas.Longitude == albumInfo.Longitude
                             select datas;
                bool ifexception = false;
                foreach (var row in result)
                {
                    if (ifexception == true) await loadAlbumVideosOneByOne(row);
                    ifexception = await loadAlbumVideosOneByOne(row);
                }
                pageTitle.Text = albumInfo.AlbumName;
                if (ifexception == true)
                    Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("pathfilechanged"));
            }
            else pageTitle.Text = "New Album";
            itemGridView.ItemsSource = ListVideo;
            itemListView.ItemsSource = ListVideo;
        }

        private async Task<bool> loadAlbumVideosOneByOne(DataModel.VideoDataStructure row)
        {
            try
            {
                StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(row.VideoPath);
                if (file != null)
                {
                    StorageItemThumbnail fileThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 900);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(fileThumbnail);
                    ListVideo.Add(new Pages.VideoDataStructure()
                    {
                        VideoData = row as DataModel.VideoDataStructure,
                        Image = bitmapImage,
                        VideoWidthHeight = Constants.HalfScreenHeight + 100,
                        VideoWidth = (Constants.HalfScreenHeight + 100) * (Convert.ToDouble(bitmapImage.PixelWidth) / Convert.ToDouble(bitmapImage.PixelHeight))
                    });
                }
                return false;
            }
            catch
            {
                ListVideo.Add(new Pages.VideoDataStructure()
                {
                    VideoData = row as DataModel.VideoDataStructure,
                    Image = null,
                    VideoWidthHeight = Constants.HalfScreenHeight + 100,
                    VideoWidth = Constants.HalfScreenHeight + 100
                });
                return true;
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

        private void GoBackClick(object sender, RoutedEventArgs e)
        {
            navigateParameter.slectedAlbum = albumInfo;
            this.Frame.Navigate(typeof(AlbumCollectionView), navigateParameter);
        }

        private async void Button_PlaySelectedVideo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync((itemGridView.SelectedItem as Pages.VideoDataStructure).VideoData.VideoPath);
                var targetStream = await file.OpenAsync(FileAccessMode.Read);
                await Launcher.LaunchFileAsync(file, new LauncherOptions { DisplayApplicationPicker = false });
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "VideoExposition - Button_PlaySelectedVideo_Click"); }
        }
    }
}
