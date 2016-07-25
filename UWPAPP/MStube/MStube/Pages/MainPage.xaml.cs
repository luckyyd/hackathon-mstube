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
        private List<VideoViewModel> listOfVideoBrief = new List<VideoViewModel>();

        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeValues();
        }

        public void InitializeValues()
        {
            Utils.DeviceInfo device = Utils.DeviceInfo.Instance;
            int user_id = Task.Run(() => GetUserId(device)).Result;
            List<VideoDetailItem> items = Task.Run(() => GetVideoJson(user_id)).Result;
            foreach (VideoDetailItem item in items)
            {
                listOfVideoBrief.Add(new VideoViewModel
                {
                    Id = item.item_id,
                    Title = item.title,
                    ImageSourceUri = item.image_src,
                    VideoSourceUri = item.video_src,
                    Description = item.description,
                    FullDescription = item.full_description
                });
            }
            VideoBriefList.ItemsSource = listOfVideoBrief;
        }

        private async Task<int> GetUserId(Utils.DeviceInfo device)
        {
            HttpClient httpClient = new HttpClient();
            var uuid = device.Id;
            var uri = new Uri("http://mstubedotnet.azurewebsites.net/api/userid?uuid=" + uuid);
            int user_id = 0;
            try
            {
                user_id = Int32.Parse(await httpClient.GetStringAsync(uri));
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
            }
            finally
            {
                httpClient.Dispose();
            }
            return user_id;
        }
        private async Task<List<VideoDetailItem>> GetVideoJson(int user_id)
        {
            List<VideoDetailItem> items = new List<VideoDetailItem>();
            HttpClient httpClient = new HttpClient();
            var uri = new Uri("http://mstubedotnet.azurewebsites.net/api/Candidates?user_id=" + user_id.ToString());
            try
            {
                var result = await httpClient.GetStringAsync(uri);
                Debug.WriteLine("User id: " + user_id);
                Debug.WriteLine(result);
                items = JsonConvert.DeserializeObject<List<VideoDetailItem>>(result as string);
            }
            catch (Exception error)
            {
                Debug.WriteLine(error);
            }
            finally
            {
                httpClient.Dispose();
            }
            return items;
        }

        private void ItemClicked(object sender, ItemClickEventArgs e)
        {
            this.Frame.Navigate(typeof(VideoPage), e.ClickedItem);
        }
    }
}
