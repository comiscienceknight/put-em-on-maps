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
    public sealed partial class PhotoPlayPage : Common.LayoutAwarePage
    {
        DispatcherTimer dispatcherTimer;
        DispatcherTimer dispatcherTimer2;
        ObservableCollection<Pages.PhotoDataStructure> listSmallPhoto = new ObservableCollection<Pages.PhotoDataStructure>();
        ObservableCollection<Pages.PhotoDataStructure> ListPhoto = new ObservableCollection<Pages.PhotoDataStructure>();

        public PhotoPlayPage()
        {
            this.InitializeComponent();
            this.Loaded += PhotoPlayPage_Loaded;
        }

        void PhotoPlayPage_Loaded(object sender, RoutedEventArgs e)
        {
            //throw new NotImplementedException();
            if (parameter.selectedIndex > 0 && flipView.ItemsSource != null && ListPhoto.Count - 1 >= parameter.selectedIndex)
                flipView.SelectedItem = ListPhoto[parameter.selectedIndex];
        }

        FromExpositionToDiaporama parameter = new FromExpositionToDiaporama();

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                parameter = e.Parameter as FromExpositionToDiaporama;
                ListPhoto = new ObservableCollection<Pages.PhotoDataStructure>();
                listSmallPhoto = parameter.ListPhoto;
                foreach (var row in listSmallPhoto)
                {
                    try
                    {
                        StorageFile file = await Windows.Storage.StorageFile.GetFileFromPathAsync(row.PhotoData.ImagePath);
                        StorageItemThumbnail fileThumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, (uint)Constants.ScreenHeight, ThumbnailOptions.UseCurrentScale);
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(fileThumbnail);

                        ListPhoto.Add(new Pages.PhotoDataStructure()
                        {
                            PhotoData = row.PhotoData,
                            Image = bitmapImage
                        });
                    }
                    catch
                    { }
                }
                flipView.ItemsSource = ListPhoto;
                if (listSmallPhoto.Count > 0)
                    pageTitle.Text = parameter.albumName;
                dispatcherTimer2 = new DispatcherTimer();
                dispatcherTimer2.Tick += dispatcherTimer2_Tick;
                dispatcherTimer2.Interval = new TimeSpan(0, 0, 0, 0, 2);
                dispatcherTimer2.Start();
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "PhotoPlayPage - OnNavigatedTo"); }
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Assign a collection of bindable groups to this.DefaultViewModel["Groups"]
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        void dispatcherTimer2_Tick(object sender, object e)
        {
            try
            {
                if (parameter.selectedIndex > 0 && parameter.selectedIndex < ListPhoto.Count)
                    flipView.SelectedItem = ListPhoto[parameter.selectedIndex];
                else
                    flipView.SelectedItem = ListPhoto[0];
                dispatcherTimer2.Stop();
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "PhotoPlayPage - OnNavigatedTo"); }
        }

        private void Button_AutoPlay_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += dispatcherTimer_Tick;
                dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
                dispatcherTimer.Start();
                BottomAppBar.IsOpen = false;
                TopAppBar.IsOpen = false;
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "PhotoPlayPage - Button_AutoPlay_Click_1"); }
        }

        int playPhotosIndex = 0;

        void dispatcherTimer_Tick(object sender, object e)
        {
            try
            {
                if (playPhotosIndex == ListPhoto.Count)
                    playPhotosIndex = 0;
                flipView.SelectedItem = ListPhoto[playPhotosIndex];
                playPhotosIndex++;
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "PhotoPlayPage - dispatcherTimer_Tick"); }
        }

        private void Button_PlayByClick_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dispatcherTimer != null) dispatcherTimer.Stop();
                if (dispatcherTimer2 != null) dispatcherTimer2.Stop();
                TopAppBar.IsOpen = false;
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "PhotoPlayPage - Button_PlayByClick_Click_1"); }
        }

        private void GoBackClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dispatcherTimer != null) dispatcherTimer.Stop();
                if (dispatcherTimer2 != null) dispatcherTimer2.Stop();
                this.Frame.Navigate(typeof(PhotoExposition));
            }
            catch (Exception excep) { Constants.ShowErrorDialog(excep, "PhotoPlayPage - GoBack"); }
        }
    }
}
