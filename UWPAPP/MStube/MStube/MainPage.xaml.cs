using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Threading.Tasks;

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
        public class VideoItem
        {
            public int id;
            public string url;
            public string description;
            public string image_src;
            public string title;
        }
        private List<VideoBrief> listOfVideoBrief = new List<VideoBrief>();
        static List<VideoItem> LoadVideoItem() {
            string path = @"json/shows.json";
            string text = "";
            List<VideoItem> items = new List<VideoItem>();
            try
            {
                if (File.Exists(path))
                {
                    Debug.WriteLine("File exists.");
                    text = System.IO.File.ReadAllText(path);
                    Debug.WriteLine(text);
                    items = JsonConvert.DeserializeObject<List<VideoItem>>(text);
                }
                else
                {
                    Debug.WriteLine("File not exists.");
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("Exception thrown.");
            }
            return items;
        }
        public MainPage()
        {
            this.InitializeComponent();
            Debug.WriteLine("Test file");
            // Read the JSON file.
            var t = Task.Run(() => LoadVideoItem());
            t.Wait();
            List<VideoItem> items = t.Result;
            foreach (VideoItem item in items)
            {
                listOfVideoBrief.Add(new VideoBrief { ImageSourceUri = item.image_src, VideoTitle = item.title, Description = item.description });
            }
            VideoBriefList.ItemsSource = listOfVideoBrief;
        }
        private void hyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoPage));
        }
    }
}
