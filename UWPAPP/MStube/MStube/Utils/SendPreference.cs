using Microsoft.HockeyApp;
using MStube.Items;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace MStube.Utils
{
    public class SendPreference
    {
        public static async void SendPreferenceToServer(long user_id, long item_id, int score)
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Prefrence preference = new Prefrence
            {
                user_id = user_id,
                item_id = item_id,
                score = score,
                timestamp = unixTimestamp
            };
            var uri = new Uri("http://mstubedotnet.azurewebsites.net/api/Preference");
            string uploadPerference = JsonConvert.SerializeObject(preference);
            Debug.WriteLine(uploadPerference);
            HttpClient httpClient = new HttpClient();
            try
            {
                HttpRequestMessage mSent = new HttpRequestMessage(HttpMethod.Post, uri);
                mSent.Content = new HttpStringContent(String.Format("{0}", uploadPerference),
                    Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
                HttpResponseMessage mReceived = await httpClient.SendRequestAsync(mSent,
                                                   HttpCompletionOption.ResponseContentRead);
                Debug.WriteLine(mReceived);
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
        }
    }
}
