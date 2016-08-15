using Microsoft.HockeyApp;
using MStube.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;

namespace MStube.Utils
{
    public class GetQuery
    {
        public static async Task<List<VideoDetailItem>> GetQueryToServer(string query)
        {
            List<VideoDetailItem> items = new List<VideoDetailItem>();
            var uri = new Uri("http://mstubedotnet.azurewebsites.net/api/Search?query=" + query);
            Debug.WriteLine(query);
            HttpClient httpClient = new HttpClient();
            try
            {
                var result = await httpClient.GetStringAsync(uri);
                Debug.WriteLine(result);
                items = JsonConvert.DeserializeObject<List<VideoDetailItem>>(result as string);
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
            return items;
        }
    }
}
