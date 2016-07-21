using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.IO;
using StackExchange.Redis;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using Microsoft.WindowsAzure.Storage;
using System.Text;

namespace mstube.Controllers
{
    public class ApiController : Controller
    {
        // GET: Api
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult UserId(string uuid) {
            //Get user_id for uuid
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect("mstube-dotnet-id.redis.cache.windows.net,abortConnect=false,ssl=true,password=Tp/f4EEuKJWK1z7HJOvyrvZrg5IA9y4/W9BELvUPWZg=");
            IDatabase cacheid = connection.GetDatabase();

            string dbsize = cacheid.StringGet("RedisSize");
            if (dbsize == null)
            {
                dbsize = "0";
                cacheid.StringSet("RedisSize", "0");
            }
            if (uuid != null)
            {
                string id = cacheid.StringGet(uuid);
                if (id == null)
                {
                    long user_id = Convert.ToInt64(dbsize) + 1;
                    cacheid.StringSet(uuid, user_id.ToString());
                    cacheid.StringSet("RedisSize", user_id.ToString());
                    return Json(user_id, JsonRequestBehavior.AllowGet);
                }
                else {
                    return Json(Convert.ToInt64(id), JsonRequestBehavior.AllowGet);
                }
            }
            else {
                return Json("Invalid", JsonRequestBehavior.AllowGet);
            }
        }

        /*
        [HttpGet]
        public JsonResult Candidates()
        {
            //DEBUG ONLY
            //Return a random recommend list for test
            StreamReader sr = new StreamReader(Server.MapPath(@"~/App_Data/items.json"));
            var json = JsonConvert.DeserializeObject<List<Item.Item>>(sr.ReadToEnd());
            return Json(json, JsonRequestBehavior.AllowGet);
        }
        */

        [HttpGet]
        public async Task<JsonResult> Candidates(long user_id)
        {
            List<Item.Item> jsonResult = new List<Item.Item>();

            //Send POST request to Azure ML
            string result = await Utils.AzureML.SendPOSTRequest(user_id);

            dynamic jsonObj = JsonConvert.DeserializeObject(result);
            JArray values = (JArray)jsonObj.Results.output1.value.Values[0];
            List<string> val = values.ToObject<List<string>>();
            val.RemoveAt(0);

            //Append val up to 10 items
            if (val.Count < 10) {
                var max = val.Select(v => int.Parse(v)).Max();
                while (val.Count < 10) {
                    Random ran = new Random();
                    int RandKey = ran.Next(1, max);
                    val.Add(RandKey.ToString());
                }   
            }

            //Return items from db
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MstubeConnection"].ToString());

            foreach (var item_id in val) {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT * FROM Item WHERE item_id = @item_id";
                    command.Parameters.AddWithValue("@item_id", item_id);
                    try
                    {
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                jsonResult.Add(new Item.Item
                                {
                                    item_id = Convert.ToInt64(reader["item_id"]),
                                    image_src = reader["image_src"].ToString(),
                                    video_src = reader["video_src"].ToString(),
                                    title = reader["title"].ToString(),
                                    url = reader["url"].ToString(),
                                    description = reader["description"].ToString(),
                                    topic = reader["topic"].ToString(),
                                });
                            }
                        }

                    }
                    catch (SqlException)
                    {
                        // error here
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ListTopic() {
            List<Item.Topic> jsonResult = new List<Item.Topic>();
            //Return list topic from db
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MstubeConnection"].ToString());
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT DISTINCT topic FROM Item";
                try
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            jsonResult.Add(new Item.Topic
                            {
                                topic = reader["topic"].ToString()
                            });
                        }
                    }

                }
                catch (SqlException)
                {
                    // error here
                }
                finally
                {
                    connection.Close();
                }
            }
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SearchTopic(string topic) {
            List<Item.Item> jsonResult = new List<Item.Item>();
            //Return search topic from db
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MstubeConnection"].ToString());
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT * FROM Item WHERE topic = @topic";
                command.Parameters.AddWithValue("@topic", topic);
                try
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            jsonResult.Add(new Item.Item {
                                item_id = Convert.ToInt64(reader["item_id"]),
                                image_src = reader["image_src"].ToString(),
                                video_src = reader["video_src"].ToString(),
                                title = reader["title"].ToString(),
                                url = reader["url"].ToString(),
                                description = reader["description"].ToString(),
                                topic = reader["topic"].ToString(),
                            });
                        }
                    }

                }
                catch (SqlException)
                {
                    // error here
                }
                finally
                {
                    connection.Close();
                }
            }
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Preference(Preference.Preference pre)
        {
            //Append preference data to Azure Blob
            const string StorageAccountName = "mstubeblob";
            const string StorageAccountKey = "nG0MPtLcKCMPj15uKalobeFWvfLNljGen/K21qcbLdxrPtdW/UWViA4xuqEJPvb9O+FoAd7BIXgFxLSluWAM5g==";
            const string storageContainerName = "mstube-container";
            const string inputBlobName = "TrainingInputdatablob.csv";
            
            string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);
            var blobClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(storageContainerName);
            container.CreateIfNotExists();
            var blob = container.GetAppendBlobReference(inputBlobName);
            StringBuilder csvData = new StringBuilder();
            csvData.AppendLine(pre.user_id + "," + pre.item_id + "," + pre.score + "," + pre.timestamp);
            string dataToUpload = csvData.ToString();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(dataToUpload)))
            {
                blob.AppendFromStream(ms);
            }

            return Json(pre);
        }
    }
}