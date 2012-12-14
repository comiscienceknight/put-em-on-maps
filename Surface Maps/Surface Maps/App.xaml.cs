using Surface_Maps.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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
using Windows.System.Threading;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml.Media.Animation;
using Surface_Maps.SettingCommands;

// The Grid App template is documented at http://go.microsoft.com/fwlink/?LinkId=234226

namespace Surface_Maps
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active

            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                SettingsPane.GetForCurrentView().CommandsRequested += onCommandsRequested;

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(PreMainPage)))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        void onCommandsRequested(SettingsPane settingsPane, SettingsPaneCommandsRequestedEventArgs eventArgs)
        {
            UICommandInvokedHandler handler = new UICommandInvokedHandler(onSettingsCommand);

            SettingsCommand languageCommand = new SettingsCommand("languagePage", Utils.Constants.ResourceLoader.GetString("Languages"), handler);
            eventArgs.Request.ApplicationCommands.Add(languageCommand);

            SettingsCommand helpCommand = new SettingsCommand("helpPage", Utils.Constants.ResourceLoader.GetString("Help"), handler);
            eventArgs.Request.ApplicationCommands.Add(helpCommand);

            SettingsCommand feedbackCommand = new SettingsCommand("feedbackpage", Utils.Constants.ResourceLoader.GetString("AboutUsAndFeedbak"), handler);
            eventArgs.Request.ApplicationCommands.Add(feedbackCommand);

            SettingsCommand tellfriendCommand = new SettingsCommand("tellfriendpage", Utils.Constants.ResourceLoader.GetString("TellToFriends"), handler);
            eventArgs.Request.ApplicationCommands.Add(tellfriendCommand);

            SettingsCommand privacypolicyCommand = new SettingsCommand("privacypolicy", Utils.Constants.ResourceLoader.GetString("PrivacyPolicy"), handler);
            eventArgs.Request.ApplicationCommands.Add(privacypolicyCommand);
        }

        void onSettingsCommand(IUICommand command)
        {
            SettingsCommand settingsCommand = (SettingsCommand)command;
            feedbackSettingCommand(settingsCommand);
            tellfriendpageSettingCommand(settingsCommand);
            languagesSettingCommand(settingsCommand);
            helpSettingCommand(settingsCommand);
            privacypolicySettingCommand(settingsCommand);
        }

        private async void privacypolicySettingCommand(SettingsCommand settingsCommand)
        {
            try
            {
                if (settingsCommand.Id.ToString() == "privacypolicy")
                {
                    var mailto = new Uri("http://www.comiscience.info/privacy/pushthemonmapsprivacypolicy.txt");
                    await Windows.System.Launcher.LaunchUriAsync(mailto);
                }
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "App - privacypolicySettingCommand"); }
        }

        private void helpSettingCommand(SettingsCommand settingsCommand)
        {
            try
            {
                if (settingsCommand.Id.ToString() == "helpPage")
                {
                    createPopupWindowContainsFlyout("Helps");
                }
            }
            catch (Exception excep) { Utils.Constants.ShowErrorDialog(excep, "App - helpSettingCommand"); }
        }

        private void languagesSettingCommand(SettingsCommand settingsCommand)
        {
            if (settingsCommand.Id.ToString() == "languagePage")
            {
                createPopupWindowContainsFlyout("Languages");
            }
        }

        async void tellfriendpageSettingCommand(SettingsCommand settingsCommand)
        {
            if (settingsCommand.Id.ToString() == "tellfriendpage")
            {
                var mailto = new Uri("mailto:?to=your_friend@example.com&subject=Check out this Win8 metro application - Photo Map-Timeline Planet&body=Hello my dear fiend, Have you tried Photo Map-Timeline Planet. Unlimited on Windows 8 Metro \n\n It's the one of best photo manager ever.. I bet it make your life more happy!");
                await Windows.System.Launcher.LaunchUriAsync(mailto);
            }
        }

        private Popup settingsPopup;
        private double settingsWidth = 500;
        private Rect windowBounds;

        void feedbackSettingCommand(SettingsCommand settingsCommand)
        {
            windowBounds = Window.Current.Bounds;
            if (settingsCommand.Id.ToString() == "feedbackpage")
            {
                createPopupWindowContainsFlyout("About Us and Feedbak");
            }
        }

        private void createPopupWindowContainsFlyout(string option)
        {
            createSettingsPopup();
            addProperAnimationForPanel();
            if (option == "About Us and Feedbak")
            {
                FeedbackSettingsFlyout mypane = new FeedbackSettingsFlyout();
                settingsWidth = 400;
                mypane.Width = settingsWidth;
                mypane.Height = windowBounds.Height;
                settingsPopup.Child = mypane;
            }
            else if (option == "Languages")
            {
                LanguagesSettingFlyout mypane = new LanguagesSettingFlyout();
                settingsWidth = 400;
                mypane.Width = settingsWidth;
                mypane.Height = windowBounds.Height;
                settingsPopup.Child = mypane;
            }
            else if (option == "Helps")
            {
                HelpSettingsFlyout mypane = new HelpSettingsFlyout();
                settingsWidth = Utils.Constants.ScreenWidth;
                mypane.Width = settingsWidth;
                mypane.Height = windowBounds.Height;
                settingsPopup.Child = mypane;
            }
            defineLocationOfOurPopup();
        }

        private void createSettingsPopup()
        {
            settingsPopup = new Popup();
            settingsPopup.Closed += OnPopupClosed;
            Window.Current.Activated += OnWindowActivated;
            settingsPopup.IsLightDismissEnabled = true;
            settingsPopup.Width = settingsWidth;
            settingsPopup.Height = windowBounds.Height;
        }

        private void addProperAnimationForPanel()
        {
            settingsPopup.ChildTransitions = new TransitionCollection();
            settingsPopup.ChildTransitions.Add(new PaneThemeTransition()
            {
                Edge = (SettingsPane.Edge == SettingsEdgeLocation.Right) ?
                       EdgeTransitionLocation.Right :
                       EdgeTransitionLocation.Left
            });
        }

        private void defineLocationOfOurPopup()
        {
            settingsPopup.SetValue(Canvas.LeftProperty, SettingsPane.Edge == SettingsEdgeLocation.Right ? (windowBounds.Width - settingsWidth) : 0);
            settingsPopup.SetValue(Canvas.TopProperty, 0);
            settingsPopup.IsOpen = true;
        }

        private void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                settingsPopup.IsOpen = false;
            }
        }

        void OnPopupClosed(object sender, object e)
        {
            Window.Current.Activated -= OnWindowActivated;
        }
    }
}
