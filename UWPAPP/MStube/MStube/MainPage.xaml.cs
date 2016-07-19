using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Threading.Tasks;
using MStube.Items;
using MStube.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MStube
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<ShowViewModel> listOfVideoBrief = new List<ShowViewModel>();
        static List<ShowItem> LoadShowItem() {
            string path = @"json/shows.json";
            string text = "";
            List<ShowItem> items = new List<ShowItem>();
            try
            {
                if (File.Exists(path))
                {
                    text = File.ReadAllText(path);
                    items = JsonConvert.DeserializeObject<List<ShowItem>>(text);
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
            this.GetUserId();
            this.InitializeValues();
        }
        private void GetUserId()
        {

        }
        public void InitializeValues() { 
            var t = Task.Run(() => LoadShowItem());
            t.Wait();
            List<ShowItem> items = t.Result;
            foreach (ShowItem item in items)
            {
                listOfVideoBrief.Add(new ShowViewModel { Id = item.id, ImageSourceUri = item.image_src, VideoTitle = item.title, Description = item.description });
            }
            VideoBriefList.ItemsSource = listOfVideoBrief;
        }
        private void hyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton button = sender as HyperlinkButton;
            var id = button.Tag;
            this.Frame.Navigate(typeof(VideoPage), id);
        }
    }
}
