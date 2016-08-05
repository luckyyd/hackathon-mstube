using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Web.Http;
using Microsoft.HockeyApp;
using Newtonsoft.Json.Linq;

namespace MStube
{
    public sealed partial class WebPage : Page
    {
        public VideoViewModel WebDetail { get; set; }
        public long user_id { get; set; }
        public WebPage()
        {
            this.InitializeComponent();
        }

        public async void InitializeValues() {
            LoadingProgressRing.IsActive = true;
            string CurrentHtmlString = await GetOembedJson();
            mediaWebview.NavigateToString(CurrentHtmlString);
            LoadingProgressRing.IsActive = false;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var video = e.Parameter as VideoViewModel;
            Debug.WriteLine(video.Title);
            WebDetail = video;
            InitializeValues();

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
            long user_id = WebDetail.user_id;
            long item_id = WebDetail.item_id;
            Task.Run(() => Utils.SendPreference.SendPreferenceToServer(user_id, item_id, RatingValue));
            Debug.WriteLine("Send Preference" + user_id.ToString() + item_id.ToString() + RatingValue.ToString());
        }


        private async Task<string> GetOembedJson()
        {
            string link = "";
            HttpClient httpClient = new HttpClient();
            // Cache-Control: private, to avoid load cache.
            var uri = new Uri("https://vimeo.com/api/oembed.json?url=" + WebDetail.Url);
            try
            {
                string result = await httpClient.GetStringAsync(uri);
                Debug.WriteLine(result);
                JToken root = JObject.Parse(result);
                JToken html = root["html"];
                link = html.ToString();
                link = "<html><head></head><body bgcolor=\"#000000\">" + link + "</body></html>";
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
                HockeyClient.Current.TrackException(error);
            }
            finally
            {
                httpClient.Dispose();
            }
            return link;
        }

    }
}
