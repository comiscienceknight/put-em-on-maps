using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
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
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AlbumCollectionView : Common.LayoutAwarePage, INotifyPropertyChanged
    {
        #region filed
        private bool justSaved = true;

        private ObservableCollection<GroupInfoList<DataModel.AlbumDataStructure>> allCollectionByDateGroups = new ObservableCollection<GroupInfoList<DataModel.AlbumDataStructure>>();
        public ObservableCollection<GroupInfoList<DataModel.AlbumDataStructure>> AllCollectionByDateGroups
        {
            get { return allCollectionByDateGroups; }
            set
            {
                allCollectionByDateGroups = value;
                NotifyPropertyChanged("AllCollectionByDateGroups");
            }
        }

        private DataModel.PushpinDataStructure pushpinData = new DataModel.PushpinDataStructure();

        private DispatcherTimer dispatcherTimer;
        private int albumChangePhotoAnimationTurnNumber = 0;

        private FromMapToCollectioin _parameter = null;
        #endregion

        public AlbumCollectionView()
        {
            this.InitializeComponent();
            DataContext = this;
            Loaded += AlbumCollectionView_Loaded;
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Assign a collection of bindable groups to this.DefaultViewModel["Groups"]
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        private async void AlbumCollectionView_Loaded(object sender, RoutedEventArgs e)
        {
            FromMapToCollectioin data = _parameter;
            pushpinData = data.pushpin;
            if (pushpinData != null)
            {
                fillBackgroundImage(this.pushpinData.BackgroundPhotoPath);
                pageTitle.Text = pushpinData.AlbumCollectionName;
                await LoadData();
                if (AllCollectionByDateGroups.Count == 0)
                    BottomAppBar.IsOpen = true;
            }
            if (data.slectedAlbum != null && AllCollectionByDateGroups.Count > 0) focusToSelectedAlbum(data);
            Constants.StopLoadingAnimation(MainGrid);
            itemGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            initializeDispatchertTimer();
        }

        private void initializeDispatchertTimer()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 2);
            dispatcherTimer.Start();
        }

        async void dispatcherTimer_Tick(object sender, object e)
        {
            try
            {
                int count = 0;
                int albumQuantity = getAlbumQuantity();
                for (int i = 0; i < AllCollectionByDateGroups.Count; i++)
                    for (int j = 0; j < AllCollectionByDateGroups[i].Count; j++)
                    {
                        if ((await changeAlbumProfilePhoto(count, AllCollectionByDateGroups[i].ElementAt(j) as DataModel.AlbumDataStructure)) == true)
                        {
                            calculateAlbumChangePhotoAnimationTurnNumber(albumQuantity);
                            return;
                        }
                        count++;
                    }
                calculateAlbumChangePhotoAnimationTurnNumber(albumQuantity);
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - dispatcherTimer_Tick"); }
        }

        private void calculateAlbumChangePhotoAnimationTurnNumber(int albumQuantity)
        {
            if (albumChangePhotoAnimationTurnNumber == albumQuantity)
                albumChangePhotoAnimationTurnNumber = 0;
            else
                albumChangePhotoAnimationTurnNumber++;
        }

        private async Task<bool> changeAlbumProfilePhoto(int count, DataModel.AlbumDataStructure item)
        {
            Random rnd = new Random();
            if (count == albumChangePhotoAnimationTurnNumber)
            {
                if (item.AlbumType == "Video")
                {
                    return await changeVideoAlbumProfilePhoto(item, rnd);
                }
                else if (item.AlbumType == "Photo")
                {
                    return await changePhotoAlbumProfilePhoto(item, rnd);
                }
            }
            return false;
        }

        private static async Task<bool> changePhotoAlbumProfilePhoto(DataModel.AlbumDataStructure item, Random rnd)
        {
            try
            {
                var result2 = from datas in Utils.LifeMapManager.GetInstance().ListOfAllPhotos
                              where datas.AlbumId == item.Id && datas.Latitude == item.Latitude && datas.Longitude == item.Longitude
                              select datas;
                for (int i = 0; i < result2.Count(); i++)
                {
                    if (i == rnd.Next(result2.Count()))
                    {
                        StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(((DataModel.PhotoDataStructure)result2.ElementAt(i)).ImagePath);
                        if (file != null) return await setAlbumProfileImage(item, file);
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> changeVideoAlbumProfilePhoto(DataModel.AlbumDataStructure item, Random rnd)
        {
            try
            {
                var result2 = from datas in Utils.LifeMapManager.GetInstance().ListOfAllVideos
                              where datas.AlbumId == item.Id && datas.Latitude == item.Latitude && datas.Longitude == item.Longitude
                              select datas;
                for (int i = 0; i < result2.Count(); i++)
                {
                    if (i == rnd.Next(result2.Count()))
                    {
                        StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(((DataModel.VideoDataStructure)result2.ElementAt(i)).VideoPath);
                        if (file != null) return await setAlbumProfileImage(item, file);
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> setAlbumProfileImage(DataModel.AlbumDataStructure item, StorageFile file)
        {
            try
            {
                StorageItemThumbnail fileThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 400);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileThumbnail);
                item.ImagePath = bitmapImage;
                return true;
            }
            catch
            {
                return false;
            }
        }

        int getAlbumQuantity()
        {
            int count = 0;
            for (int i = 0; i < AllCollectionByDateGroups.Count; i++)
            {
                for (int j = 0; j < AllCollectionByDateGroups[i].Count; j++)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                itemGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                Constants.StartLoadingAnimation(MainGrid);
                if (e.Parameter != null && e.Parameter is FromMapToCollectioin)
                {
                    _parameter = e.Parameter as FromMapToCollectioin;
                }
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - OnNavigatedTo"); }
        }

        private void focusToSelectedAlbum(FromMapToCollectioin data)
        {
            for (int i = 0; i < AllCollectionByDateGroups.Count; i++)
            {
                foreach (var row in (GroupInfoList<DataModel.AlbumDataStructure>)AllCollectionByDateGroups[i])
                {
                    if (((DataModel.AlbumDataStructure)row).Id == data.slectedAlbum.Id)
                    {
                        itemGridView.SelectedItem = (DataModel.AlbumDataStructure)row;
                        itemGridView.ScrollIntoView(itemGridView.SelectedItem, ScrollIntoViewAlignment.Leading);
                        break;
                    }
                }
            }
        }

        public async Task LoadData()
        {
			try
			{
				Data.LifeMapMgr.ListOfAllAlbums = await Helper.GetContent<ObservableCollection<DataModel.AlbumDataStructure>>(Data.LifeMapMgr.SelectedLifeMap.Id + Constants.NamingListAlbums);

				if (Utils.LifeMapManager.GetInstance().ListOfAllAlbums != null)
				{
					AllCollectionByDateGroups = new ObservableCollection<GroupInfoList<DataModel.AlbumDataStructure>>();
					var result = Utils.LifeMapManager.GetInstance().ListOfAllAlbums.Where(p => p.Latitude == pushpinData.Latitude && p.Longitude == pushpinData.Longitude);
					// 然后根据日期排序
					foreach (var row in result)
					{
						if (row.AlbumType == "Photo") await loadOnePhotoAlbum(row);
						else if (row.AlbumType == "Video") await loadOneVideoAlbum(row);
						AddAlbumToDataSourceCollection(row);
					}
				}
			}
			catch{}
        }

        private static async Task loadOneVideoAlbum(DataModel.AlbumDataStructure row)
        {
            var result2 = from datas in Utils.LifeMapManager.GetInstance().ListOfAllVideos
                          where datas.AlbumId == row.Id && datas.Latitude == row.Latitude && datas.Longitude == row.Longitude
                          select datas;
            foreach (var row2 in result2)
            {
                StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(row2.VideoPath);
                if (file != null)
                {
                    await setAlbumProfileImage(row, file);
                    break;
                }
            }
        }

        private static async Task loadOnePhotoAlbum(DataModel.AlbumDataStructure row)
        {
            var result2 = from datas in Utils.LifeMapManager.GetInstance().ListOfAllPhotos
                          where datas.AlbumId == row.Id && datas.Latitude == row.Latitude && datas.Longitude == row.Longitude
                          select datas;
            foreach (var row2 in result2)
            {
                StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(row2.ImagePath);
                if (file != null)
                {
                    await setAlbumProfileImage(row, file);
                    break;
                }
            }
        }

        private void AddAlbumToDataSourceCollection(DataModel.AlbumDataStructure row)
        {
            bool hasAlreadyExisted = false;
            string month = (row.Date.Month < 10) ? "0" + row.Date.Month.ToString() : row.Date.Month.ToString();
            string day = (row.Date.Day < 10) ? "0" + row.Date.Day.ToString() : row.Date.Day.ToString();
            for (int i = 0; i < AllCollectionByDateGroups.Count; i++)
            {
                if (AllCollectionByDateGroups[i].Count > 0)
                    if (AllCollectionByDateGroups[i].Key == row.Date.Year.ToString() + "-" + month + "-" + day)
                    {
                        AllCollectionByDateGroups[i].Add(row);
                        hasAlreadyExisted = true;
                    }
            }
            if (hasAlreadyExisted == false)
            {
                GroupInfoList<DataModel.AlbumDataStructure> newItem = new GroupInfoList<DataModel.AlbumDataStructure>();
                newItem.Add(row);
                newItem.Key = row.Date.Year.ToString() + "-" + month + "-" + day;
                AllCollectionByDateGroups.Add(newItem);
            }
        }

        private void Button_PlayAlbum_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FromMapToCollectioin parameter = new FromMapToCollectioin();
                parameter.slectedAlbum = itemGridView.SelectedItem as DataModel.AlbumDataStructure;
                parameter.pushpin = pushpinData;
                if (parameter.slectedAlbum != null)
                {
                    dispatcherTimer.Stop();
                    if (parameter.slectedAlbum.AlbumType == "Photo")
                        this.Frame.Navigate(typeof(PhotoExposition), parameter);
                    else if (parameter.slectedAlbum.AlbumType == "Video")
                        this.Frame.Navigate(typeof(VideoExposition), parameter);
                }
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "AlbumCollectionView - Button_PlayAlbum_Click"); }
        }

        private void Button_AddNewAlbum_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataModel.AlbumDataStructure newAlbum = new DataModel.AlbumDataStructure()
                {
                    AlbumName = "",
                    Longitude = pushpinData.Longitude,
                    Latitude = pushpinData.Latitude,
                    PushpinId = DateTime.Now.ToString() + pushpinData.Longitude
                };
                FromMapToCollectioin parameter = new FromMapToCollectioin();
                parameter.slectedAlbum = newAlbum;
                parameter.pushpin = pushpinData;
                dispatcherTimer.Stop();
                this.Frame.Navigate(typeof(AlbumModifyView), parameter);
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - Button_AddNewAlbum_Click"); }
        }

        private void Button_ModifyAlbum_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (itemGridView.SelectedItem != null && Utils.LifeMapManager.GetInstance().ListOfAllAlbums.Contains(itemGridView.SelectedItem as DataModel.AlbumDataStructure))
                {
                    FromMapToCollectioin parameter = new FromMapToCollectioin();
                    parameter.slectedAlbum = itemGridView.SelectedItem as DataModel.AlbumDataStructure;
                    parameter.pushpin = pushpinData;
                    dispatcherTimer.Stop();
                    if (parameter.slectedAlbum.AlbumType == "Photo")
                        this.Frame.Navigate(typeof(AlbumModifyView), parameter);
                    else if (parameter.slectedAlbum.AlbumType == "Video")
                        this.Frame.Navigate(typeof(VideoAlbumModifyView), parameter);
                }
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - Button_ModifyAlbum_Click"); }
        }

        private async void Button_RemoveAlbum_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageDialog dialog = new MessageDialog(Utils.Constants.ResourceLoader.GetString("ConfirmationForDeleteAlbum"), Utils.Constants.ResourceLoader.GetString("ConfirmationForDeletePushpinNotification"));
                dialog.Commands.Add(new UICommand(Utils.Constants.ResourceLoader.GetString("ConfirmationForDeletePushpinYes"), async p =>
                {
                    if (itemGridView.SelectedItem != null && Utils.LifeMapManager.GetInstance().ListOfAllAlbums.Contains(itemGridView.SelectedItem as DataModel.AlbumDataStructure))
                    {

                        (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = null;

                        DataModel.AlbumDataStructure removeItem = itemGridView.SelectedItem as DataModel.AlbumDataStructure;
                        await removeAlbumPhotos(removeItem);
                        await removeAlbum(removeItem);
                        Helper.CreateToastNotifications(Utils.Constants.ResourceLoader.GetString("AlbumHasBeenRemoved"));
                    }
                }));
                dialog.Commands.Add(new UICommand(Utils.Constants.ResourceLoader.GetString("ConfirmationForDeletePushpinNo")));
                await dialog.ShowAsync();
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - Button_ModifyAlbum_Click"); }
        }

        private async Task removeAlbum(DataModel.AlbumDataStructure removeItem)
        {
            for (int i = 0; i < AllCollectionByDateGroups.Count; i++)
            {
                if (AllCollectionByDateGroups[i].Contains(removeItem))
                {
                    AllCollectionByDateGroups[i].Remove(removeItem);
                    if (AllCollectionByDateGroups[i].Count == 0)
                        allCollectionByDateGroups.RemoveAt(i);
                    break;
                }

            }
            Utils.LifeMapManager.GetInstance().ListOfAllAlbums.Remove(removeItem);
            await Utils.FilesSaver<DataModel.AlbumDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListOfAllAlbums, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Constants.NamingListAlbums);
        }

        public static async Task removeAlbumPhotos(DataModel.AlbumDataStructure removeItem)
        {
            var results = from datas in Utils.LifeMapManager.GetInstance().ListOfAllPhotos
                          where datas.AlbumId == removeItem.Id
                          select datas;
            if (results != null)
            {
                removeAlbumPhotosExecution(results);
            }
            await Utils.FilesSaver<DataModel.PhotoDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListOfAllPhotos, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Constants.NamingListPhotos);
        }

        private static void removeAlbumPhotosExecution(IEnumerable<DataModel.PhotoDataStructure> results)
        {
            List<DataModel.PhotoDataStructure> needRemovePhotos = new List<DataModel.PhotoDataStructure>();
            foreach (var row in results)
            {
                needRemovePhotos.Add(row as DataModel.PhotoDataStructure);
            }
            foreach (var row in needRemovePhotos)
            {
                Utils.LifeMapManager.GetInstance().ListOfAllPhotos.Remove(row as DataModel.PhotoDataStructure);
            }
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
                        this.Frame.Navigate(typeof(MapView));
                    }));
                    dialog.Commands.Add(new UICommand(Utils.Constants.ResourceLoader.GetString("noleaveforsave")));
                    await dialog.ShowAsync();
                }

                else
                {
                    this.Frame.Navigate(typeof(MapView));
                }
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - GoBackClick"); }
        }

        private async void Button_SaveCollection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Constants.StartLoadingAnimation(MainGrid);
                pushpinData.AlbumCollectionName = pageTitle.Text;
                await Utils.FilesSaver<DataModel.PushpinDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListPushpin, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Constants.NamingListPushpin);
                await Utils.FilesSaver<DataModel.AlbumDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListOfAllAlbums, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Constants.NamingListAlbums);
                Helper.CreateToastNotifications(Utils.Constants.ResourceLoader.GetString("PushpinCollectionIsSaved"));
                BottomAppBar.IsOpen = false;
                Constants.StopLoadingAnimation(MainGrid);
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - Button_SaveCollection_Click"); }
            justSaved = true;
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

        private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                itemGridView.SelectedItem = (DataModel.AlbumDataStructure)e.ClickedItem;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - ItemView_ItemClick"); }
        }

        private void Header_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Button_ChangeCollectionBackground_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				Utils.Constants.StartLoadingAnimation(MainGrid);
                StorageFile fileFromPicker = await selectPhotosFromFilePicker();
                if (fileFromPicker != null && fileFromPicker.Path != "")
                    this.pushpinData.BackgroundPhotoPath = fileFromPicker.Path;
				else if (fileFromPicker != null && fileFromPicker.Path == "")
					this.pushpinData.BackgroundPhotoPath = await fillBackgroundImageFromNonLocal(fileFromPicker);
				fillBackgroundImage(this.pushpinData.BackgroundPhotoPath);
                justSaved = false;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - Button_ChangeCollectionBackground_Click"); }
			Utils.Constants.StopLoadingAnimation(MainGrid);
        }

		private static async Task<string> fillBackgroundImageFromNonLocal(StorageFile storageFile)
        {
            try
            {
                IRandomAccessStream iras = await storageFile.OpenReadAsync();
                Windows.Storage.Streams.Buffer MyBuffer = new Windows.Storage.Streams.Buffer(Convert.ToUInt32(iras.Size));
                IBuffer iBuf = await iras.ReadAsync(MyBuffer, MyBuffer.Capacity, InputStreamOptions.None);
                string filename = DateTime.Now.ToString().Replace(":", "").Replace("/", "_").Replace("\\", "_").Replace(".", "").Replace("\"", "") + "lifemapcover" + storageFile.Name;
				return await Helper.SaveImages(iBuf, filename);
            }
            catch{
				return "";
			}
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
                    Utils.Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("2CannotReadFolderBackgroundPic") + " ： " +
													  path + "\n\r" +
													  Constants.ResourceLoader.GetString("2possiblereasondocumentlibararycannotaccess") + "\n\r" +
                                                      Constants.ResourceLoader.GetString("2possiblereasonpathfilechanged"));
                }
            }
        }

        private static async Task<StorageFile> selectPhotosFromFilePicker()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".jpg");
            StorageFile file = await openPicker.PickSingleFileAsync();
            return file;
        }

        #region change album name popup window
        private Popup settingsPopup;
        private double settingsWidth = 500;
        private Rect windowBounds;

        private void Button_ChangeCollectionName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                windowBounds = Window.Current.Bounds;
                createPopupWindowContainsFlyout("ChangeCollectionName");
                justSaved = false;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - Button_ChangeCollectionName_Click"); }
        }

        private void createPopupWindowContainsFlyout(string options)
        {
            settingsPopup = new Popup();
            settingsPopup.Closed += OnPopupClosed;
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

            if (options == "ChangeCollectionName")
            {
                // Create a SettingsFlyout the same dimenssions as the Popup.
                ChangeCollectionNameFlyout mypane = new ChangeCollectionNameFlyout(pageTitle);
                settingsWidth = 650;
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
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - OnWindowActivated"); }
        }

        void OnPopupClosed(object sender, object e)
        {
            try
            {
                Window.Current.Activated -= OnWindowActivated;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - OnPopupClosed"); }
        }
        #endregion

        private void Button_AddVideoAlbum_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataModel.AlbumDataStructure newVideoAlbum = new DataModel.AlbumDataStructure()
                {
                    AlbumName = "",
                    AlbumType = "Video",
                    Longitude = pushpinData.Longitude,
                    Latitude = pushpinData.Latitude,
                    PushpinId = DateTime.Now.ToString() + pushpinData.Longitude
                };
                FromMapToCollectioin parameter = new FromMapToCollectioin();
                parameter.slectedAlbum = newVideoAlbum;
                parameter.pushpin = pushpinData;
                this.Frame.Navigate(typeof(VideoAlbumModifyView), parameter);
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - Button_AddVideoAlbum_Click"); }
        }

        private async void Button_MakeAGift_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageDialog dialog = new MessageDialog(Utils.Constants.ResourceLoader.GetString("surveyfornewfunction"), Utils.Constants.ResourceLoader.GetString("survey"));
                dialog.Commands.Add(new UICommand(Utils.Constants.ResourceLoader.GetString("ConfirmationForAgreeSurvey"), async p =>
                {
                    var mailto = new Uri("mailto:?to=comiscience@hotmail.fr&subject=I want have our real photo/video album, and this is my suggestion&body=Send from my Windows RT device");
                    await Windows.System.Launcher.LaunchUriAsync(mailto);
                }));
                dialog.Commands.Add(new UICommand(Utils.Constants.ResourceLoader.GetString("ConfirmationForDeletePushpinNo")));
                await dialog.ShowAsync();
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - Button_MakeAGift_Click"); }
        }

        private void Button_Synchronisation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //MyWorld.Common.FromMapToCollectioin parameter = new MyWorld.Common.FromMapToCollectioin();
                //parameter.slectedAlbum = itemGridView.SelectedItem as DataModel.AlbumDataStructure;
                //parameter.pushpin = pushpinData;
                //dispatcherTimer.Stop();
                //this.Frame.Navigate(typeof(View.SkyDrive.CollectionSynchronisationPage), parameter);
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "AlbumCollectionView - Button_Synchronisation_Click"); }
        }

        private void TextBlock_AlbumSubCollectionDate_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = groupedItemsViewSource.View.CollectionGroups;
            semanticZoom.ToggleActiveView();
        }

        private void AppBar_TopAppBar_Closed(object sender, object e)
        {
            TopAppBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void Grid_Item_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TopAppBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            TopAppBar.IsOpen = true;
            BottomAppBar.IsOpen = false;
        }

        private void Button_AddAlbum_Click(object sender, RoutedEventArgs e)
        {
            //Border_AddAlbum.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Popup_AddAlbum.Closed += OnPopupClosed;
            Window.Current.Activated += OnWindowActivated;
            Popup_AddAlbum.IsLightDismissEnabled = true;
            Popup_AddAlbum.Width = 120;
            Popup_AddAlbum.Height = 100;

            // Add the proper animation for the panel.
            Popup_AddAlbum.ChildTransitions = new TransitionCollection();

            Popup_AddAlbum.VerticalOffset = this.ActualHeight/2 - 100;
            Popup_AddAlbum.HorizontalOffset = this.ActualWidth/2 - 120;
            Popup_AddAlbum.IsOpen = true;
        }

        private void AppBar_BottomAppBar_Closed(object sender, object e)
        {
            //Border_AddAlbum.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
