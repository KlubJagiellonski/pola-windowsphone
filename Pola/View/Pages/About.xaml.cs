using Pola.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Pola.View.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class About : Page
    {
        #region Fields

        private NavigationHelper navigationHelper;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        #endregion

        #region Constructor

        public About()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.VersionTextBlock.Text = Package.Current.Id.Version.ToVersion().ToString();
        }

        #endregion

        #region Event handlers

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            this.SetupStatusBar();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        private async void OnFacebookTapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("fb:pages?id=1497031433925482"));
        }

        private async void OnTwitterTapped(object sender, TappedRoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://twitter.com/pola_app"));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Setups StatusBar opacity and colors.
        /// </summary>
        private void SetupStatusBar()
        {
            StatusBar statusBar = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
            statusBar.ForegroundColor = Colors.Black;
        }

        #endregion

        private void OnReportClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Report));
        }

        private void OnFeedbackClick(object sender, RoutedEventArgs e)
        {
            string version = Package.Current.Id.Version.ToVersion().ToString();
            string email = "kontakt@pola-app.pl";
            string subject = string.Format("Pola {0}, Windows Phone", version);
            string body = "";
            Uri mailto = new Uri(string.Format("mailto:?to={0}&subject={1}&body={2}", email, subject, body));
            var ignore = Launcher.LaunchUriAsync(mailto);
        }

        private void OnRateClick(object sender, RoutedEventArgs e)
        {
            string appid = Windows.ApplicationModel.Package.Current.Id.Name;
            var ignore = Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + appid));
        }
    }
}
