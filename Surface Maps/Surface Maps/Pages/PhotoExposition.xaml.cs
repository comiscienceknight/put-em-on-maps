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
using Surface_Maps.Utils;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Surface_Maps.Pages
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class PhotoExposition : Common.LayoutAwarePage, INotifyPropertyChanged
    {
        private ObservableCollection<Photos> listPhoto = new ObservableCollection<Photos>();
        public ObservableCollection<Photos> ListPhoto
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

        FromMapToCollectioin navigateParameter;

        DataModel.AlbumDataStructure albumInfo = new DataModel.AlbumDataStructure();

        public PhotoExposition()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            DataContext = this;
            this.Loaded += PhotoExposition_Loaded;
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Assign a collection of bindable groups to this.DefaultViewModel["Groups"]
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        async void PhotoExposition_Loaded(object sender, RoutedEventArgs e)
        {
            if (navigateParameter.pushpin != null && navigateParameter.pushpin.BackgroundPhotoPath != "")
                fillBackgroundImage(navigateParameter.pushpin.BackgroundPhotoPath);
            albumInfo = navigateParameter.slectedAlbum;
            pageTitle.Text = albumInfo.AlbumName;
            await LoadData();
            VariableGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Constants.StopLoadingAnimation(MainGrid);

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                VariableGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                Constants.StartLoadingAnimation(MainGrid);
                if (e.Parameter != null)
                {
                    navigateParameter = e.Parameter as FromMapToCollectioin;
                }
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "PhotoExposition - OnNavigatedTo"); }
        }

        public async Task LoadData()
        {
            if (albumInfo != null && albumInfo.AlbumName != "")
            {
                ListPhoto = new ObservableCollection<Photos>();
                GC.Collect();
                itemsViewSource.Source = ListPhoto;
                var result = from datas in Utils.LifeMapManager.GetInstance().ListOfAllPhotos
                             where datas.AlbumId == albumInfo.Id &&
                                   datas.Latitude == albumInfo.Latitude &&
                                   datas.Longitude == albumInfo.Longitude
                             select datas;
                await loadPhotos(result);
            }
        }

        private async Task loadPhotos(IEnumerable<DataModel.PhotoDataStructure> result)
        {
            bool ifexception = false;
            foreach (var row in result)
            {
                try
                {
                    StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(row.ImagePath);
                    if (file != null)
                    {
                        StorageItemThumbnail fileThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 400);
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(fileThumbnail);

                        int heightForPhoto = setPhotoHeight(bitmapImage);
                        ListPhoto.Add(new Photos()
                        {
                            photoDataStructure = new PhotoDataStructure()
                            {
                                PhotoData = row as DataModel.PhotoDataStructure,
                                Image = bitmapImage
                            },
                            Width = Convert.ToInt32((heightForPhoto * (Convert.ToDouble(bitmapImage.PixelWidth) / Convert.ToDouble(bitmapImage.PixelHeight) + 0.05))),
                            Height = heightForPhoto
                        });
                    }
                }
                catch
                {
                    ListPhoto.Add(new Photos()
                    {
                        photoDataStructure = new PhotoDataStructure()
                        {
                            PhotoData = row as DataModel.PhotoDataStructure,
                            Image = null
                        },
                        Width = 0,
                        Height = 0
                    });
                    ifexception = true;
                }
            }
            if (ifexception == true)
                Constants.ShowWarningDialog(Constants.ResourceLoader.GetString("pathfilechanged"));
        }

        private static int setPhotoHeight(BitmapImage bitmapImage)
        {
            int heightForPhoto = 0;
            if (bitmapImage.PixelHeight > (Constants.ScreenHeight - 200))
            {
                heightForPhoto = Convert.ToInt32((Constants.ScreenHeight - 200)) / 100;
            }
            else if (bitmapImage.PixelHeight < 100)
            {
                heightForPhoto = 2;
            }
            else
            {
                heightForPhoto = bitmapImage.PixelHeight / 100;
            }
            return heightForPhoto;
        }

        private void Button_PlayPhoto_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                FromExpositionToDiaporama parameter = new FromExpositionToDiaporama();
                for (int i = 0; i < ListPhoto.Count; i++)
                {
                    parameter.ListPhoto.Add(ListPhoto[i].photoDataStructure);
                }
                parameter.selectedIndex = VariableGridView.SelectedIndex;
                parameter.navigateParameter = navigateParameter;
                parameter.albumName = pageTitle.Text;
                this.Frame.Navigate(typeof(PhotoPlayPage), parameter);
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "PhotoExposition - Button_PlayPhoto_Click_1"); }
        }

        private void GoBackToAlbumCollection(object sender, RoutedEventArgs e)
        {
            try
            {
                navigateParameter.slectedAlbum = albumInfo;
                this.Frame.Navigate(typeof(AlbumCollectionView), navigateParameter);
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "PhotoExposition - GoBackToAlbumCollection"); }
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

		private void variableGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (VariableGridView.SelectedItem == null) return;
				BottomAppBar.IsOpen = false;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "PhotoExposition - variableGridView_SelectionChanged"); }
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

    public class Photos : IResizable
    {
        public Pages.PhotoDataStructure photoDataStructure;

        public string Comment
        {
            get
            {
                if (photoDataStructure != null)
                    return photoDataStructure.PhotoData.Comment;
                return "";
            }
        }
        public string ImagePath
        {
            get
            {
                if (photoDataStructure != null)
                    return photoDataStructure.PhotoData.ImagePath;
                return "";
            }
        }
        public int Width { get; set; }
        public int Height { get; set; }

        public BitmapImage Image
        {
            get
            {
                if (photoDataStructure != null)
                    return photoDataStructure.Image;
                return null;
            }
        }
    }
}
