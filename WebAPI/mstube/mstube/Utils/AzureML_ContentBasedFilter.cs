using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace mstube.Utils
{
    public class AzureML_ContentBasedFilter
    {
        public static async Task<string> SendPOSTRequest(long user_id, long item_id, int ratings)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"user_id", "item_id", "ratings"},
                                Values = new string[,] {  { user_id.ToString(), item_id.ToString(), ratings.ToString() },  }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };
                string apiKey = Properties.Settings.Default.AzureMLContentBasedApiKey;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri(Properties.Settings.Default.AzureMLContentBasedUri);
                HttpResponseMessage response = await client.PostAsJsonAsync("", scoreRequest);

                string result = "";
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Result: {0}", result);
                }
                else
                {
                    Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

                    Console.WriteLine(response.Headers.ToString());

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseContent);
                }
                return result;
            }

        }
    }
}