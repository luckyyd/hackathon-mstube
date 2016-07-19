using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MStube.Items;
using MStube.ViewModels;
using System.Threading.Tasks;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MStube
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPage : Page
    {
        private static VideoDetailItem LoadVideoDetail()
        {
            string path = @"json/videos.json";
            string text = "";
            text = File.ReadAllText(path);
            List<VideoDetailItem> items = JsonConvert.DeserializeObject<List<VideoDetailItem>>(text);
            // Here choose an example item.
            VideoDetailItem item = items[0];
            return item;
        }
        public VideoPage()
        {
            this.InitializeComponent();
            this.InitializeValues();
        }
        public void InitializeValues()
        {
            var t = Task.Run(() => LoadVideoDetail());
            t.Wait();
            // TODO: Use ViewModel to bind variables.
            VideoDetailItem item = t.Result;
            textTitle.Text = item.title;
            textDescription.Text = item.video_description;
            videoView.Source = new System.Uri(item.video_src);
        }
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var id = e.Parameter;
            Debug.WriteLine(id);
        }
    }
}
