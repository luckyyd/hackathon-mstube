﻿using System;
using System.Collections.Generic;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Threading.Tasks;
using MStube.Items;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MStube
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<VideoBrief> listOfVideoBrief = new List<VideoBrief>();
        static List<VideoItem> LoadVideoItem() {
            string path = @"json/shows.json";
            string text = "";
            List<VideoItem> items = new List<VideoItem>();
            try
            {
                if (File.Exists(path))
                {
                    text = System.IO.File.ReadAllText(path);
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
