using Microsoft.HockeyApp;
using MStube.ViewModels;
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
    class GetTopicList
    {
        public static async Task<List<TopicViewModel>> GetTopicListFromServer()
        {
            List<TopicViewModel> topicList = new List<TopicViewModel>();
            HttpClient httpClient = new HttpClient();
            // Cache-Control: private, to avoid load cache.
            var uri = new Uri("http://mstubedotnet.azurewebsites.net/api/ListTopic");
            try
            {
                var result = await httpClient.GetStringAsync(uri);
                //Debug.WriteLine("User id: " + user_id);
                topicList = JsonConvert.DeserializeObject<List<TopicViewModel>>(result as string);
            }
            catch (Exception error)
            {
                HockeyClient.Current.TrackException(error);
            }
            finally
            {
                httpClient.Dispose();
            }
            return topicList;
        }
    }
}
