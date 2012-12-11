using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Surface_Maps.DataModel;

namespace Surface_Maps
{
    public class Data
    {
        public static Utils.LifeMapManager LifeMapMgr = Utils.LifeMapManager.GetInstance();
    }
}

namespace Surface_Maps.Utils
{
    public class LifeMapManager : DataModel.INotifyPropertyChangedBaseClass
    {
        private ObservableCollection<DataModel.LifeMapStructure> lifeMaps = new ObservableCollection<DataModel.LifeMapStructure>();
        public ObservableCollection<DataModel.LifeMapStructure> LifeMaps
        {
            get
            {
                return lifeMaps;
            }
            set
            {
                lifeMaps = value;
                NotifyPropertyChanged("LifeMaps");
            }
        }
        public DataModel.LifeMapStructure SelectedLifeMap;

        public ObservableCollection<DataModel.PushpinDataStructure> ListPushpin;
        public ObservableCollection<DataModel.AlbumDataStructure> ListOfAllAlbums;
        public ObservableCollection<DataModel.PhotoDataStructure> ListOfAllPhotos;
        public ObservableCollection<DataModel.VideoDataStructure> ListOfAllVideos;



        private LifeMapManager()
        {
        }

        public async Task InitializeLifeMapManager()
        {
            ObservableCollection<PushpinDataStructure> ListPushpin = new ObservableCollection<PushpinDataStructure>();
            ObservableCollection<AlbumDataStructure> ListOfAllAlbums = new ObservableCollection<AlbumDataStructure>();
            ObservableCollection<PhotoDataStructure> ListOfAllPhotos = new ObservableCollection<PhotoDataStructure>();
            ObservableCollection<VideoDataStructure> ListOfAllVideos = new ObservableCollection<VideoDataStructure>();
            await loadLifeMaps();
        }

        private async Task loadLifeMaps()
        {
            var data = await Helper.GetContent<ObservableCollection<DataModel.LifeMapStructure>>(Utils.Constants.NamingListLifeMaps);
            if (data != null) LifeMaps = new ObservableCollection<DataModel.LifeMapStructure>(data);
			if(LifeMaps == null) return;
            for (int i = 0; i < LifeMaps.Count; i++)
            {
				try
				{
                    if (LifeMaps[i].ImagePath != "")
                    {
                        StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(LifeMaps[i].ImagePath);
                        if (file != null)
                            await setAlbumProfileImage(LifeMaps[i], file);
                    }
                    else
                    {
                    }
				}
                catch
                {
                    //Utils.Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("cannotreadfilepossiblereason") + "\n\r" +
                    //                                  Constants.ResourceLoader.GetString("documentlibararycannotaccess") + "\n\r" +
                    //                                  Constants.ResourceLoader.GetString("pathfilechanged"));
                }
                lifeMaps[i].Width = Utils.Constants.ScreenWidth / 5;
                lifeMaps[i].Height = Utils.Constants.ScreenHeight - 320;
            }
            await loadDefaultLifeMap();
        }

        private async Task loadDefaultLifeMap()
        {
            if (LifeMaps != null && LifeMaps.Count != 0) return;
            if (LifeMaps == null) LifeMaps = new ObservableCollection<LifeMapStructure>();
            LifeMaps.Add(new DataModel.LifeMapStructure()
            {
                Id = "LifeMap_" + DateTime.Now.ToString().Replace(" ", "").Replace("/", "").Replace(":", "") + "_" + Data.LifeMapMgr.LifeMaps.Count.ToString(),
                Name = Utils.Constants.ResourceLoader.GetString("HelpLifeMapName"),
                ImagePath = "",
                Height = Utils.Constants.ScreenHeight - 320,
                Width = Utils.Constants.ScreenWidth / 5
            });
            await Utils.FilesSaver<LifeMapStructure>.SaveData(Data.LifeMapMgr.LifeMaps, Utils.Constants.NamingListLifeMaps);
            Helper.CreateToastNotifications(Constants.ResourceLoader.GetString("AddedLiftMap"));
        }

        public BitmapImage ImageFromRelativePath(FrameworkElement parent, string path)
        {
            var uri = new Uri(parent.BaseUri, path);
            BitmapImage result = new BitmapImage();
            result.UriSource = uri;
            return result;
        }

        private async Task loadLifeMapData(LifeMapStructure lifeMap)
        {
            var pushpins = await Helper.GetContent<ObservableCollection<PushpinDataStructure>>(lifeMap.Name + "Pushpins");
            if (pushpins != null) ListPushpin = new ObservableCollection<PushpinDataStructure>(pushpins);
            var albums = await Helper.GetContent<ObservableCollection<AlbumDataStructure>>("Albums");
            if (albums != null) ListOfAllAlbums = new ObservableCollection<AlbumDataStructure>(albums);
            var photos = await Helper.GetContent<ObservableCollection<PhotoDataStructure>>("Photos");
            if (photos != null) ListOfAllPhotos = new ObservableCollection<PhotoDataStructure>(photos);
            var videos = await Helper.GetContent<ObservableCollection<VideoDataStructure>>("Videos");
            if (videos != null) ListOfAllVideos = new ObservableCollection<VideoDataStructure>(videos);
        }

        public void changeSelectedLifeMapName(DataModel.LifeMapStructure lifeMap, string name)
        {
            if (lifeMap != null && LifeMaps.Contains(lifeMap)) lifeMap.Name = name;
        }

        private async Task setAlbumProfileImage(DataModel.LifeMapStructure lifeMap, StorageFile file)
        {
            StorageItemThumbnail fileThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 800);
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.SetSource(fileThumbnail);
            lifeMap.Image = bitmapImage;
        }

        public static LifeMapManager GetInstance()
        {
            return Utils.Singleton<LifeMapManager>.Instance;
        }
    }
}
