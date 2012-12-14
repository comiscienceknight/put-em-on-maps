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
    public sealed partial class PreMainPage : Surface_Maps.Common.LayoutAwarePage
    {
        public PreMainPage()
        {
            this.InitializeComponent();
            this.Loaded += PreMainPage_Loaded;
        }

        async void PreMainPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Utils.Constants.ScreenHeight = this.ActualHeight;
                Utils.Constants.HalfScreenHeight = (Utils.Constants.ScreenHeight - 230) / 2;
                Utils.Constants.ScreenWidth = this.ActualWidth;
                if (Data.LifeMapMgr != null)
                    await Data.LifeMapMgr.InitializeLifeMapManager();
                itemGridView.ItemsSource = Data.LifeMapMgr.LifeMaps;
                itemGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                if (Data.LifeMapMgr.LifeMaps.Count == 1)
                    itemGridView.SelectedIndex = 0;
                if (Data.LifeMapMgr.LifeMaps.Count <= 1)
                    BottomAppBar.IsOpen = true;
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "PreMainPage - PreMainPage_Loaded"); }
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
    }
}
