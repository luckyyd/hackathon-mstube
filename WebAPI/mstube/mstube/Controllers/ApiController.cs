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

            timer.Start();
            //Filter data in cache
            ConnectionMultiplexer FilterRedis = ConnectionMultiplexer.Connect(Properties.Settings.Default.RedisPostHistory);
            IDatabase cachefilter = FilterRedis.GetDatabase();
            timer.Stop();
            long timerConnectHistory = timer.ElapsedMilliseconds;
            //Get Data from collaborative filtering
            //Send POST request to Azure ML

            timer.Restart();
            string result = await AzureML_CollaborativeFilter.SendPOSTRequest(user_id);
            timer.Stop();
            long timerConnectAzureCF = timer.ElapsedMilliseconds;
            dynamic jsonObj = JsonConvert.DeserializeObject(result);
            JArray values = (JArray)jsonObj.Results.ScoringOutput.value.Values[0];
            collaborativeFilteringCandidates = values.ToObject<List<string>>();
            collaborativeFilteringCandidates.RemoveAt(0);

            timer.Restart();
            if (collaborativeFilteringCandidates.Count > 0)
            {
                //Filter collaborativeFilteringCandidates
                for (int i = collaborativeFilteringCandidates.Count - 1; i >= 0; i--)
                {
                    string v = collaborativeFilteringCandidates[i];
                    if (cachefilter.SetContains(user_id.ToString(), v))
                    {
                        collaborativeFilteringCandidates.Remove(collaborativeFilteringCandidates[i]);
                    }
                }
                collaborativeFilteringCandidates = collaborativeFilteringCandidates.Take(5).ToList();
            }
            timer.Stop();
            long timerFilterCF = timer.ElapsedMilliseconds;

            timer.Restart();

            //Get data from content-based filtering in Redis
            ConnectionMultiplexer ContentBasedRedis = ConnectionMultiplexer.Connect(Properties.Settings.Default.RedisLastItem);
            IDatabase cacheid = ContentBasedRedis.GetDatabase();
            timer.Stop();
            long timerConnectRedisLastItem = timer.ElapsedMilliseconds;

            string last_item_id = cacheid.StringGet(user_id.ToString());
            Debug.WriteLine("Last item id is: {0}", last_item_id);
            timer.Restart();
            if (last_item_id != null)
            {
                //Call ML api
                string contentbasedResult = await AzureML_ContentBasedFilter.SendPOSTRequest(user_id, Convert.ToInt64(last_item_id), 3);
                dynamic jsonContentbasedResultObj = JsonConvert.DeserializeObject(contentbasedResult);
                JArray valuesContentbasedResult = (JArray)jsonContentbasedResultObj.Results.output1.value.Values[0];
                contentBasedCandidates = valuesContentbasedResult.ToObject<List<string>>();
                contentBasedCandidates.RemoveAt(0);

            }
            timer.Stop();
            long timerAzureMLcontent = timer.ElapsedMilliseconds;
            long CountContent = contentBasedCandidates.Count;

            timer.Restart();
            if (contentBasedCandidates.Count > 0)
            {
                //Filter contentBasedCandidates
                for (int i = contentBasedCandidates.Count - 1; i >= 0; i--)
                {
                    string v = contentBasedCandidates[i];
                    if (cachefilter.SetContains(user_id.ToString(), v))
                    {
                        contentBasedCandidates.Remove(contentBasedCandidates[i]);
                    }
                }
                contentBasedCandidates = contentBasedCandidates.Take(5).ToList();
            }
            timer.Stop();
            long timerFilterContent = timer.ElapsedMilliseconds;
            timer.Restart();
            HashSet<int> randSet = new HashSet<int>();
            //Get data from popularity filtering
            const int max = 5000;
            while (randSet.Count < 50)
            {
                Random ran = new Random();
                int Randkey = ran.Next(1, max);
                if (!randSet.Contains(Randkey))
                {
                    popularityCandidates.Add(Randkey.ToString());
                    randSet.Add(Randkey);
                }
            }
            timer.Stop();
            long timerGenerateRandomSet = timer.ElapsedMilliseconds;
            totalCandidates.AddRange(contentBasedCandidates);
            totalCandidates.AddRange(collaborativeFilteringCandidates);
            totalCandidates.AddRange(popularityCandidates);

            timer.Restart();
            //Return items from db
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MstubeConnection"].ToString());

            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;

                string itemsSet = "";
                foreach (var item_id in totalCandidates)
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
                        if (collaborativeFilteringCandidates.Contains(item_id)) { item.brand = 1; }
                        else if (contentBasedCandidates.Contains(item_id)) { item.brand = 2; }
                        else if (popularityCandidates.Contains(item_id)) { item.brand = 3; }
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

            timer.Stop();
            long timerGetItemsFromSQLServer = timer.ElapsedMilliseconds;

            timer.Restart();
            //filter and sort
            List<Item.Item> distinctList = resultList.GroupBy(x => x.item_id).Select(g => g.First()).ToList();
            List<Item.Item> popularityList = new List<Item.Item>();
            for (int i = distinctList.Count - 1; i >= 0; i--)
            {
                if (distinctList[i].brand == 3)
                {
                    popularityList.Add(distinctList[i]);
                    distinctList.Remove(distinctList[i]);
                }
            }
            popularityList = popularityList.OrderByDescending(o => o.views).ToList();

            if (popularityList.Count > 0)
            {
                //Filter popularityList
                for (int i = popularityList.Count - 1; i >= 0; i--)
                {
                    string v = popularityList[i].item_id.ToString();
                    if (cachefilter.SetContains(user_id.ToString(), v))
                    {
                        popularityList.Remove(popularityList[i]);
                    }
                }
                popularityList = popularityList.Take(5).ToList();
            }
            distinctList.AddRange(popularityList);
            distinctList = distinctList.Take(10).ToList();
            distinctList.Shuffle();
            timer.Stop();
            long timerFilterPopularity = timer.ElapsedMilliseconds;

            timer.Restart();
            foreach (var v in totalCandidates)
            {
                cachefilter.SetAdd(user_id.ToString(), v);
            }
            timer.Stop();
            long timerAddCache = timer.ElapsedMilliseconds;

            timerTotal.Stop();
            Debug.WriteLine("Content based item count: {0}", contentBasedCandidates.Count);
            Debug.WriteLine("total candidates: {0} ms", totalCandidates.Count);
            Debug.WriteLine("");
            Debug.WriteLine("Connect Redis Post History time: {0} ms", timerConnectHistory);
            Debug.WriteLine("Get result from Azure ML CF time: {0} ms", timerConnectAzureCF);
            Debug.WriteLine("CF candidates count: {0}", collaborativeFilteringCandidates.Count);
            Debug.WriteLine("Filter the CF result with history time: {0} ms", timerFilterCF);
            Debug.WriteLine("Connect to Redis last item time: {0}", timerConnectRedisLastItem);
            Debug.WriteLine("Get the Azure ML content base items time: {0} ms", timerAzureMLcontent);
            Debug.WriteLine("Filter the content based items time: {0} ms", timerFilterContent);
            Debug.WriteLine("Generate 50 random set  time: {0} ms", timerGenerateRandomSet);
            Debug.WriteLine("Get items from SQL server time: {0} ms", timerGetItemsFromSQLServer);
            Debug.WriteLine("Filter and sort popularity list time: {0} ms", timerFilterPopularity);
            Debug.WriteLine("Add Cache: {0} ms ", timerAddCache);

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