using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using MStube.Items;
using MStube.ViewModels;
using Newtonsoft.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MStube
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private List<ShowViewModel> listOfVideoBrief = new List<ShowViewModel>();
        private int user_id;
        private string shows_json;

        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeValues();
        }

        public void InitializeValues() {
            int user_id = Task.Run(()=>GetUserId()).Result;
            List<ShowItem> items = Task.Run(() => GetShowJson(user_id)).Result;
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
        private async Task<int> GetUserId()
        {
            HttpClient httpClient = new HttpClient();
            var uri = new Uri("http://mstubedotnet.azurewebsites.net/api/userid?uuid=1234567890abcd");
            int user_id = 0;
            try
            {
                user_id = Int32.Parse(await httpClient.GetStringAsync(uri));
            }
            catch (Exception)
            {
                // Error
            }
            finally
            {
                httpClient.Dispose();
            }
            return user_id;
        }
        private async Task<List<ShowItem>> GetShowJson(int user_id)
        {
            List<ShowItem> items = new List<ShowItem>();
            HttpClient httpClient = new HttpClient();
            var uri = new Uri("http://mstubedotnet.azurewebsites.net/api/Candidates?user_id=" + user_id.ToString());
            try
            {
                var result = await httpClient.GetStringAsync(uri);
                items = JsonConvert.DeserializeObject<List<ShowItem>>(result as string);
            }
            catch (Exception)
            {
                // Error
            }
            finally
            {
                httpClient.Dispose();
            }
            return items;
        }
    }
}
