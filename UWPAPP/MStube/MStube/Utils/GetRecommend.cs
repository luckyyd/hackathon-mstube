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
    class GetRecommend
    {
        public static async Task<List<VideoDetailItem>> GetRecommendFromServer(long user_id)
        {
            List<VideoDetailItem> items = new List<VideoDetailItem>();
            var uri = new Uri("http://mstubedotnet.azurewebsites.net/api/Candidates?user_id=" + user_id.ToString() + "&t=" + new Random().Next(1, 1000).ToString());
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
