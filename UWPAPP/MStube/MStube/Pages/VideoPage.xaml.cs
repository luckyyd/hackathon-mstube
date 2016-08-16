using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MStube.ViewModels;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MStube
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPage : Page
    {
        public VideoViewModel VideoDetail { get; set; }
        public long user_id { get; set; }

        public VideoPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var video = e.Parameter as VideoViewModel;
            VideoDetail = video;

            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CanGoBack)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
            else
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }

            RatingBar.GetRatingBarValue += new RatingBar.RatingBarDelegate(RatingBar_GetRatingBarValue);
        }

        private void RatingBar_GetRatingBarValue(int RatingValue)
        {
            long user_id = VideoDetail.user_id;
            long item_id = VideoDetail.item_id;
            Task.Run(() => Utils.SendPreference.SendPreferenceToServer(user_id, item_id, RatingValue));
            //Debug.WriteLine("Send Preference" + user_id.ToString() + item_id.ToString() + RatingValue.ToString());
        }

    }
}
