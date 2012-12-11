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
using System.ComponentModel;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Popups;
using Windows.System;
using Windows.Storage;
using System.Threading.Tasks;
using Bing.Maps;
using Surface_Maps.Utils;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Surface_Maps.Pages
{
    public sealed partial class DiyPushpin : UserControl, INotifyPropertyChanged
    {
        public double latitude = 0;
        public double longitude = 0;

        public Map map = null;

        private DataModel.PushpinDataStructure pushPinDataSource;
        public DataModel.PushpinDataStructure PushPinDataSource
        {
            get
            {
                return pushPinDataSource;
            }
            private set
            {
                pushPinDataSource = value;
                NotifyPropertyChanged("PushPinDataSource");
            }
        }

        public void SetPushpinDataSource(DataModel.PushpinDataStructure pushPinDataSource)
        {
            PushPinDataSource = pushPinDataSource;
            if (pushPinDataSource.FilePath == null) return;
            if (pushPinDataSource.FilePath.EndsWith(".pdf") || pushPinDataSource.FilePath.EndsWith(".xls") || pushPinDataSource.FilePath.EndsWith(".xlsx") || pushPinDataSource.FilePath.EndsWith(".one") ||
                pushPinDataSource.FilePath.EndsWith(".ppt") || pushPinDataSource.FilePath.EndsWith(".pptx") || pushPinDataSource.FilePath.EndsWith(".doc") || pushPinDataSource.FilePath.EndsWith(".docx"))
                Image_LargePushpin.Source = ImageFromRelativePath(this, "/Assets/Pushpin/pushpinfile.png");
            else if (pushPinDataSource.FilePath.EndsWith(".mp3") || pushPinDataSource.FilePath.EndsWith(".wmv"))
                Image_LargePushpin.Source = ImageFromRelativePath(this, "/Assets/Pushpin/Music note SH.png");
            else if (pushPinDataSource.FilePath.EndsWith(".mp4") ||
                     pushPinDataSource.FilePath.EndsWith(".rmvb") || pushPinDataSource.FilePath.EndsWith(".avi") || pushPinDataSource.FilePath.EndsWith(".mkv"))
                Image_LargePushpin.Source = ImageFromRelativePath(this, "/Assets/Pushpin/logo_video.png");
            else if (pushPinDataSource.FilePath.EndsWith(".jpg") || pushPinDataSource.FilePath.EndsWith(".png"))
                Image_LargePushpin.Source = ImageFromRelativePath(this, "/Assets/Pushpin/logo_04.png");
            else
                Image_LargePushpin.Source = ImageFromRelativePath(this, "/Assets/Images/bigpushpin.png");
        }

        public DiyPushpin()
        {
            this.InitializeComponent();
            DataContext = this;

            TextBlock_Title.Height = 0;
            TextBlock_Title.Width = 0;

            Image_LargePushpin.Height = 50;
            Image_LargePushpin.Width = 50;
        }

        public BitmapImage ImageFromRelativePath(FrameworkElement parent, string path)
        {
            var uri = new Uri(parent.BaseUri, path);
            BitmapImage result = new BitmapImage();
            result.UriSource = uri;
            return result;
        }

        /// <summary>
        /// 只允许输入0，0.5和1三个值，只有这两个值有效
        /// 该函数将DiyPushpin的所有子图片和TextBlock的长宽设置为原始尺寸或0
        /// </summary>
        /// <param name="opacity"></param>
        public void setOpacity(double opactiy)
        {
            if (opactiy == 0)
            {
                Image_LargePushpin.Height = 0;
                Image_LargePushpin.Width = 0;
                TextBlock_Title.Height = 0;
                TextBlock_Title.Width = 0;
            }
            else if (opactiy == 0.5)
            {
                Image_LargePushpin.Height = 50;
                Image_LargePushpin.Width = 50;
            }
            else
            {
                Image_LargePushpin.Height = 70;
                Image_LargePushpin.Width = 70;
            }
        }

        /// <summary>
        /// 只允许输入0和1两个值，只有这两个值有效
        /// 该函数将DiyPushpin的标题TextBlock的长宽设置为原始尺寸或0
        /// </summary>
        /// <param name="opacity"></param>
        public void setPushpinTextOpacity(double opacity)
        {
            if (opacity < 0.1)
            {
                TextBlock_Title.Height = 0;
                TextBlock_Title.Width = 0;
            }
            else if (opacity > 0.1)
            {
                TextBlock_Title.Height = 85;
                TextBlock_Title.Width = TextBlock_PushpinTitle.Width + 100;
            }
        }

        public double getPushpinTextBlock_TitleHeight()
        {
            return TextBlock_Title.Height;
        }

        public void toLargePushpin(bool large)
        {
            if (large == true)
            {
                Image_LargePushpin.Height = 70;
                Image_LargePushpin.Width = 70;
            }
            else
            {
                Image_LargePushpin.Height = 50;
                Image_LargePushpin.Width = 50;
            }
        }

        #region ProertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        private void Button_ShowPhotoByAlbum_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.PushPinDataSource == null) return;
                if (this.PushPinDataSource.PushpinType == DataModel.PushpinTypeEnum.PhotoVideo)
                {
                    ShowPhotoVideoByAlbum();
                }
                else if (this.PushPinDataSource.PushpinType == DataModel.PushpinTypeEnum.File)
                {
                    asyncvoidDownloadAndOpenFile();
                }
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "DiyPushpin - Button_ShowPhotoByAlbum_Click"); }
        }

        private void ShowPhotoVideoByAlbum()
        {
            DataModel.PushpinDataStructure pushpin = Utils.LifeMapManager.GetInstance().ListPushpin.Where(p => p.Id == this.PushPinDataSource.Id).First();
            PageToPageParameterManager.parameters = new FromMapToCollectioin()
            {
                pushpin = pushpin,
                slectedAlbum = null
            };
            MapView.MapPageFrame.Navigate(typeof(AlbumCollectionView), PageToPageParameterManager.parameters);
        }

        private async void asyncvoidDownloadAndOpenFile()
        {
            MessageDialog dialog = null;
            try
            {
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(this.PushPinDataSource.FilePath);
                var targetStream = await file.OpenAsync(FileAccessMode.Read);
                await Launcher.LaunchFileAsync(file, new LauncherOptions { DisplayApplicationPicker = true });
            }
            catch (Exception e)
            {
                dialog = new MessageDialog(Utils.Constants.ResourceLoader.GetString("documentlibararycannotaccess") + "\n\r" + e.Message);
            }
            if (dialog != null) await dialog.ShowAsync();
        }

        private async void Button_RemoveAPushPin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (map.Children.Contains(this))
                {
                    MessageDialog dialog = new MessageDialog(Utils.Constants.ResourceLoader.GetString("ConfirmationForDeletePushpin"), Utils.Constants.ResourceLoader.GetString("ConfirmationForDeletePushpinNotification"));
                    dialog.Commands.Add(new UICommand(Utils.Constants.ResourceLoader.GetString("ConfirmationForDeletePushpinYes"), async p =>
                    {
                        await removeAPushPin();
                    }));
                    dialog.Commands.Add(new UICommand(Utils.Constants.ResourceLoader.GetString("ConfirmationForDeletePushpinNo")));
                    await dialog.ShowAsync();
                }
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "DiyPushpin - Button_RemoveAPushPin_Click"); }
        }

        private async Task removeAPushPin()
        {
            DataModel.PushpinDataStructure pushpinDS = (from datas in Utils.LifeMapManager.GetInstance().ListPushpin
                                                        where datas.Latitude == this.latitude && datas.Longitude == this.longitude
                                                        select datas).First();
            if (pushpinDS != null)
            {
                await removeAlbums(pushpinDS);
                Utils.LifeMapManager.GetInstance().ListPushpin.Remove(pushpinDS);
                map.Children.Remove(this);
                //currentSelectedPushpin = null;
            }
            await Utils.FilesSaver<DataModel.PushpinDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListPushpin, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Utils.Constants.NamingListPushpin);
            Utils.Helper.CreateToastNotifications(Utils.Constants.ResourceLoader.GetString("PushpinHasBeenRemoved"));
        }

        private async Task removeAlbums(DataModel.PushpinDataStructure pushpinDS)
        {
            var results = from datas in Utils.LifeMapManager.GetInstance().ListOfAllAlbums
                          where datas.Latitude == pushpinDS.Latitude && datas.Longitude == pushpinDS.Longitude
                          select datas;
            if (results != null)
            {
                await removeAlbumsExecution(results);
            }
        }

        private async Task removeAlbumsExecution(IEnumerable<DataModel.AlbumDataStructure> results)
        {
            List<DataModel.AlbumDataStructure> albums = new List<DataModel.AlbumDataStructure>();
            foreach (var row in results)
            {
                albums.Add(row as DataModel.AlbumDataStructure);
            }
            foreach (var row in albums)
            {
                await AlbumCollectionView.removeAlbumPhotos(row);
                Utils.LifeMapManager.GetInstance().ListOfAllAlbums.Remove(row);
            }
            await Utils.FilesSaver<DataModel.AlbumDataStructure>.SaveData(Utils.LifeMapManager.GetInstance().ListOfAllAlbums, Utils.LifeMapManager.GetInstance().SelectedLifeMap.Id + Utils.Constants.NamingListAlbums);
        }
    }
}
