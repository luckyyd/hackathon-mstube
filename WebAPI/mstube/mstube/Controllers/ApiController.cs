using Microsoft.WindowsAzure.Storage;
using mstube.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace mstube.Controllers
{
    public class ApiController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult UserId(string uuid)
        {
            //Get user_id for uuid
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(Properties.Settings.Default.RedisUserId);
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
                else
                {
                    return Json(Convert.ToInt64(id), JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Invalid", JsonRequestBehavior.AllowGet);
            }
        }
        private HashSet<int> GenerateRandomSet(int sum, int max)
        {
            HashSet<int> randSet = new HashSet<int>();
            while (randSet.Count < sum)
            {
                Random ran = new Random();
                int Randkey = ran.Next(1, max);
                if (!randSet.Contains(Randkey))
                {
                    randSet.Add(Randkey);
                }
            }
            return randSet;
        }
        private List<Item.Item> GetItemsFromSQLServer(List<string> choicedItems)
        {
            List<Item.Item> resultList = new List<Item.Item>();
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MstubeConnection"].ToString());
            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                string itemsSet = "";
                foreach (var item_id in choicedItems)
                {
                    itemsSet += item_id.ToString();
                    itemsSet += ',';
                }
                itemsSet = itemsSet.TrimEnd(',');
                command.CommandText = "SELECT * FROM Item WHERE item_id in (" + itemsSet + ")";

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Item.Item item = new Item.Item
                        {
                            item_id = Convert.ToInt64(reader["item_id"]),
                            image_src = reader["image_src"].ToString(),
                            video_src = reader["video_src"].ToString(),
                            title = reader["title"].ToString(),
                            url = reader["url"].ToString(),
                            description = reader["description"].ToString(),
                            topic = reader["topic"].ToString(),
                            category = reader["category"].ToString(),
                            full_description = reader["full_description"].ToString(),
                            posted_time = reader["posted_time"].ToString(),
                            views = Convert.ToInt32(reader["views"]),
                            quality = Convert.ToDouble(reader["quality"]),
                        };
                        string item_id = item.item_id.ToString();
                        resultList.Add(item);
                    }
                }
            }
            catch (SqlException err)
            {
                Debug.WriteLine(err);
            }
            finally
            {
                connection.Close();
            }
            return resultList;
        }

        private async Task<List<Item.Item>> GetPopularItemsFromSQLServer(int choices = 50, int top = 5)
        {
            List<Item.Item> resultList = new List<Item.Item>();
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MstubeConnection"].ToString());
            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT top 5 * FROM Item WHERE item_id IN (SELECT TOP 50 item_id FROM Item ORDER BY NewID() ) ORDER BY cast(views as int) DESC";
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Item.Item item = new Item.Item
                        {
                            item_id = Convert.ToInt64(reader["item_id"]),
                            image_src = reader["image_src"].ToString(),
                            video_src = reader["video_src"].ToString(),
                            title = reader["title"].ToString(),
                            url = reader["url"].ToString(),
                            description = reader["description"].ToString(),
                            topic = reader["topic"].ToString(),
                            category = reader["category"].ToString(),
                            full_description = reader["full_description"].ToString(),
                            posted_time = reader["posted_time"].ToString(),
                            views = Convert.ToInt32(reader["views"]),
                            quality = Convert.ToDouble(reader["quality"]),
                            brand = 3,
                        };
                        string item_id = item.item_id.ToString();
                        resultList.Add(item);
                    }
                }
            }
            catch (SqlException err)
            {
                Debug.WriteLine(err);
            }
            finally
            {
                connection.Close();
            }
            return resultList;
        }
        private async Task<List<string>> ItemFilter(long user_id, List<string> itemsToFilter, IDatabase cachefilter)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            List<string> result = new List<string>(itemsToFilter);
            if (result.Count > 0)
            {
                for (int i = result.Count - 1; i >= 0; --i)
                {
                    string item_id = result[i];
                    if (cachefilter.SetContains(user_id.ToString(), item_id))
                    {
                        result.Remove(item_id);
                    }
                }
            }
            Debug.WriteLine("Filter Time: {0} ms", timer.ElapsedMilliseconds);
            return result;
        }
        private void LogRecommendHistory(long user_id, List<Item.Item> items, IDatabase cachefilter)
        {
            foreach (var v in items)
            {
                cachefilter.SetAdd(user_id.ToString(), v.item_id.ToString());
            }
        }

        [HttpGet]
        public async Task<JsonResult> Candidates(long user_id)
        {
            Stopwatch timer = new Stopwatch();
            Stopwatch timerTotal = new Stopwatch();

            timerTotal.Start();

            List<Item.Item> resultList = new List<Item.Item>();
            List<string> totalCandidates = new List<string>();
            List<string> collaborativeFilteringCandidates = new List<string>();
            List<string> contentBasedCandidates = new List<string>();
            List<string> popularityCandidates = new List<string>();

            // Connect to Redis
            timer.Start();
            ConnectionMultiplexer FilterRedis = ConnectionMultiplexer.Connect(Properties.Settings.Default.RedisPostHistory);
            IDatabase cachefilter = FilterRedis.GetDatabase();
            timer.Stop();
            long timerConnectHistory = timer.ElapsedMilliseconds;

            // Run get popularity items task
            Task<List<Item.Item>> taskGetPopularityItems = Task.Run(() => GetPopularItemsFromSQLServer(50, 5));

            // Get CF result
            timer.Restart();
            string result = await AzureML_CollaborativeFilter.SendPOSTRequest(user_id);
            timer.Stop();
            long timerConnectAzureCF = timer.ElapsedMilliseconds;

            // Filter CF result;
            dynamic jsonObj = JsonConvert.DeserializeObject(result);
            JArray values = (JArray)jsonObj.Results.ScoringOutput.value.Values[0];
            collaborativeFilteringCandidates = values.ToObject<List<string>>();
            collaborativeFilteringCandidates.RemoveAt(0);
            Task<List<string>> taskFilterCFCandidates = Task.Run(() => ItemFilter(user_id, collaborativeFilteringCandidates, cachefilter));

            // Connect to Redis
            timer.Restart();
            ConnectionMultiplexer ContentBasedRedis = ConnectionMultiplexer.Connect(Properties.Settings.Default.RedisLastItem);
            IDatabase cacheid = ContentBasedRedis.GetDatabase();
            timer.Stop();
            long timerConnectRedisLastItem = timer.ElapsedMilliseconds;

            // Get last item 
            string last_item_id = cacheid.StringGet(user_id.ToString());
            Debug.WriteLine("Last item id is " + last_item_id);

            // Get Content-based result
            timer.Restart();
            if (last_item_id != null)
            {
                string contentbasedResult = await AzureML_ContentBasedFilter.SendPOSTRequest(user_id, Convert.ToInt64(last_item_id), 3);
                dynamic jsonContentbasedResultObj = JsonConvert.DeserializeObject(contentbasedResult);
                JArray valuesContentbasedResult = (JArray)jsonContentbasedResultObj.Results.output1.value.Values[0];
                contentBasedCandidates = valuesContentbasedResult.ToObject<List<string>>();
                contentBasedCandidates.RemoveAt(0);
            }
            timer.Stop();
            long timerAzureMLcontent = timer.ElapsedMilliseconds;

            // Filter Contect-based items
            timer.Restart();
            Task<List<string>> taskFilterContentBasedCandidates = Task.Run(() => ItemFilter(user_id, contentBasedCandidates, cachefilter));

            // Get task results
            contentBasedCandidates = taskFilterContentBasedCandidates.Result.Take(5).ToList();
            collaborativeFilteringCandidates = taskFilterCFCandidates.Result.Take(5).ToList();

            // Append to total candidates
            totalCandidates.AddRange(contentBasedCandidates);
            totalCandidates.AddRange(collaborativeFilteringCandidates);
            timer.Stop();
            long timerWaitingFilter = timer.ElapsedMilliseconds;

            // Get items from SQL Server
            timer.Restart();
            resultList = GetItemsFromSQLServer(totalCandidates);
            timer.Stop();
            long timerGetItemsFromSQLServer = timer.ElapsedMilliseconds;

            // Add brands
            foreach (Item.Item item in resultList)
            {
                string item_id = item.item_id.ToString();
                if (collaborativeFilteringCandidates.Contains(item_id)) { item.brand = 1; }
                else if (contentBasedCandidates.Contains(item_id)) { item.brand = 2; }
            }

            // Get distinct items
            List<Item.Item> distinctList = resultList.GroupBy(x => x.item_id).Select(g => g.First()).ToList();

            // Add popularity list
            List<Item.Item> popularityList = taskGetPopularityItems.Result;
            distinctList.AddRange(popularityList);
            distinctList = distinctList.Take(10).ToList();
            distinctList.Shuffle();

            // Post History
            Task logRecommendHistory = Task.Run(() => LogRecommendHistory(user_id, distinctList, cachefilter));

            timerTotal.Stop();
            Debug.WriteLine("Content based item count: {0}", contentBasedCandidates.Count);
            Debug.WriteLine("total candidates:         {0}", distinctList.Count);
            Debug.WriteLine("");
            Debug.WriteLine("Connect Redis Post History time: {0} ms", timerConnectHistory);
            Debug.WriteLine("Connect to Redis last item time: {0} ms", timerConnectRedisLastItem);
            Debug.WriteLine("Get CF result time:              {0} ms", timerConnectAzureCF);
            Debug.WriteLine("Get content-base result time:    {0} ms", timerAzureMLcontent);
            Debug.WriteLine("Waiting filter time:             {0} ms", timerWaitingFilter);
            Debug.WriteLine("Get items from SQL server time:  {0} ms", timerGetItemsFromSQLServer);
            Debug.WriteLine("Total time: {0} ms", timerTotal.ElapsedMilliseconds);
            return Json(distinctList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ListTopic()
        {
            List<Item.Topic> jsonResult = new List<Item.Topic>();
            //Return list topic from db
            using (SqlCommand command = new SqlCommand())
            {
                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MstubeConnection"].ToString());
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
                catch (SqlException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
                finally
                {
                    connection.Close();
                }
            }
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult SearchTopic(string topic)
        {
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
                                category = reader["topic"].ToString(),
                                full_description = reader["full_description"].ToString(),
                                posted_time = reader["posted_time"].ToString(),
                                views = Convert.ToInt32(reader["views"]),
                                quality = Convert.ToDouble(reader["quality"])
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
            const string storageContainerName = "mstube-container";
            string StorageAccountKey = Properties.Settings.Default.StorageAccountKey;
            string inputBlobName = Properties.Settings.Default.StorageInputBlobName;

            string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);
            var blobClient = CloudStorageAccount.Parse(storageConnectionString).CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(storageContainerName);
            container.CreateIfNotExists();
            var blob = container.GetAppendBlobReference(inputBlobName);
            StringBuilder csvData = new StringBuilder();
            csvData.AppendLine(pre.user_id + "," + pre.item_id + "," + pre.score + "," + pre.timestamp);
            //csvData.AppendLine("user_id,item_id,ratings,timestamp");
            string dataToUpload = csvData.ToString();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(dataToUpload)))
            {
                //blob.UploadFromStream(ms);
                blob.AppendFromStream(ms);
            }

            //Update User Profile
            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            UserProfile.Update.updateUserProfile(pre, milliseconds);

            //Append data to Redis
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(Properties.Settings.Default.RedisLastItem);
            IDatabase cacheid = connection.GetDatabase();
            cacheid.StringSet(pre.user_id.ToString(), pre.item_id.ToString());

            return Json(pre);
        }

        //FOR DEBUG ONLY!
        [HttpGet]
        public JsonResult UpdateDB()
        {
            StreamReader sr = new StreamReader(Server.MapPath(@"~/App_Data/items_full.json"));
            List<Item.Item> jsonItem = JsonConvert.DeserializeObject<List<Item.Item>>(sr.ReadToEnd());

            foreach (var item in jsonItem)
            {
                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MstubeConnection"].ToString());
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "INSERT INTO Item (item_id, image_src, video_src, title, url, description, topic, category, full_description, posted_time, views, quality)"
                                            + "VALUES (@item_id, @image_src, @video_src, @title, @url, @description, @topic, @category, @full_description, @posted_time, @views, @quality)";
                    command.Parameters.AddWithValue("@item_id", item.item_id);
                    command.Parameters.AddWithValue("@image_src", item.image_src);
                    command.Parameters.AddWithValue("@video_src", item.video_src);
                    command.Parameters.AddWithValue("@title", item.title);
                    command.Parameters.AddWithValue("@url", item.url);
                    command.Parameters.AddWithValue("@description", item.description);
                    command.Parameters.AddWithValue("@topic", item.topic);
                    command.Parameters.AddWithValue("@category", item.category);
                    command.Parameters.AddWithValue("@full_description", item.full_description);
                    command.Parameters.AddWithValue("@posted_time", item.posted_time);
                    //command.Parameters.AddWithValue("@video_time", item.video_time);
                    command.Parameters.AddWithValue("@views", item.views);
                    command.Parameters.AddWithValue("@quality", item.quality);
                    try
                    {
                        connection.Open();
                        int recordsAffected = command.ExecuteNonQuery();

                    }
                    catch (SqlException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                        break;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
                System.Diagnostics.Debug.WriteLine("Insert Succeed!");
            }
            return Json(jsonItem, JsonRequestBehavior.AllowGet);
        }
    }
}