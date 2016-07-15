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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MStube
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public class VideoBrief
        {
            public string ImageSourceUri { get; set; }
            public string VideoTitle { get; set; }
            public string Description { get; set; }
        }
        private List<VideoBrief> listOfVideoBrief = new List<VideoBrief>();
        public MainPage()
        {
            this.InitializeComponent();
            listOfVideoBrief.Add(new VideoBrief { ImageSourceUri = "https://www.google.com.sg/images/branding/product/ico/googleg_lodp.ico", VideoTitle = "Google", Description = "Hello, Google." });
            listOfVideoBrief.Add(new VideoBrief { ImageSourceUri = "https://www.microsoft.com/favicon.ico?v2", VideoTitle = "Microsoft", Description = "Hi, Microsoft." });
            VideoBriefList.ItemsSource = listOfVideoBrief;
        }
        private void hyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoPage));
        }
    }
}
