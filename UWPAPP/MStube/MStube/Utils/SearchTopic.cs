using Microsoft.HockeyApp;
using MStube.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;

namespace MStube.Utils
{
    public class SearchTopic
    {
        public static async Task<List<VideoDetailItem>> SearchTopicToServer(string query)
        {
            List<VideoDetailItem> items = new List<VideoDetailItem>();
            var uri = new Uri("http://mstubedotnet.azurewebsites.net/api/SearchTopic?topic=" + query);
            HttpClient httpClient = new HttpClient();
            try
            {
                var result = await httpClient.GetStringAsync(uri);
                items = JsonConvert.DeserializeObject<List<VideoDetailItem>>(result as string);
            }
            catch (Exception error)
            {
                HockeyClient.Current.TrackException(error);
            }
            finally
            {
                httpClient.Dispose();
            }
            return items;
        }
    }
}
