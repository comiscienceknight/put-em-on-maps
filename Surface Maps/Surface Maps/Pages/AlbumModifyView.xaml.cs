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
using Surface_Maps.DataModel;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Surface_Maps.Pages
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class AlbumModifyView : Common.LayoutAwarePage, INotifyPropertyChanged
    {
        private bool justSaved = true;

        private ObservableCollection<PhotoDataStructure> listPhoto = new ObservableCollection<PhotoDataStructure>();
        public ObservableCollection<PhotoDataStructure> ListPhoto
        {
            get
            {
                return listPhoto;
            }
            set
            {
                listPhoto = value;
                NotifyPropertyChanged("ListPhoto");
            }
        }

        AlbumDataStructure albumInfo = new AlbumDataStructure();
        //View.AlbumCollectionView.ParameterForNivagateToAlbumCollectionView navigateParameter = new AlbumCollectionView.ParameterForNivagateToAlbumCollectionView();

        FromMapToCollectioin navigateParameter = new FromMapToCollectioin();

        public AlbumModifyView()
        {
            this.InitializeComponent();
            DataContext = this;
            this.Loaded += AlbumModifyView_Loaded;
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Assign a collection of bindable groups to this.DefaultViewModel["Groups"]
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        async void AlbumModifyView_Loaded(object sender, RoutedEventArgs e)
        {
            if (navigateParameter.pushpin != null && navigateParameter.pushpin.BackgroundPhotoPath != "")
                fillBackgroundImage(navigateParameter.pushpin.BackgroundPhotoPath);
            albumInfo = navigateParameter.slectedAlbum;
            if (albumInfo.AlbumName != "")
                pageTitle.Text = albumInfo.AlbumName;
            else pageTitle.Text = "New Album";
            await LoadData();
            if (ListPhoto.Count == 0)
                BottomAppBar.IsOpen = true;
            itemGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Constants.StopLoadingAnimation(MainGrid);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                itemGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                Constants.StartLoadingAnimation(MainGrid);
                navigateParameter = e.Parameter as FromMapToCollectioin;
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "AlbumModifyView - OnNavigatedTo"); }
        }

        public async Task LoadData()
        {
            if (albumInfo.AlbumName != "")
            {
                var result = from datas in Utils.LifeMapManager.GetInstance().ListOfAllPhotos
                             where datas.AlbumId == albumInfo.Id &&
                                   datas.Latitude == albumInfo.Latitude &&
                                   datas.Longitude == albumInfo.Longitude
                             select datas;
                bool ifexception = false;
                foreach (var row in result)
                {
                    if (ifexception == true) await loadAlbumPhotosOneByOne(row);
                    else ifexception = await loadAlbumPhotosOneByOne(row);
                }
                pageTitle.Text = albumInfo.AlbumName;
                if (ifexception == true)
                    Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("pathfilechanged"));
            }
            else pageTitle.Text = "New Album";
        }

        private async Task<bool> loadAlbumPhotosOneByOne(DataModel.PhotoDataStructure row)
        {
            try
            {
                StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(row.ImagePath);
                if (file != null)
                {
                    StorageItemThumbnail fileThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 500);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(fileThumbnail);
                    insertRowToListPhoto(row as DataModel.PhotoDataStructure, bitmapImage);
                }
                return false;
            }
            catch
            {
                insertRowToListPhoto(row as DataModel.PhotoDataStructure, null);
                return true;
            }
        }

        private void insertRowToListPhoto(DataModel.PhotoDataStructure row, BitmapImage bitmapImage)
        {
            ListPhoto.Add(new PhotoDataStructure()
            {
                PhotoData = row,
                Image = bitmapImage,
                ImageWidthHeight = Constants.HalfScreenHeight - 30
            });
        }

        private async void Button_SaveAlbum_Click_1(object sender, RoutedEventArgs e)
        {
            Constants.StartLoadingAnimation(MainGrid);
            try
            {
                await updateAppAlbumList();
                await updateAppPhotoList();
                string notificationStr = Constants.ResourceLoader.GetString("NewPhotosHaveBeenAdded") + " " + albumInfo.AlbumName + " " + Constants.ResourceLoader.GetString("NewPhotosHaveBeenAdded2");
                Helper.CreateToastNotifications(notificationStr);
                BottomAppBar.IsOpen = false;
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "AlbumModifyView - Button_SaveAlbum_Click_1"); }
            Constants.StopLoadingAnimation(MainGrid);
            justSaved = true;
        }

        private async Task updateAppPhotoList()
        {
            int intialcount = ListPhoto.Count;
            for (int i = 0; i < ListPhoto.Count; i++)
            {
                ListPhoto[i].PhotoData.AlbumId = albumInfo.Id;
                if (!Utils.LifeMapManager.GetInstance().ListOfAllPhotos.Contains(ListPhoto[i].PhotoData))
                {
                    try
                    {
                        await updateSingleLocalTileNotificationImage(ListPhoto[i].PhotoData, (i + 1) % 5);
                        Utils.LifeMapManager.GetInstance().ListOfAllPhotos.Add(ListPhoto[i].PhotoData);
                    }
                    catch
                    {
                        ListPhoto.RemoveAt(i);
                        i--;
                    }
                }
            }

            if (intialcount != ListPhoto.Count)
                Utils.Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("cannotreadallphotos") + "\n\r" +
                                                  Constants.ResourceLoader.GetString("documentlibararycannotaccess"));
            await Utils.FilesSaver<DataModel.PhotoDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListOfAllPhotos, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Constants.NamingListPhotos);
            await Helper.CreateLocalNotifications();
        }




        private static async Task updateSingleLocalTileNotificationImage(DataModel.PhotoDataStructure photods, int i)
        {
            StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(photods.ImagePath);
            StorageItemThumbnail fileThumbnail2 = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 900);
            var buffer = new Windows.Storage.Streams.Buffer(Convert.ToUInt32(fileThumbnail2.Size));
            await fileThumbnail2.ReadAsync(buffer, Convert.ToUInt32(fileThumbnail2.Size), InputStreamOptions.None);
            var thumbFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(i.ToString() + ".png", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBufferAsync(thumbFile, buffer);
        }

        private async Task updateAppAlbumList()
        {
            if (Utils.LifeMapManager.GetInstance().ListOfAllAlbums == null) Utils.LifeMapManager.GetInstance().ListOfAllAlbums = new ObservableCollection<AlbumDataStructure>();
            albumInfo.AlbumName = pageTitle.Text;
            if (albumInfo.Date.Year < 1000)
                albumInfo.Date = DateTime.Now;
            if (!Utils.LifeMapManager.GetInstance().ListOfAllAlbums.Contains(albumInfo))
            {
                albumInfo.Id = pageTitle.Text + DateTime.Now.ToString();
                Utils.LifeMapManager.GetInstance().ListOfAllAlbums.Add(albumInfo);
            }

            await Utils.FilesSaver<AlbumDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListOfAllAlbums, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Constants.NamingListAlbums);
        }

        #region add photo
        private async void Button_AddNewPhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                IReadOnlyList<StorageFile> fileList = await selectPhotosFromFilePicker();
				Constants.StartLoadingAnimation(MainGrid);
                if (fileList.Count <= 0) return;
                foreach (StorageFile file in fileList)
                {
                    if((from datas in listPhoto where datas.PhotoData.ImagePath == file.Path select datas).Count() == 0)
					{
                        PhotoDataStructure pds;
						if(file.Path == "") //一般来讲，如果file.Path == "" 说明他是非本地的数据，那就要保存到本地再说
						{
							pds = await saveNonLocalFileToLocalAndAddPhotoToList(file);
							if(pds != null)
							{
								await affectBitmapImageToNewDisplayablePhotoObject(file, pds);
								ListPhoto.Add(pds);
							}
						}
						else //如果是本地的，就从本地读取，如果是非法范围内的文件，提示用户解决方案
						{
							try
							{
								//看看skydrive下和facebook下的路径是否可以访问
								pds = createNewDisplayablePhotoObject(file);
								await affectBitmapImageToNewDisplayablePhotoObject(file, pds);
								ListPhoto.Add(pds);
							}
							catch
							{
								Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("2CannotReadSeveralFile") + " " +
															file.Path + "\n\r" +
															Constants.ResourceLoader.GetString("2CannotReadLocalFileReasonAndResolution"));
							}
						}
                    }
                }
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "AlbumModifyView - Button_AddNewPhoto_Click"); }
            justSaved = false;
			Constants.StopLoadingAnimation(MainGrid);
        }
		
		private async Task<PhotoDataStructure> saveNonLocalFileToLocalAndAddPhotoToList(StorageFile file)
		{
			PhotoDataStructure pds = null;
			try
			{
				MessageDialog dialog = new MessageDialog(Utils.Constants.ResourceLoader.GetString("2MustSaveNonLocalFileToLocal_ButParticuliarlyForSeveralCase"),
												 Utils.Constants.ResourceLoader.GetString("notification"));
				dialog.Commands.Add(new UICommand(Utils.Constants.ResourceLoader.GetString("2Ok_SaveNonLocalFileToLocal"), p =>
				{	
					//http://msdn.microsoft.com/en-us/library/windows/apps/hh465174.aspx
					//Integrating with file picker contracts
					//http://msdn.microsoft.com/en-us/library/windows/apps/hh465255.aspx
					//Quickstart: Receiving shared content 
					//http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh973051.aspx
					//How to receive an image
					
					//http://msdn.microsoft.com/en-us/library/windows/apps/xaml/JJ150592(v=win.10).aspx
					//How to save files through file pickers
					
					//
				}));
				dialog.Commands.Add(new UICommand(Utils.Constants.ResourceLoader.GetString("2No_IgnoreThisFile")));
				await dialog.ShowAsync();
			}
			catch (Exception exp)
			{
				Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("2FileLocalizationFailed") + "\n\r" + exp.Message);
			}
			return pds;
		}
		
        private static async Task<IReadOnlyList<StorageFile>> selectPhotosFromFilePicker()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpg");
            IReadOnlyList<StorageFile> fileList = await openPicker.PickMultipleFilesAsync();
            return fileList;
        }

        private static async Task affectBitmapImageToNewDisplayablePhotoObject(StorageFile file, PhotoDataStructure pds)
        {
            StorageItemThumbnail fileThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 900);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(fileThumbnail);
            pds.Image = bitmapImage;
        }

        private PhotoDataStructure createNewDisplayablePhotoObject(StorageFile file)
        {
            PhotoDataStructure pds = new PhotoDataStructure()
            {
                PhotoData = new DataModel.PhotoDataStructure()
                {
                    AlbumId = albumInfo.Id,
                    Latitude = albumInfo.Latitude,
                    Longitude = albumInfo.Longitude,
                    ImagePath = file.Path,
                    ItemId = DateTime.Now.ToString() + file.Path
                },
                Image = new BitmapImage(new Uri(file.Path, UriKind.Absolute)),
                ImageWidthHeight = Constants.HalfScreenHeight - 30
            };
            return pds;
        }
        #endregion

        private void Button_RemovePhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<PhotoDataStructure> listItem = new List<PhotoDataStructure>();
                for (int i = 0; i < itemGridView.SelectedItems.Count; i++)
                {
                    listItem.Add(itemGridView.SelectedItems[i] as PhotoDataStructure);
                }
                if (ListPhoto.Count - itemGridView.SelectedItems.Count == 0)
                    removeAllPhotoInAlbum(listItem);
                else
                    removeSelectedPhotoInALbum(listItem);
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "AlbumModifyView - Button_RemovePhoto_Click"); }
            justSaved = false;
        }

        private void removeSelectedPhotoInALbum(List<PhotoDataStructure> listItem)
        {
            for (int i = 0; i < listItem.Count; i++)
            {
                ListPhoto.Remove(listItem[i]);
                if (Utils.LifeMapManager.GetInstance().ListOfAllPhotos.Contains(listItem[i].PhotoData))
                {
                    Utils.LifeMapManager.GetInstance().ListOfAllPhotos.Remove(listItem[i].PhotoData);
                }
            }
        }

        private void removeAllPhotoInAlbum(List<PhotoDataStructure> listItem)
        {
            for (int i = 0; i < listItem.Count - 1; i++)
            {
                ListPhoto.Remove(listItem[i]);
                if (Utils.LifeMapManager.GetInstance().ListOfAllPhotos.Contains(listItem[i].PhotoData))
                {
                    Utils.LifeMapManager.GetInstance().ListOfAllPhotos.Remove(listItem[i].PhotoData);
                }
            }
            itemsViewSource.Source = null;
            ListPhoto = new ObservableCollection<PhotoDataStructure>();
            itemsViewSource.Source = ListPhoto;
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
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "AlbumModifyView - GoBackClick"); }
        }


        #region change album name popup window
        private Popup settingsPopup;
        private double settingsWidth = 500;
        private Rect windowBounds;

        private void Button_ChangeAlbumName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                windowBounds = Window.Current.Bounds;
                createPopupWindowContainsFlyout("changealbum");
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "AlbumModifyView - Button_ChangeAlbumName_Click"); }
            justSaved = false;
        }

        private void Button_ValidPhotoComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                windowBounds = Window.Current.Bounds;
                createPopupWindowContainsFlyout("updatecomment");
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "AlbumModifyView - Button_ValidPhotoComment_Click"); }
            justSaved = false;
        }

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
            // Add the proper animation for the panel.
            settingsPopup.ChildTransitions = new TransitionCollection();
            settingsPopup.ChildTransitions.Add(new PaneThemeTransition()
            {
                Edge = (SettingsPane.Edge == SettingsEdgeLocation.Right) ?
                       EdgeTransitionLocation.Right :
                       EdgeTransitionLocation.Left
            });

            // Create a SettingsFlyout the same dimenssions as the Popup.
            if (options == "changealbum")
            {
                AlbumNameDateModifyFlyout mypane = new AlbumNameDateModifyFlyout(pageTitle, albumInfo);
                settingsWidth = 650;
                mypane.Width = settingsWidth;
                mypane.Height = windowBounds.Height;
                // Place the SettingsFlyout inside our Popup window.
                settingsPopup.Child = mypane;
            }
            else if (options == "updatecomment")
            {
                List<PhotoDataStructure> listmp = new List<PhotoDataStructure>();
                if (itemGridView.SelectedItems.Count > 0)
                {
                    for (int i = itemGridView.SelectedItems.Count - 1; i >= 0; i--)
                    {
                        listmp.Add((PhotoDataStructure)itemGridView.SelectedItems[i]);
                    }
                }
                AlbumMediasCommentModifyFlyout mypane = new AlbumMediasCommentModifyFlyout(listmp);
                settingsWidth = 550;
                mypane.Width = settingsWidth;
                mypane.Height = windowBounds.Height;
                // Place the SettingsFlyout inside our Popup window.
                settingsPopup.Child = mypane;
            }
            // Let's define the location of our Popup.
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
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "AlbumModifyView - OnWindowActivated"); }
        }

        void OnPopupClosed(object sender, object e)
        {
            try
            {
                Window.Current.Activated -= OnWindowActivated;
                //createPopupWindowContainsFlyout("updatecomment");
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "AlbumModifyView - OnPopupClosed"); }
        }
        #endregion
    }

    public class PhotoDataStructure : INotifyPropertyChanged
    {
        private DataModel.PhotoDataStructure photoData = new DataModel.PhotoDataStructure();
        public DataModel.PhotoDataStructure PhotoData
        {
            get
            {
                return photoData;
            }
            set
            {
                photoData = value;
                NotifyPropertyChanged("PhotoData");
            }
        }

        private double imageWidthHeight;
        public double ImageWidthHeight
        {
            get
            {
                return imageWidthHeight;
            }
            set
            {
                imageWidthHeight = value;
                NotifyPropertyChanged("ImageWidthHeight");
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

        public string imageWidth;
        public string ImageWidth
        {
            get
            {
                return Image.PixelWidth.ToString();
            }
        }

        public string imageHeight;
        public string ImageHeight
        {
            get
            {
                return Convert.ToInt32(Constants.ScreenHeight - 300).ToString();
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