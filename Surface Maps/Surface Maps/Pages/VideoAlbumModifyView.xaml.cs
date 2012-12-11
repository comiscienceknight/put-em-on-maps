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
using Windows.UI.Popups;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Media.Animation;
using Surface_Maps.Utils;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Surface_Maps.Pages
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class VideoAlbumModifyView : Common.LayoutAwarePage, INotifyPropertyChanged
    {
        private bool justSaved = true;

        private DataModel.AlbumDataStructure albumInfo = new DataModel.AlbumDataStructure();

        private FromMapToCollectioin navigateParameter = new FromMapToCollectioin();

        private ObservableCollection<VideoDataStructure> listVideo = new ObservableCollection<VideoDataStructure>();
        public ObservableCollection<VideoDataStructure> ListVideo
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

        public VideoAlbumModifyView()
        {
            this.InitializeComponent();
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
            if (navigateParameter.pushpin != null && navigateParameter.pushpin.BackgroundPhotoPath != "")
                fillBackgroundImage(navigateParameter.pushpin.BackgroundPhotoPath);
            albumInfo = navigateParameter.slectedAlbum;
            if (albumInfo.AlbumName != "")
                pageTitle.Text = albumInfo.AlbumName;
            else pageTitle.Text = "New Album";
            await LoadData();
            if (ListVideo.Count == 0)
                BottomAppBar.IsOpen = true;
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
                navigateParameter = e.Parameter as FromMapToCollectioin;
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "VideoAlbumModifyView - OnNavigatedTo"); }
        }

        public async Task LoadData()
        {
            if (albumInfo.AlbumName != "")
            {
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
        }

        private async Task<bool> loadAlbumVideosOneByOne(DataModel.VideoDataStructure row)
        {
            try
            {
                StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(row.VideoPath);
                if (file != null)
                {
                    StorageItemThumbnail fileThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 500);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(fileThumbnail);
                    insertRowToListVideo(row, bitmapImage);
                }
                return false;
            }
            catch
            {
                insertRowToListVideo(row as DataModel.VideoDataStructure, null);
                return true;
            }
        }

        private void insertRowToListVideo(DataModel.VideoDataStructure row, BitmapImage bitmapImage)
        {
            ListVideo.Add(new VideoDataStructure()
            {
                VideoData = row as DataModel.VideoDataStructure,
                Image = bitmapImage,
                VideoWidthHeight = Constants.HalfScreenHeight - 35,
                VideoWidth = (bitmapImage != null) ? (Constants.HalfScreenHeight - 35) * (Convert.ToDouble(bitmapImage.PixelWidth) / Convert.ToDouble(bitmapImage.PixelHeight)) :
                             Constants.HalfScreenHeight - 35
            });
        }

        private async void Button_AddNewVideo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IReadOnlyList<StorageFile> fileList = await selectPhotosFromFilePicker();
                if (fileList.Count <= 0) return;
                int count = 0;
                foreach (StorageFile file in fileList)
                {
                    int sfd = (from datas in listVideo where datas.VideoData.VideoPath == file.Path select datas).Count();
                    if (sfd == 0)
                    {
                        VideoDataStructure pds;
                        try
                        { 
                            pds= createNewDisplayableVideoObject(file);
                        }
                        catch
                        {
                            Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("dontsupportnonlocalfile"));
                            break;
                        }
                        await affectBitmapImageToNewDisplayablePhotoObject(file, pds);
                        ListVideo.Add(pds);
                    }
                    count++;
                }
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "VideoAlbumModifyView - Button_AddNewVideo_Click"); }
            justSaved = false;
        }

        private static async Task<IReadOnlyList<StorageFile>> selectPhotosFromFilePicker()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".wmv");
            IReadOnlyList<StorageFile> fileList = await openPicker.PickMultipleFilesAsync();
            return fileList;
        }

        private static async Task affectBitmapImageToNewDisplayablePhotoObject(StorageFile file, VideoDataStructure pds)
        {
            StorageItemThumbnail fileThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 900);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(fileThumbnail);
            pds.Image = bitmapImage;
        }

        private VideoDataStructure createNewDisplayableVideoObject(StorageFile file)
        {
            BitmapImage bi = new BitmapImage(new Uri(file.Path, UriKind.Absolute));
            VideoDataStructure pds = new VideoDataStructure()
            {
                VideoData = new DataModel.VideoDataStructure()
                {
                    AlbumId = albumInfo.Id,
                    Latitude = albumInfo.Latitude,
                    Longitude = albumInfo.Longitude,
                    VideoPath = file.Path,
                    ItemId = DateTime.Now.ToString() + file.Path
                },
                Image = bi,
                VideoWidthHeight = Constants.HalfScreenHeight - 35,
                VideoWidth = (Constants.HalfScreenHeight - 35) * (Convert.ToDouble(bi.PixelWidth) / Convert.ToDouble(bi.PixelHeight))
            };
            return pds;
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

        private async void Button_SaveAlbum_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Constants.StartLoadingAnimation(MainGrid);
                await updateAppAlbumList();
                await updateAppVideoList();
                string notificationStr = Constants.ResourceLoader.GetString("NewVidoesHaveBeenAdded") + " " + albumInfo.AlbumName + " " + Constants.ResourceLoader.GetString("NewPhotosHaveBeenAdded2");
                Helper.CreateToastNotifications(notificationStr);
                BottomAppBar.IsOpen = false;
                Constants.StopLoadingAnimation(MainGrid);
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "VideoAlbumModifyView - Button_SaveAlbum_Click"); }
            justSaved = true;
        }

        private async Task updateAppAlbumList()
        {
            if (Utils.LifeMapManager.GetInstance().ListOfAllAlbums == null) Utils.LifeMapManager.GetInstance().ListOfAllAlbums = new ObservableCollection<DataModel.AlbumDataStructure>();
            albumInfo.AlbumName = pageTitle.Text;
            if (albumInfo.Date.Year < 1000)
                albumInfo.Date = DateTime.Now;
            if (!Utils.LifeMapManager.GetInstance().ListOfAllAlbums.Contains(albumInfo))
            {
                albumInfo.Id = albumInfo.AlbumName + DateTime.Now.ToString();
                Utils.LifeMapManager.GetInstance().ListOfAllAlbums.Add(albumInfo);
            }
            await Utils.FilesSaver<DataModel.AlbumDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListOfAllAlbums, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Constants.NamingListAlbums);
        }

        private async Task updateAppVideoList()
        {
            int intialcount = ListVideo.Count;
            for (int i = 0; i < ListVideo.Count; i++)
            {
                ListVideo[i].VideoData.AlbumId = albumInfo.Id;
                if (!Utils.LifeMapManager.GetInstance().ListOfAllVideos.Contains(ListVideo[i].VideoData))
                {
                    try
                    {
                        StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(ListVideo[i].VideoData.VideoPath);
                        Utils.LifeMapManager.GetInstance().ListOfAllVideos.Add(ListVideo[i].VideoData);
                    }
                    catch
                    {
                        ListVideo.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (intialcount != ListVideo.Count)
                Utils.Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("cannotreadallvideos") + "\n\r" +
                                                  Constants.ResourceLoader.GetString("documentlibararycannotaccess"));
            await Utils.FilesSaver<DataModel.VideoDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListOfAllVideos, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + "ListVideos");
        }

        private async void GoBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (justSaved == false)
                {
                    MessageDialog dialog = new MessageDialog(Utils.Constants.ResourceLoader.GetString("leavewithoutsave"),
                                                             Utils.Constants.ResourceLoader.GetString("notification"));
                    dialog.Commands.Add(new UICommand(Utils.Constants.ResourceLoader.GetString("yesleavewithoutsave"), p =>
                    {
                        navigateParameter.slectedAlbum = albumInfo;
                        this.Frame.Navigate(typeof(AlbumCollectionView), navigateParameter);
                    }));
                    dialog.Commands.Add(new UICommand(Utils.Constants.ResourceLoader.GetString("noleaveforsave")));
                    await dialog.ShowAsync();
                }
                else
                {
                    navigateParameter.slectedAlbum = albumInfo;
                    this.Frame.Navigate(typeof(AlbumCollectionView), navigateParameter);
                }
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "VideoAlbumModifyView - GoBackClick"); }
        }

        private void Button_RemoveVideo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<VideoDataStructure> listItem = new List<VideoDataStructure>();
                for (int i = 0; i < itemGridView.SelectedItems.Count; i++)
                    listItem.Add(itemGridView.SelectedItems[i] as VideoDataStructure);
                if (ListVideo.Count - itemGridView.SelectedItems.Count == 0)
                    removeAllExistedVideoInAlbum(listItem);
                else
                    removeSelectedVideosInAlbum(listItem);
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "VideoAlbumModifyView - GoBackClick"); }
            justSaved = false;
        }

        private void removeSelectedVideosInAlbum(List<VideoDataStructure> listItem)
        {
            for (int i = 0; i < listItem.Count; i++)
            {
                ListVideo.Remove(listItem[i]);
                if (Utils.LifeMapManager.GetInstance().ListOfAllVideos.Contains(listItem[i].VideoData))
                {
                    Utils.LifeMapManager.GetInstance().ListOfAllVideos.Remove(listItem[i].VideoData);
                }
            }
        }

        private void removeAllExistedVideoInAlbum(List<VideoDataStructure> listItem)
        {
            for (int i = 0; i < listItem.Count - 1; i++)
            {
                ListVideo.Remove(listItem[i]);
                if (Utils.LifeMapManager.GetInstance().ListOfAllVideos.Contains(listItem[i].VideoData))
                {
                    Utils.LifeMapManager.GetInstance().ListOfAllVideos.Remove(listItem[i].VideoData);
                }
            }
            itemsViewSource.Source = null;
            ListVideo = new ObservableCollection<VideoDataStructure>();
            itemsViewSource.Source = ListVideo;
        }

        private void Button_ChangeAlbumName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                windowBounds = Window.Current.Bounds;
                createPopupWindowContainsFlyout("changealbumName");
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "VideoAlbumModifyView - Button_ChangeAlbumName_Click"); }
            justSaved = false;
        }

        private void Button_ValidPhotoComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                windowBounds = Window.Current.Bounds;
                createPopupWindowContainsFlyout("updatecomment");
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "VideoAlbumModifyView - Button_ValidPhotoComment_Click"); }
            justSaved = false;
        }


        #region change album name popup window
        private Popup settingsPopup;
        private double settingsWidth = 500;
        private Rect windowBounds;

        private void createPopupWindowContainsFlyout(string options)
        {
            settingsPopup = new Popup();
            settingsPopup.Closed -= OnPopupClosed;
            settingsPopup.Closed += OnPopupClosed;
            Window.Current.Activated -= OnWindowActivated;
            Window.Current.Activated += OnWindowActivated;
            settingsPopup.IsLightDismissEnabled = true;
            settingsPopup.Width = settingsWidth;
            settingsPopup.Height = windowBounds.Height;
            addProperAnimationToPopup(); // Add the proper animation for the panel.
            createASettingsFlyoutTheSameDimenssionsAsThePopup(options);
            definePopupLocationAndPopUpIt(); // Let's define the location of our Popup.
        }

        private void createASettingsFlyoutTheSameDimenssionsAsThePopup(string options)
        {
            if (options == "changealbumName")
            {
                createChangealbumNamePopup();
            }
            else if (options == "updatecomment")
            {
                createUpdateCommentPopup();
            }
        }

        private void createUpdateCommentPopup()
        {
            List<VideoDataStructure> listmp = new List<VideoDataStructure>();
            if (itemGridView.SelectedItems.Count > 0)
            {
                for (int i = itemGridView.SelectedItems.Count - 1; i >= 0; i--)
                {
                    listmp.Add((VideoDataStructure)itemGridView.SelectedItems[i]);
                }
            }
            AlbumMediasCommentModifyFlyout mypane = new AlbumMediasCommentModifyFlyout(listmp);
            settingsWidth = 550;
            mypane.Width = settingsWidth;
            mypane.Height = windowBounds.Height;
            settingsPopup.Child = mypane;
        }

        private void createChangealbumNamePopup()
        {
            AlbumNameDateModifyFlyout mypane = new AlbumNameDateModifyFlyout(pageTitle, albumInfo);
            settingsWidth = 550;
            mypane.Width = settingsWidth;
            mypane.Height = windowBounds.Height;
            settingsPopup.Child = mypane;
        }

        private void addProperAnimationToPopup()
        {
            settingsPopup.ChildTransitions = new TransitionCollection();
            settingsPopup.ChildTransitions.Add(new PaneThemeTransition()
            {
                Edge = (SettingsPane.Edge == SettingsEdgeLocation.Right) ?
                       EdgeTransitionLocation.Right :
                       EdgeTransitionLocation.Left
            });
        }

        private void definePopupLocationAndPopUpIt()
        {
            settingsPopup.SetValue(Canvas.LeftProperty, SettingsPane.Edge == SettingsEdgeLocation.Right ? (windowBounds.Width - settingsWidth) : 0);
            settingsPopup.SetValue(Canvas.TopProperty, 0);
            settingsPopup.IsOpen = true;
        }

        private void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            try
            {
                if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
                {
                    settingsPopup.IsOpen = false;
                }
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "VideoAlbumModifyView - OnWindowActivated"); }
        }

        void OnPopupClosed(object sender, object e)
        {
            try
            {
                Window.Current.Activated -= OnWindowActivated;
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "VideoAlbumModifyView - OnPopupClosed"); }
        }
        #endregion
    }

    public class VideoDataStructure : INotifyPropertyChanged
    {
        private DataModel.VideoDataStructure videoData = new DataModel.VideoDataStructure();
        public DataModel.VideoDataStructure VideoData
        {
            get
            {
                return videoData;
            }
            set
            {
                videoData = value;
                NotifyPropertyChanged("VideoData");
            }
        }

        private double videoWidthHeight;
        public double VideoWidthHeight
        {
            get
            {
                return videoWidthHeight;
            }
            set
            {
                videoWidthHeight = value;
                NotifyPropertyChanged("VideoWidthHeight");
            }
        }

        private double videoWidth;
        public double VideoWidth
        {
            get
            {
                return videoWidth;
            }
            set
            {
                videoWidth = value;
                NotifyPropertyChanged("VideoWidth");
            }
        }

        public BitmapImage image = new BitmapImage();
        public BitmapImage Image
        {
            get
            {
                return image;
            }
            set
            {
                image = value;
                NotifyPropertyChanged("Image");
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