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
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Media.Animation;
using Surface_Maps.Utils;
using Surface_Maps.DataModel;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Surface_Maps
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class MainPage : Surface_Maps.Common.LayoutAwarePage
    {
        public static MainPage MainPageUIElement;

        public MainPage()
        {
            this.InitializeComponent();
            MainPageUIElement = this;
            this.Loaded += MainPage_Loaded;
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Utils.Constants.ScreenHeight = this.ActualHeight;
                Utils.Constants.HalfScreenHeight = (Utils.Constants.ScreenHeight - 230) / 2;
                Utils.Constants.ScreenWidth = this.ActualWidth;
                if (Data.LifeMapMgr != null)
                {
                    if (await Data.LifeMapMgr.InitializeLifeMapManager() == true) // TODO: First time use, so show user guide panel
                    {
                        windowBounds = Window.Current.Bounds;
                        createPopupWindowContainsFlyout("Helps");
                    }
                }
                itemGridView.ItemsSource = Data.LifeMapMgr.LifeMaps;
                itemGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                if (Data.LifeMapMgr.LifeMaps.Count == 1)
                    itemGridView.SelectedIndex = 0;
                if (Data.LifeMapMgr.LifeMaps.Count <= 1)
                    BottomAppBar.IsOpen = true;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MainPage - MainPage_Loaded"); }
            Utils.Constants.StopLoadingAnimation(MainGrid);
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Assign a bindable collection of items to this.DefaultViewModel["Items"]
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            itemGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            Utils.Constants.StartLoadingAnimation(MainGrid);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        private void Button_EnterSelectedLifeMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (itemGridView.SelectedItem == null) return;
                Utils.LifeMapManager.GetInstance().SelectedLifeMap = itemGridView.SelectedItem as DataModel.LifeMapStructure;
                this.Frame.Navigate(typeof(Pages.MapView), "reset");
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MainPage - Button_EnterSelectedLifeMap_Click"); }
        }

        private void enterHelpPage()
        {
            try
            {
                windowBounds = Window.Current.Bounds;
                createPopupWindowContainsFlyout("Helps");
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "App - helpSettingCommand"); }
        }

        private async void Button_AddNewLifeMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Data.LifeMapMgr.LifeMaps.Insert(0,
                new LifeMapStructure()
                {
                    Id = "LifeMap_" + DateTime.Now.ToString().Replace(" ", "").Replace("/", "").Replace(":", "") + "_" + Data.LifeMapMgr.LifeMaps.Count.ToString(),
                    Name = "",
                    ImagePath = "",
                    Height = Utils.Constants.ScreenHeight - 320,
                    Width = Utils.Constants.ScreenWidth / 5
                });
                itemGridView.SelectedItem = Data.LifeMapMgr.LifeMaps.First();
                await Utils.FilesSaver<LifeMapStructure>.SaveData(Data.LifeMapMgr.LifeMaps, Utils.Constants.NamingListLifeMaps);
                Helper.CreateToastNotifications(Constants.ResourceLoader.GetString("AddedLiftMap"));
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "MainPage - Button_AddNewLifeMap_Click"); }
        }

        public BitmapImage ImageFromRelativePath(FrameworkElement parent, string path)
        {
            var uri = new Uri(parent.BaseUri, path);
            BitmapImage result = new BitmapImage();
            result.UriSource = uri;
            return result;
        }

        private async void Button_RemoveLifeMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Data.LifeMapMgr.LifeMaps.Contains(itemGridView.SelectedItem)) return;
                MessageDialog dialog = new MessageDialog(Utils.Constants.ResourceLoader.GetString("ConfirmationForDeleteLifeMap"), Utils.Constants.ResourceLoader.GetString("ConfirmationForDeletePushpinNotification"));
                dialog.Commands.Add(new UICommand(Utils.Constants.ResourceLoader.GetString("ConfirmationForDeletePushpinYes"), async p =>
                {
                    await RemoveLifeMapConcernedALbumPhotoVideoFiles();
                    Data.LifeMapMgr.LifeMaps.Remove(itemGridView.SelectedItem as LifeMapStructure);
                    await Utils.FilesSaver<LifeMapStructure>.SaveData(Data.LifeMapMgr.LifeMaps, Constants.NamingListLifeMaps);
                    Helper.CreateToastNotifications(Constants.ResourceLoader.GetString("RemovedLiftMap"));
                }));
                dialog.Commands.Add(new UICommand(Utils.Constants.ResourceLoader.GetString("ConfirmationForDeletePushpinNo")));
                await dialog.ShowAsync();
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MainPage - Button_RemoveLifeMap_Click"); }
        }

        private async Task RemoveLifeMapConcernedALbumPhotoVideoFiles()
        {
            await RemoveALocalFile((itemGridView.SelectedItem as LifeMapStructure).Id + Constants.NamingListPushpin);
            await RemoveALocalFile((itemGridView.SelectedItem as LifeMapStructure).Id + Constants.NamingListAlbums);
            await RemoveALocalFile((itemGridView.SelectedItem as LifeMapStructure).Id + Constants.NamingListPhotos);
            await RemoveALocalFile((itemGridView.SelectedItem as LifeMapStructure).Id + "ListVideos");
        }

        private async Task RemoveALocalFile(string fileName)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            try
            {
                if (await CheckFileExist(folder, fileName))
                {
                    StorageFile file = await folder.GetFileAsync(fileName);
                    await file.DeleteAsync();
                }
            }
            catch (Exception ex) { throw ex; }
        }

        private async Task<bool> CheckFileExist(StorageFolder folder, string fileName)
        {
            try
            {
                var files = await folder.GetFileAsync(fileName);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        private void itemGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (itemGridView.SelectedItem == null) return;
                Utils.LifeMapManager.GetInstance().SelectedLifeMap = itemGridView.SelectedItem as LifeMapStructure;
                if (Utils.LifeMapManager.GetInstance().SelectedLifeMap != null)
                {
                    StackPanel_MapCommand.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    BottomAppBar.IsOpen = true;
                }
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MainPage - itemGridView_SelectionChanged"); }
        }

        private void Button_ChangeLifeMapName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (itemGridView.SelectedItem == null) return;
                TextBox_ChangeLiftMapName.Text = (itemGridView.SelectedItem as LifeMapStructure).Name;
                //TextBox_ChangeLiftMapName.Opacity = 1;
                //Button_UpdateLiftMapName.Opacity = 1;
                //BottomAppBar.IsOpen = true;
                StackPanel_ChangeMapName.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "MainPage - Button_ChangeLifeMapName_Click"); }
        }

        private async void Button_UpdateLiftMapName_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				if(itemGridView == null || itemGridView.SelectedItem == null) return;
                (itemGridView.SelectedItem as LifeMapStructure).Name = TextBox_ChangeLiftMapName.Text;
                await Utils.FilesSaver<LifeMapStructure>.SaveData(Data.LifeMapMgr.LifeMaps, Constants.NamingListLifeMaps);
                Helper.CreateToastNotifications(Constants.ResourceLoader.GetString("UpdatedLiftMapName"));
                StackPanel_ChangeMapName.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "MainPage - Button_UpdateLiftMapName_Click"); }
        }

        private async void Button_ChangeLifeMapBackground_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Data.LifeMapMgr.SelectedLifeMap == null) return;
                StorageFile fileFromPicker = await selectPhotosFromFilePicker();
                if (fileFromPicker != null)
                {
                    Data.LifeMapMgr.SelectedLifeMap.ImagePath = fileFromPicker.Path;
                    fillBackgroundImage(Data.LifeMapMgr.SelectedLifeMap, fileFromPicker);
                }
                //else if (fileFromPicker != null && fileFromPicker.Path == "")
                //{
                //    Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("dontsupportnonlocalfile"));
                //}
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "MainPage - Button_ChangeLifeMapBackground_Click"); }
        }

        private async void fillBackgroundImage(LifeMapStructure lifeMap, StorageFile storageFile)
        {
            Utils.Constants.StartLoadingAnimation(MainGrid);
            if (lifeMap.ImagePath != "")
            {
                try
                {
                    StorageFile sf = await Windows.Storage.StorageFile.GetFileFromPathAsync(storageFile.Path);
                    StorageItemThumbnail fileThumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem, 800);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(fileThumbnail);
                    lifeMap.Image = bitmapImage;
                    await Utils.FilesSaver<LifeMapStructure>.SaveData(Data.LifeMapMgr.LifeMaps, Constants.NamingListLifeMaps);
                    Helper.CreateToastNotifications(Constants.ResourceLoader.GetString("UpdatedLiftMapBackground"));
                }
                catch
                {
					lifeMap.ImagePath = "";
                    Utils.Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("2cannotreadfile") + " ： " + storageFile.Path + "\n\r" +
                                                      Constants.ResourceLoader.GetString("2possiblereasondocumentlibararycannotaccess"));
                }
            }
            else
            {
                await fillBackgroundImageNonLocal(lifeMap, storageFile);
            }
            Utils.Constants.StopLoadingAnimation(MainGrid);
        }

        private static async Task fillBackgroundImageNonLocal(LifeMapStructure lifeMap, StorageFile storageFile)
        {
            try
            {
                IRandomAccessStream iras = await storageFile.OpenReadAsync();
                Windows.Storage.Streams.Buffer MyBuffer = new Windows.Storage.Streams.Buffer(Convert.ToUInt32(iras.Size));
                IBuffer iBuf = await iras.ReadAsync(MyBuffer, MyBuffer.Capacity, InputStreamOptions.None);
                string filename = DateTime.Now.ToString().Replace(":", "").Replace("/", "_").Replace("\\", "_").Replace(".", "").Replace("\"", "") + "lifemapcover" + storageFile.Name;
                string filePath = await Helper.SaveImages(iBuf, filename);
                lifeMap.ImagePath = filePath;
                await Utils.FilesSaver<LifeMapStructure>.SaveData(Data.LifeMapMgr.LifeMaps, Constants.NamingListLifeMaps);
                StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(lifeMap.ImagePath);
                StorageItemThumbnail fileThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 800);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileThumbnail);
                lifeMap.Image = bitmapImage;
                Helper.CreateToastNotifications(Constants.ResourceLoader.GetString("UpdatedLiftMapBackground"));
            }
            catch(Exception exp)
            {
				lifeMap.ImagePath = "";
				Utils.Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("2cannotreadfile") + " ： " + exp.Message + "\n\r" +
                                                  Constants.ResourceLoader.GetString("2possiblereasoncannotaccessnonlocalfile"));
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

            if (options == "Helps")
            {
                // Create a SettingsFlyout the same dimenssions as the Popup.
                SettingCommands.HelpSettingsFlyout mypane = new SettingCommands.HelpSettingsFlyout();
                settingsWidth = Constants.ScreenWidth;
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

        private void AppBar_BottomAppBar_Closed(object sender, object e)
        {
            StackPanel_ChangeMapName.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            StackPanel_MapCommand.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //TextBox_ChangeLiftMapName.Opacity = 0;
            //Button_UpdateLiftMapName.Opacity = 0;
        }

        private void LifeMapGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            StackPanel_MapCommand.Visibility = Windows.UI.Xaml.Visibility.Visible;
            BottomAppBar.IsOpen = true;
        }
    }
}
