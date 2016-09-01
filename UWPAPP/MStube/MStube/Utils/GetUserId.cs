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
    class GetUserId
    {
        public static async Task<int> GetUserIdFromServer(DeviceInfo device)
        {
            var uuid = device.Id;
            var uri = new Uri("http://mstubedotnet.azurewebsites.net/api/userid?uuid=" + uuid);
            HttpClient httpClient = new HttpClient();
            int user_id = 0;
            try
            {
                user_id = Int32.Parse(await httpClient.GetStringAsync(uri));
                System.Diagnostics.Debug.WriteLine(user_id);
            }
            catch (Exception error)
            {
                HockeyClient.Current.TrackException(error);
            }
            finally
            {
                httpClient.Dispose();
            }
            HockeyClient.Current.TrackEvent("User: " + user_id.ToString());
            return user_id;
        }
    }
}
