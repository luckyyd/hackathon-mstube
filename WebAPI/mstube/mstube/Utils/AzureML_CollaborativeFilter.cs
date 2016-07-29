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
    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }

    public class AzureML_CollaborativeFilter
    {
        public static async Task<string> SendPOSTRequest(long user_id)
        {
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {

                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "ScoringInput",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"user_id", "item_id", "ratings"},
                                Values = new string[,] {  { user_id.ToString(), "0", "0" } }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };
                const string apiKey = "wDJrd0QXCPH7bo6r0a+3zKjdSpnvMxw8LZp0wTMvdDVeslUmdDohgbwylT+7oOQLQBx/TDe8K2O8ujGf2fz8sw==";
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                client.BaseAddress = new Uri("https://asiasoutheast.services.azureml.net/workspaces/0cd16c9abdb94e249ae6f6fb27b76402/services/cc0f591208854390b93d137a1098d034/execute?api-version=2.0&details=true");

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