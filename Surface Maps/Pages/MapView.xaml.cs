using Bing.Maps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.System.Threading;
using Windows.Networking.PushNotifications;
using Windows.UI.Notifications;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Geolocation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage.FileProperties;
using Windows.System;
using Surface_Maps.Utils;
using Windows.UI.Xaml.Media.Animation;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace Surface_Maps.Pages
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class MapView : Surface_Maps.Common.LayoutAwarePage, INotifyPropertyChanged
    {
        /// <summary>
        /// 一个IAsyncAction,用于在线程池中执行异步线程的实例
        /// </summary>
        public IAsyncAction threadPoolWorkItem;

        public static Frame MapPageFrame = null;

        /// <summary>
        /// 当前选中的（在地图中变大的）pushpin
        /// </summary>
        private DiyPushpin currentSelectedPushpin;

        private Geolocator geolocator = null;
        private CancellationTokenSource ctsForGeolocator = null;

        private enum TapStateEnum
        {
            MovePushpin,
            NewPushpinPhotoVideo,
            NewPushpinFile,
            Normal
        }
        private TapStateEnum _tapStateEnum = TapStateEnum.Normal;

        public MapView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            this.Loaded += MapView_Loaded;
        }

        async void MapView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                pageTitle.Text = Data.LifeMapMgr.SelectedLifeMap.Name;
                await InitializeWillEnterLifeMapDatas();
                MapPageFrame = this.Frame;

                map.Culture = Utils.Constants.ResourceLoader.GetString("MapCulture");
                setMapHomeRegion();
                GetTileUpdate();

                if (threadPoolWorkItem == null)                   // 利用线程池，在一个新线程中模拟加载已有pushpin数据，并每发现一个pushpin数据，就返回给UI进行
                {                                                 // 在map上添加pushpin的UI操作。读取pushpin数据和加载UImap的操作是同时进行的。
                    intializeAndAddPushpinsOnTheMap();
                    readPushpinsInfoThreadPoolCompleted(); // 如果读取pushpin信息的线程结束，那就结束转到这页时的加载动画
                }
                if (Utils.Constants.IsGeolized == false)
                {
                    geolocalize(13);
                    Utils.Constants.IsGeolized = true;
                }
				
				BottomAppBar.IsOpen = true;
				TopAppBar.IsOpen = true;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - MapView_Loaded"); }
        }

        private async void geolocalize(int zoomLevel)
        {
            try
            {
                geolocator = new Geolocator();
                ctsForGeolocator = new CancellationTokenSource();
                CancellationToken token = ctsForGeolocator.Token;
                Geoposition pos = await geolocator.GetGeopositionAsync().AsTask(token);
                if (pos.Coordinate != null)
                {
                    map.SetView(new Location(pos.Coordinate.Latitude, pos.Coordinate.Longitude), zoomLevel);
                }
                //map.SetZoomLevelAroundPoint(15, new Point(pos.Coordinate.Longitude, pos.Coordinate.Latitude), new TimeSpan(0, 0, 1));
            }
            catch
            { }
        }

        private void intializeAndAddPushpinsOnTheMap()
        {
            threadPoolWorkItem = Windows.System.Threading.ThreadPool.RunAsync(
            async (source) =>
            {
                // 如果有对WorkItem的取消申请，那么还没有开始的Work items会被取消
                // 如果work item已经开始运行了，如果他不支持取消功能的话，那会一直执行到结束
                // 为了支持取消能力，work item需要检查IAsyncAction.Status来确定取消这个状态
                // 并且完全的退出如果被执行了取消命令
                if (source.Status == AsyncStatus.Canceled)
                {
                    return;
                }
                await addAPushpinOnTheMap();
            }, WorkItemPriority.Normal);
        }

        private async Task addAPushpinOnTheMap()
        {
            if (Utils.LifeMapManager.GetInstance().ListPushpin == null) return;
            for (int i = 0; i < Data.LifeMapMgr.ListPushpin.Count; i++)
            {
                await Dispatcher.RunAsync(
                        CoreDispatcherPriority.High, () =>
                        {
                            this.UpdateWorkItem(Utils.LifeMapManager.GetInstance().ListPushpin[i]);
                        });
            }
        }

        private void readPushpinsInfoThreadPoolCompleted()
        {
            threadPoolWorkItem.Completed = new AsyncActionCompletedHandler(
                async (IAsyncAction source, AsyncStatus status) =>
                {
                    await Dispatcher.RunAsync(
                            CoreDispatcherPriority.High,
                            executionsAfterReadPushpinInfoThreadPoolComplted(status));
                });
        }

        private DispatchedHandler executionsAfterReadPushpinInfoThreadPoolComplted(AsyncStatus status)
        {
            return () =>
            {
                switch (status)
                {
                    case AsyncStatus.Started:
                        break;
                    case AsyncStatus.Completed:
                        stopLoadingAnimation();
                        break;
                }
            };
        }

        /// <summary>
        /// 初始化页面，加载地图中的Pushpin.这个函数被异步的线程调用。每次被调用，有一个pushpin会加载到地图上
        /// </summary>
        /// <param name="pushpinItem"></param>
        public void UpdateWorkItem(DataModel.PushpinDataStructure pushpinItem)
        {
            DiyPushpin diyPushpin = new DiyPushpin();
            diyPushpin.latitude = pushpinItem.Latitude;
            diyPushpin.longitude = pushpinItem.Longitude;
            diyPushpin.map = this.map;
            map.Children.Add(diyPushpin);
            MapLayer.SetPosition(diyPushpin, new Location(diyPushpin.latitude,
                                                          diyPushpin.longitude));
            diyPushpin.Tapped += diyPushpin_Tapped;
            diyPushpin.SetPushpinDataSource(pushpinItem);
        }

        void diyPushpin_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (((DiyPushpin)sender) == null) return;
                if (currentSelectedPushpin == ((DiyPushpin)sender) && currentSelectedPushpin.getPushpinTextBlock_TitleHeight() != 0)
                {
                    zoominCurrentSelectedPushpinSize();
                    currentSelectedPushpin = null;
                }
                else
                {
                    changeCurrentSelectedPushpin(sender);
                }
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - diyPushpin_Tapped"); }
        }

        private void changeCurrentSelectedPushpin(object sender)
        {
            zoominCurrentSelectedPushpinSize();
            currentSelectedPushpin = ((DiyPushpin)sender);
            zoomoutCurrentSelectedPushpinSize();
            //BottomAppBar.IsOpen = true;
            map.Children.Remove(currentSelectedPushpin);
            map.Children.Add(currentSelectedPushpin);
            Location loc = new Location(currentSelectedPushpin.latitude, currentSelectedPushpin.longitude);
            MapLayer.SetPosition(currentSelectedPushpin, loc);
            ((DiyPushpin)sender).setPushpinTextOpacity(1);
        }

        private void zoominCurrentSelectedPushpinSize()
        {
            if (currentSelectedPushpin != null && map.Children.Contains(currentSelectedPushpin))
            {
                currentSelectedPushpin.toLargePushpin(false);
                currentSelectedPushpin.setPushpinTextOpacity(0);
            }
        }

        private void zoomoutCurrentSelectedPushpinSize()
        {
            if (currentSelectedPushpin != null)
            {
                currentSelectedPushpin.toLargePushpin(true);
                currentSelectedPushpin.setPushpinTextOpacity(1);
            }
        }

        /// <summary>
        /// 文件读取以及地图初始化加载完毕后，会让loading动画停止
        /// </summary>
        private void stopLoadingAnimation()
        {
            ProgressBar_PageLoading.IsIndeterminate = false;
            Grid_PageLoading.Height = 0;
            Grid_PageLoading.Width = 0;
        }

        TileUpdater notifier;

        private void GetTileUpdate()
        {
            notifier = TileUpdateManager.CreateTileUpdaterForApplication();
            notifier.EnableNotificationQueue(true);
        }

        private async System.Threading.Tasks.Task InitializeWillEnterLifeMapDatas()
        {
            var dataPushpins = await Utils.Helper.GetContent<ObservableCollection<DataModel.PushpinDataStructure>>(Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Utils.Constants.NamingListPushpin);
            Data.LifeMapMgr.ListPushpin = ((dataPushpins != null) ? new ObservableCollection<DataModel.PushpinDataStructure>(dataPushpins) :
                                                                    new ObservableCollection<DataModel.PushpinDataStructure>());

            var dataVideos = await Utils.Helper.GetContent<ObservableCollection<DataModel.VideoDataStructure>>(Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + "ListVideos");
            Data.LifeMapMgr.ListOfAllVideos = ((dataVideos != null) ? new ObservableCollection<DataModel.VideoDataStructure>(dataVideos) :
                                                                      new ObservableCollection<DataModel.VideoDataStructure>());

            var dataAlbums = await Utils.Helper.GetContent<ObservableCollection<DataModel.AlbumDataStructure>>(Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Utils.Constants.NamingListAlbums);
            Data.LifeMapMgr.ListOfAllAlbums = ((dataAlbums != null) ? new ObservableCollection<DataModel.AlbumDataStructure>(dataAlbums) :
                                                                      new ObservableCollection<DataModel.AlbumDataStructure>());

            var dataPhotos = await Utils.Helper.GetContent<ObservableCollection<DataModel.PhotoDataStructure>>(Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Utils.Constants.NamingListPhotos);
            Data.LifeMapMgr.ListOfAllPhotos = ((dataPhotos != null) ? new ObservableCollection<DataModel.PhotoDataStructure>(dataPhotos) :
                                                                      new ObservableCollection<DataModel.PhotoDataStructure>());
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
            // Allow saved page state to override the initial item to display
            if (pageState != null && pageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = pageState["SelectedItem"];
            }

            // TODO: Assign a bindable group to this.DefaultViewModel["Group"]
            // TODO: Assign a collection of bindable items to this.DefaultViewModel["Items"]
            // TODO: Assign the selected item to this.flipView.SelectedItem
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {

            // TODO: Derive a serializable navigation parameter and assign it to pageState["SelectedItem"]
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                if (e.Parameter != null && e.Parameter.ToString() == "reset")
                {
                    resetMap();
                }

            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - OnNavigatedTo"); }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        private void resetMap()
        {
            map.Children.Clear();
            threadPoolWorkItem = null;
        }

        private void setMapHomeRegion()
        {
            if (map.HomeRegion == "CN" || map.HomeRegion == "KR" || map.HomeRegion == "AR" || map.HomeRegion == "MA" ||
                map.HomeRegion == "IN" || map.HomeRegion == "PK" || map.HomeRegion == "AZ")
                map.HomeRegion = "US";
        }

        private async void map_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                if (_tapStateEnum == TapStateEnum.NewPushpinPhotoVideo)
                    await addNewPhotoVideoPushpin(e);
                else if (_tapStateEnum == TapStateEnum.NewPushpinFile)
                    await addNewFilePushpin(e);
                else if (_tapStateEnum == TapStateEnum.MovePushpin)
                    await movePushpin(e);
                Grid_HandIndicate.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                _tapStateEnum = TapStateEnum.Normal;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - map_Tapped"); }
        }

        private async Task addNewFilePushpin(TappedRoutedEventArgs e)
        {
            Utils.Constants.StartLoadingAnimation(MainGrid);
            try
            {
                resetOldCurrentSelectedPushpin();
                StorageFile fileFromPicker = await selectPhotosFromFilePicker();
                if (fileFromPicker == null || fileFromPicker.Path == null)
                {
                    Utils.Constants.StopLoadingAnimation(MainGrid);
                    return;
                }
                if (fileFromPicker != null && fileFromPicker.Path == "")
                {
                    await addNewFileNonLocalPushpin2(e, fileFromPicker);
                    Utils.Constants.StopLoadingAnimation(MainGrid);
                    return;
                }
                Location loc = addNewPushpinLocatePushpinOnMapPosition(e);
                IEnumerable<DataModel.PushpinDataStructure> listPushPinTempo = addNewPushpinGetListPushpinTempo(loc);
                if (listPushPinTempo == null || listPushPinTempo.Count() == 0)
                {
                    await addNewFilePushpin2(fileFromPicker, loc);
                }
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - addNewFilePushpin"); }
            Utils.Constants.StopLoadingAnimation(MainGrid);
        }

        private async Task addNewFileNonLocalPushpin2(TappedRoutedEventArgs e, StorageFile fileFromPicker)
        {
            try
            {
                IRandomAccessStream iras = await fileFromPicker.OpenReadAsync();
                Windows.Storage.Streams.Buffer MyBuffer = new Windows.Storage.Streams.Buffer(Convert.ToUInt32(iras.Size));
                IBuffer iBuf = await iras.ReadAsync(MyBuffer, MyBuffer.Capacity, InputStreamOptions.None);
                string filename = DateTime.Now.ToString().Replace(":", "").Replace("/", "_").Replace("\\", "_").Replace(".", "").Replace("\"", "") + "lifemapcover" + fileFromPicker.Name;
                string filePath = await Helper.SaveFiles(iBuf, filename);

                Location loc = addNewPushpinLocatePushpinOnMapPosition(e);
                DiyPushpin diyPushpin = addNewPushpinLocatePushpin(loc);
                DataModel.PushpinDataStructure pds = addNewPushpinConfigureDataOfNewPushpin(diyPushpin);
                pds.PushpinType = DataModel.PushpinTypeEnum.File;
                pds.FilePath = filePath;
                pds.AlbumCollectionName = fileFromPicker.Name;
                await addNewPushpinSaveToFile(pds);
                diyPushpin.SetPushpinDataSource(pds);
                diyPushpin.setPushpinTextOpacity(1);
                addNewPushpinSetCurrentSelectedPushpin(diyPushpin);
                diyPushpin.map = this.map;
                addNewPushpinUpdateMapPageState();
            }
            catch
            {
                //Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("dontsupportnonlocalfile"));
            }
        }

        private async Task addNewPhotoVideoPushpin(TappedRoutedEventArgs e)
        {
            resetOldCurrentSelectedPushpin();
            Location loc = addNewPushpinLocatePushpinOnMapPosition(e);
            IEnumerable<DataModel.PushpinDataStructure> listPushPinTempo = addNewPushpinGetListPushpinTempo(loc);
            if (listPushPinTempo == null || listPushPinTempo.Count() == 0)
            {
                DiyPushpin diyPushpin = addNewPushpinLocatePushpin(loc);
                DataModel.PushpinDataStructure pds = addNewPushpinConfigureDataOfNewPushpin(diyPushpin);
                await addNewPushpinSaveToFile(pds);
                diyPushpin.SetPushpinDataSource(pds);
                addNewPushpinSetCurrentSelectedPushpin(diyPushpin);
                diyPushpin.setPushpinTextOpacity(1);
                diyPushpin.map = this.map;
                addNewPushpinUpdateMapPageState();
            }
        }

        private async Task addNewFilePushpin2(StorageFile fileFromPicker, Location loc)
        {
            DiyPushpin diyPushpin = addNewPushpinLocatePushpin(loc);
            DataModel.PushpinDataStructure pds = addNewPushpinConfigureDataOfNewPushpin(diyPushpin);
            pds.PushpinType = DataModel.PushpinTypeEnum.File;
            pds.FilePath = fileFromPicker.Path;
            pds.AlbumCollectionName = fileFromPicker.Name;
            await addNewPushpinSaveToFile(pds);
            diyPushpin.SetPushpinDataSource(pds);
            diyPushpin.setPushpinTextOpacity(1);
            addNewPushpinSetCurrentSelectedPushpin(diyPushpin);
            diyPushpin.map = this.map;
            addNewPushpinUpdateMapPageState();
        }

        private static async Task addNewPushpinSaveToFile(DataModel.PushpinDataStructure pds)
        {
            if (Utils.LifeMapManager.GetInstance().ListPushpin == null) Utils.LifeMapManager.GetInstance().ListPushpin = new ObservableCollection<DataModel.PushpinDataStructure>();
            Utils.LifeMapManager.GetInstance().ListPushpin.Add(pds);
            await Utils.FilesSaver<DataModel.PushpinDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListPushpin, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Constants.NamingListPushpin);
            Helper.CreateToastNotifications(Constants.ResourceLoader.GetString("ANewPushpinHasBeenAdded"));
        }



        private static async Task<StorageFile> selectPhotosFromFilePicker()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.Desktop;
            openPicker.FileTypeFilter.Add(".ppt");
            openPicker.FileTypeFilter.Add(".pdf");
            openPicker.FileTypeFilter.Add(".pptx");
            openPicker.FileTypeFilter.Add(".doc");
            openPicker.FileTypeFilter.Add(".docx");
            openPicker.FileTypeFilter.Add(".mp3");
            openPicker.FileTypeFilter.Add(".xls");
            openPicker.FileTypeFilter.Add(".xlsx");
            openPicker.FileTypeFilter.Add(".txt");
            openPicker.FileTypeFilter.Add(".wmv");
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".mp4");
            openPicker.FileTypeFilter.Add(".rmvb");
            openPicker.FileTypeFilter.Add(".avi");
            openPicker.FileTypeFilter.Add(".mkv");
            openPicker.FileTypeFilter.Add(".one");
            StorageFile file = await openPicker.PickSingleFileAsync();
            return file;
        }

        private void addNewPushpinSetCurrentSelectedPushpin(DiyPushpin diyPushpin)
        {
            if (currentSelectedPushpin != null && map.Children.Contains(currentSelectedPushpin))
            {
                currentSelectedPushpin.toLargePushpin(false);
            }
            currentSelectedPushpin = diyPushpin;
            currentSelectedPushpin.toLargePushpin(true);
        }

        private void addNewPushpinUpdateMapPageState()
        {
            _tapStateEnum = TapStateEnum.Normal;
            BottomAppBar.IsOpen = true;
            //Button_RemoveAPushPin.Opacity = 1;
            //Button_ShowPhotoByAlbum.Opacity = 1;
        }

        private DataModel.PushpinDataStructure addNewPushpinConfigureDataOfNewPushpin(DiyPushpin diyPushpin)
        {
            diyPushpin.Tapped += diyPushpin_Tapped;
            DataModel.PushpinDataStructure pds = new DataModel.PushpinDataStructure()
            {
                Latitude = diyPushpin.latitude,
                Longitude = diyPushpin.longitude,
                Id = DateTime.Now.ToString() + diyPushpin.latitude.ToString()
                //PushpinType = _tapStateEnum
            };
            return pds;
        }

        private DiyPushpin addNewPushpinLocatePushpin(Location loc)
        {
            DiyPushpin diyPushpin = new DiyPushpin();
            diyPushpin.latitude = loc.Latitude;
            diyPushpin.longitude = loc.Longitude;
            map.Children.Add(diyPushpin);
            MapLayer.SetPosition(diyPushpin, loc);
            return diyPushpin;
        }

        private Location addNewPushpinLocatePushpinOnMapPosition(TappedRoutedEventArgs e)
        {
            Point point = e.GetPosition(map);
            Location loc = new Location();
            map.TryPixelToLocation(point, out loc);
            return loc;
        }

        private IEnumerable<DataModel.PushpinDataStructure> addNewPushpinGetListPushpinTempo(Location loc)
        {
            if (Utils.LifeMapManager.GetInstance().ListPushpin == null) return null;
            IEnumerable<DataModel.PushpinDataStructure> listPushPinTempo = from datas in Utils.LifeMapManager.GetInstance().ListPushpin
                                                                           where datas.Latitude == loc.Latitude && datas.Longitude == loc.Longitude
                                                                           select datas;
            return listPushPinTempo;
        }

        private void resetOldCurrentSelectedPushpin()
        {
            if (currentSelectedPushpin != null && map.Children.Contains(currentSelectedPushpin))
            {
                currentSelectedPushpin.toLargePushpin(false);
                currentSelectedPushpin.setPushpinTextOpacity(0);
            }
        }

        private async Task movePushpin(TappedRoutedEventArgs e)
        {
            try
            {
                if (currentSelectedPushpin == null) return;
                Utils.Constants.StartLoadingAnimation(MainGrid);
                movePushpinRemoveOldPushpinFromMap();
                Location loc = movePushpinGetNewPositionLocation(e);
                await movePushpinUpdateSavedInfo(loc);
                movePushpinReLocateCurrentSelectedPushpin(loc);
                Utils.Helper.CreateToastNotifications(Constants.ResourceLoader.GetString("ASelectedPushpinHasBeenUpdated"));
                Utils.Constants.StopLoadingAnimation(MainGrid);
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - map_Tapped"); }
        }

        private async Task movePushpinUpdateSavedInfo(Location loc)
        {
            await movePushpinUpdatePushpinList(loc);
            await movePushpinUpdatePushpinAlbums(loc);
            await movePushpinUpdatePushpinPhotos(loc);
            await movePushpinUpdatePushpinVideos(loc);
        }

        private async Task movePushpinUpdatePushpinVideos(Location loc)
        {
            IEnumerable<DataModel.VideoDataStructure> ienumerableVideoTempo = from datas in Utils.LifeMapManager.GetInstance().ListOfAllVideos
                                                                              where datas.Latitude == currentSelectedPushpin.latitude && datas.Longitude == currentSelectedPushpin.longitude
                                                                              select datas;
            if (ienumerableVideoTempo == null || ienumerableVideoTempo.Count() == 0) return;
            List<DataModel.VideoDataStructure> listVideoTempo = new List<DataModel.VideoDataStructure>();
            foreach (var item in ienumerableVideoTempo) listVideoTempo.Add(item);
            for (int i = 0; i < listVideoTempo.Count(); i++)
            {
                listVideoTempo.ElementAt(i).Latitude = loc.Latitude;
                listVideoTempo.ElementAt(i).Longitude = loc.Longitude;
            }
            await Utils.FilesSaver<DataModel.VideoDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListOfAllVideos, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + "ListVideos");
        }

        private async Task movePushpinUpdatePushpinPhotos(Location loc)
        {
            IEnumerable<DataModel.PhotoDataStructure> ienumerablePhotoTempo = from datas in Utils.LifeMapManager.GetInstance().ListOfAllPhotos
                                                                              where datas.Latitude == currentSelectedPushpin.latitude && datas.Longitude == currentSelectedPushpin.longitude
                                                                              select datas;
            if (ienumerablePhotoTempo == null || ienumerablePhotoTempo.Count() == 0) return;
            List<DataModel.PhotoDataStructure> listPhotoTempo = new List<DataModel.PhotoDataStructure>();
            foreach (var item in ienumerablePhotoTempo) listPhotoTempo.Add(item);
            for (int i = 0; i < listPhotoTempo.Count; i++)
            {
                listPhotoTempo[i].Latitude = loc.Latitude;
                listPhotoTempo[i].Longitude = loc.Longitude;
            }
            await Utils.FilesSaver<DataModel.PhotoDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListOfAllPhotos, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Constants.NamingListPhotos);
        }

        private async Task movePushpinUpdatePushpinAlbums(Location loc)
        {
            IEnumerable<DataModel.AlbumDataStructure> ienumerableAlbumTempo = from datas in Utils.LifeMapManager.GetInstance().ListOfAllAlbums
                                                                              where datas.Latitude == currentSelectedPushpin.latitude && datas.Longitude == currentSelectedPushpin.longitude
                                                                              select datas;
            if (ienumerableAlbumTempo == null || ienumerableAlbumTempo.Count() == 0) return;
            List<DataModel.AlbumDataStructure> listAlbumTempo = new List<DataModel.AlbumDataStructure>();
            foreach (var item in ienumerableAlbumTempo) listAlbumTempo.Add(item);
            for (int i = 0; i < listAlbumTempo.Count(); i++)
            {
                listAlbumTempo.ElementAt(i).Latitude = loc.Latitude;
                listAlbumTempo.ElementAt(i).Longitude = loc.Longitude;
            }
            await Utils.FilesSaver<DataModel.AlbumDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListOfAllAlbums, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Constants.NamingListAlbums);
        }

        private async Task movePushpinUpdatePushpinList(Location loc)
        {
            DataModel.PushpinDataStructure ppds = (from datas in Utils.LifeMapManager.GetInstance().ListPushpin
                                                   where datas.Latitude == currentSelectedPushpin.latitude && datas.Longitude == currentSelectedPushpin.longitude &&
                                                         datas.AlbumCollectionName == currentSelectedPushpin.PushPinDataSource.AlbumCollectionName
                                                   select datas).First();
            ppds.Latitude = loc.Latitude;
            ppds.Longitude = loc.Longitude;
            currentSelectedPushpin.SetPushpinDataSource(ppds);
            await Utils.FilesSaver<DataModel.PushpinDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListPushpin, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Constants.NamingListPushpin);
        }

        private Location movePushpinGetNewPositionLocation(TappedRoutedEventArgs e)
        {
            Point point = e.GetPosition(map);
            Location loc = new Location();
            map.TryPixelToLocation(point, out loc);
            return loc;
        }

        private void movePushpinReLocateCurrentSelectedPushpin(Location loc)
        {
            currentSelectedPushpin.longitude = loc.Longitude;
            currentSelectedPushpin.latitude = loc.Latitude;
            map.Children.Add(currentSelectedPushpin);
            MapLayer.SetPosition(currentSelectedPushpin, loc);
        }

        private void movePushpinRemoveOldPushpinFromMap()
        {
            if (map.Children.Contains(currentSelectedPushpin))
                map.Children.Remove(currentSelectedPushpin);
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
            try
            {
                this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
                this.Frame.Navigate(typeof(MainPage));
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - GoBackClick"); }
        }

        private void Button_Localize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Utils.Constants.StartLoadingAnimation(MainGrid);
                geolocalize(13);
                Utils.Constants.StopLoadingAnimation(MainGrid);
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - Button_Localize_Click"); }
        }

        private void Button_AddAFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tapStateEnum = TapStateEnum.NewPushpinFile;
                Grid_HandIndicate.Visibility = Windows.UI.Xaml.Visibility.Visible;
                TextBlock_MapHandIndicate.Text = Utils.Constants.ResourceLoader.GetString("AddFileHandIndicateText");
                hideAppBars();
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - Button_AddAFile_Click"); }
        }

        private void hideAppBars()
        {
            BottomAppBar.IsOpen = false;
            TopAppBar.IsOpen = false;
        }

        private void Button_AddAPushPin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Popup_AddElement.Closed += OnPopupClosed;
                Window.Current.Activated += OnWindowActivated;
                Popup_AddElement.IsLightDismissEnabled = true;
                Popup_AddElement.Width = 300;
                Popup_AddElement.Height = 100;

                // Add the proper animation for the panel.
                Popup_AddElement.ChildTransitions = new TransitionCollection();

                Popup_AddElement.VerticalOffset = this.ActualHeight / 2 - 100;
                Popup_AddElement.HorizontalOffset = this.ActualWidth / 2 - 180;
                Popup_AddElement.IsOpen = true;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - Button_AddAPushPin_Click"); }
        }

        private void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            try
            {
                if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
                {
                    //settingsPopup.IsOpen = false;
                }
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - OnWindowActivated"); }
        }

        void OnPopupClosed(object sender, object e)
        {
            try
            {
                Window.Current.Activated -= OnWindowActivated;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - OnPopupClosed"); }
        }

        private void Button_MoveSelectedPushPin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (currentSelectedPushpin != null)
                {
                    _tapStateEnum = TapStateEnum.MovePushpin;
                    Grid_HandIndicate.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    TextBlock_MapHandIndicate.Text = Utils.Constants.ResourceLoader.GetString("AddRemoveHandIndicateText");
                    hideAppBars();
                }
                else
                    _tapStateEnum = TapStateEnum.Normal;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - Button_MoveSelectedPushPin_Click"); }
        }

        private void Button_DisplayHidePushpin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (System.Math.Round(Button_DisplayHidePushpin.Opacity, 1) != 0.6)
                {
                    hideAllPushpins();
                    Button_DisplayHidePushpin.Opacity = 0.6;
                }
                else
                {
                    showAllPushpins();
                    Button_DisplayHidePushpin.Opacity = 1;
                }
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - Button_DisplayHidePushpin_Click"); }
        }

        private void hideAllPushpins()
        {
            foreach (var child in map.Children)
            {
                if (child.GetType() == typeof(DiyPushpin))
                {
                    ((DiyPushpin)child).setOpacity(0);
                }
            }
        }

        private void showAllPushpins()
        {
            foreach (var child in map.Children)
            {
                if (child.GetType() == typeof(DiyPushpin))
                {
                    ((DiyPushpin)child).setOpacity(0.5);
                }
            }
            if (currentSelectedPushpin != null && map.Children.Contains(currentSelectedPushpin))
            {
                currentSelectedPushpin.setOpacity(1);
            }
        }

        private void Button_AddPhotoVideoAlbum_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tapStateEnum = TapStateEnum.NewPushpinPhotoVideo;
                Grid_HandIndicate.Visibility = Windows.UI.Xaml.Visibility.Visible;
                TextBlock_MapHandIndicate.Text = Utils.Constants.ResourceLoader.GetString("AddPushpinHandIndicateText");
                hideAppBars();
                Popup_AddElement.IsOpen = false;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - Button_AddPhotoVideoAlbum_Click"); }
        }

        private void Button_AddSingleFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _tapStateEnum = TapStateEnum.NewPushpinFile;
                Grid_HandIndicate.Visibility = Windows.UI.Xaml.Visibility.Visible;
                TextBlock_MapHandIndicate.Text = Utils.Constants.ResourceLoader.GetString("AddFileHandIndicateText");
                hideAppBars();
                Popup_AddElement.IsOpen = false;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "MapView - Button_AddSingleFile_Click"); }
        }
    }
}
