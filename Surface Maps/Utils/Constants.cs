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
using Windows.ApplicationModel.Resources;

namespace Surface_Maps.Utils
{
    public class Constants
    {
        private static ResourceLoader resourceLoader;
        public static ResourceLoader ResourceLoader
        {
            get
            {
                if (resourceLoader == null)
                    resourceLoader = new ResourceLoader();
                return resourceLoader;
            }
        }

        public static ProgressRing loadingAnimationProgressRing = new ProgressRing();

        public static void StartLoadingAnimation(Grid MainGrid)
        {
            loadingAnimationProgressRing.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            loadingAnimationProgressRing.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            loadingAnimationProgressRing.Margin = new Thickness(0, 0, 0, 0);
            loadingAnimationProgressRing.IsActive = true;
            loadingAnimationProgressRing.Height = 120;
            loadingAnimationProgressRing.Width = 120;
            MainGrid.Children.Add(loadingAnimationProgressRing);
        }

        public static void StopLoadingAnimation(Grid MainGrid)
        {
            if (MainGrid.Children.Contains(loadingAnimationProgressRing))
                MainGrid.Children.Remove(loadingAnimationProgressRing);
        }

        public static double ScreenHeight = 0;
        public static double HalfScreenHeight = 0;
        public static double ScreenWidth = 0;

        public async static void ShowErrorDialog(Exception e, string erroroccuredfunction)
        {
            MessageDialog dialog = new MessageDialog(e.Message + "\n\r" + Constants.ResourceLoader.GetString("ifwantfeedback"),
                                                     Constants.ResourceLoader.GetString("wehaveaproblem"));
            dialog.Commands.Add(new UICommand(Constants.ResourceLoader.GetString("yesfeedback"), async p =>
            {
                var mailto = new Uri("mailto:?to=comiscience@hotmail.fr&subject=From Metro App, i got a error!&body=" + erroroccuredfunction + "\n\r" + e.Message + "\n\r" + Constants.ResourceLoader.GetString("ifwantfeedback") + "\n\r  Send from my Windows RT device");
                await Windows.System.Launcher.LaunchUriAsync(mailto);
            }));

            dialog.Commands.Add(new UICommand(Constants.ResourceLoader.GetString("ConfirmationForDeletePushpinNo")));
            await dialog.ShowAsync();
        }

        public async static void ShowWarningDialog(string e)
        {
            MessageDialog dialog = new MessageDialog(e, Constants.ResourceLoader.GetString("wehaveaproblem"));
            await dialog.ShowAsync();
        }

        public static bool IsGeolized = false;

        public static string NamingListAlbums = "ListAlbums";
        public static string NamingListLifeMaps = "ListLifeMaps";
        public static string NamingListPushpin = "ListPushpins";
        public static string NamingListPhotos = "ListPhotos";
    }

    public class GroupInfoList<T> : ObservableCollection<object>
    {
        public string Key { get; set; }


        public new IEnumerator<object> GetEnumerator()
        {
            return (System.Collections.Generic.IEnumerator<object>)base.GetEnumerator();
        }
    }
}
