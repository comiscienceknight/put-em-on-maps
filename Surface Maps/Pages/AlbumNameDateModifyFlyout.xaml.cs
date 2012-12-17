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
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Surface_Maps.Pages
{
    public sealed partial class AlbumNameDateModifyFlyout : Common.LayoutAwarePage
    {
        // The guidelines recommend using 100px offset for the content animation.
        const int ContentAnimationOffset = 100;
        TextBlock albumNameTextBlock;
        DataModel.AlbumDataStructure albumInfo;
        public AlbumNameDateModifyFlyout(TextBlock albumNameTextBlock, DataModel.AlbumDataStructure albumInfo)
        {
            this.InitializeComponent();

            FlyoutContent.Transitions = new TransitionCollection();
            FlyoutContent.Transitions.Add(new EntranceThemeTransition()
            {
                FromHorizontalOffset = (SettingsPane.Edge == SettingsEdgeLocation.Right) ? ContentAnimationOffset : (ContentAnimationOffset * -1)
            });

            this.albumNameTextBlock = albumNameTextBlock;
            TextBox_NeedChangedAlbumName.Text = albumNameTextBlock.Text;
            this.albumInfo = albumInfo;
            InitializeDateComboBoxes();
            InitializeDateComboBoxesValue();
        }

        #region InitializeDateComboBoxes
        private void InitializeDateComboBoxes()
        {
            InitializeYearComboBox();
            InitializeMonthComboBox();
            InitializeDayComboBox();
        }

        private void InitializeDayComboBox()
        {
            for (int i = 1; i <= 31; i++)
            {
                ComboBox_Day.Items.Insert(0, i.ToString());
            }
        }

        private void InitializeDayComboBox(int days)
        {
            for (int i = 1; i <= days; i++)
            {
                ComboBox_Day.Items.Insert(0, i.ToString());
            }
        }

        private void InitializeMonthComboBox()
        {
            for (int i = 1; i <= 12; i++)
            {
                ComboBox_Month.Items.Insert(0, i.ToString());
            }
        }

        private void InitializeYearComboBox()
        {
            for (int i = 1984; i <= DateTime.Now.Year + 1; i++)
            {
                ComboBox_Year.Items.Insert(0, i.ToString());
            }
        }

        private void InitializeDateComboBoxesValue()
        {
            if (albumInfo.AlbumName != "")
            {
                ComboBox_Year.SelectedIndex = ComboBox_Year.Items.Count - (albumInfo.Date.Year - 1983);
                ComboBox_Month.SelectedIndex = 12 - albumInfo.Date.Month;
                ComboBox_Day.SelectedIndex = 31 - albumInfo.Date.Day;
            }
            else
            {
                ComboBox_Day.SelectedIndex = 31 - DateTime.Now.Day;
                ComboBox_Month.SelectedIndex = 12 - DateTime.Now.Month;
                ComboBox_Year.SelectedIndex = ComboBox_Year.Items.Count - DateTime.Now.Year + 1983;
            }
        }
        #endregion

        private void MySettingsBackClicked(object sender, RoutedEventArgs e)
        {
            // First close our Flyout.
            Popup parent = this.Parent as Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }
        }

        private void Button_ChangeAlbumName_Click(object sender, RoutedEventArgs e)
        {
            albumNameTextBlock.Text = TextBox_NeedChangedAlbumName.Text;
            albumInfo.AlbumName = TextBox_NeedChangedAlbumName.Text;
            int days = 1;
            if(Convert.ToInt32(ComboBox_Day.SelectedValue) > DateTime.DaysInMonth(Convert.ToInt32(ComboBox_Year.SelectedValue), Convert.ToInt32(ComboBox_Month.SelectedValue)))
                days = DateTime.DaysInMonth(Convert.ToInt32(ComboBox_Year.SelectedValue), Convert.ToInt32(ComboBox_Month.SelectedValue));
            else
                days = Convert.ToInt32(ComboBox_Day.SelectedValue);
            albumInfo.Date = new DateTime(Convert.ToInt32(ComboBox_Year.SelectedValue), Convert.ToInt32(ComboBox_Month.SelectedValue), days);

            Popup parent = this.Parent as Popup;
            if (parent != null)
            {
                parent.IsOpen = false;
            }
        }

        private void ComboBox_Month_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_Year != null && ComboBox_Year.SelectedValue != null && ComboBox_Year.SelectedValue.ToString() != "" &&
                ComboBox_Month != null && ComboBox_Month.SelectedValue != null && ComboBox_Month.SelectedValue.ToString() != "")
            {
                ComboBox_Day.Items.Clear();
                InitializeDayComboBox(DateTime.DaysInMonth(Convert.ToInt32(ComboBox_Year.SelectedValue), Convert.ToInt32(ComboBox_Month.SelectedValue)));
                if (ComboBox_Day != null && ComboBox_Day.Items.Count > 0)
                {
                    ComboBox_Day.SelectedIndex = DateTime.Now.Day - 1;
                }
            }
        }

        private void ComboBox_Year_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_Year != null && ComboBox_Year.SelectedValue != null && ComboBox_Year.SelectedValue.ToString() != "" &&
                ComboBox_Month != null && ComboBox_Month.SelectedValue != null && ComboBox_Month.SelectedValue.ToString() != "")
            {
                ComboBox_Day.Items.Clear();
                InitializeDayComboBox(DateTime.DaysInMonth(Convert.ToInt32(ComboBox_Year.SelectedValue), Convert.ToInt32(ComboBox_Month.SelectedValue)));
                if (ComboBox_Day != null && ComboBox_Day.Items.Count > 0)
                {
                    ComboBox_Day.SelectedIndex = DateTime.Now.Day - 1;
                }
            }
        }
    }
}
