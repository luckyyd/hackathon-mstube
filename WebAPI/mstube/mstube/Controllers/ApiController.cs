﻿using Microsoft.WindowsAzure.Storage;
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
        private IDatabase cachefilter;
        private IDatabase cacheid;
        public ApiController()
        {
            ConnectionMultiplexer FilterRedis = ConnectionMultiplexer.Connect(Properties.Settings.Default.RedisPostHistory);
            cachefilter = FilterRedis.GetDatabase();
            ConnectionMultiplexer ContentBasedRedis = ConnectionMultiplexer.Connect(Properties.Settings.Default.RedisLastItem);
            cacheid = ContentBasedRedis.GetDatabase();
        }
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult UserId(string uuid)
        {
            //Get user_id for uuid
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
        private async Task<List<Item.Item>> GetItemsFromSQLServerAsync(List<string> choicedItems, int brand)
        {
            List<Item.Item> resultList = new List<Item.Item>();
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MstubeConnection"].ToString());
            try
            {
                connection.Open();
                SqlCommand command = new SqlCommand();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                switch (brand)
                {
                    case 1:
                    case 2:
                        string itemsSet = "";
                        foreach (var item_id in choicedItems)
                        {
                            itemsSet += item_id.ToString();
                            itemsSet += ',';
                        }
                        itemsSet = itemsSet.TrimEnd(',');
                        command.CommandText = "SELECT * FROM Item WHERE item_id in (" + itemsSet + ")";
                        break;
                    case 3:
                        command.CommandText = "SELECT top 5 * FROM Item WHERE item_id IN (SELECT TOP 50 item_id FROM Item ORDER BY NewID() ) ORDER BY cast(views as int) DESC";
                        break;
                }
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        Debug.WriteLine(reader["item_id"]);
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
                            source = reader["source"].ToString(),
                            brand = brand,
                        };
                        resultList.Add(item);
                    }
                }
            }
            catch (SqlException err)
            {
                Debug.WriteLine(err);
            }
            catch (InvalidOperationException err)
            {
                Debug.WriteLine(err);
            }
            finally
            {
                connection.Close();
            }
            return resultList;
        }
        private async Task<List<string>> ItemFilterAsync(long user_id, List<string> itemsToFilter)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            List<string> result = new List<string>(itemsToFilter);
            string key = user_id.ToString();
            var historySet = await cachefilter.SetMembersAsync(key);
            if (result.Count > 0)
            {
                for (int i = result.Count - 1; i >= 0; --i)
                {
                    string item_id = result[i];
                    if (historySet.Contains(item_id))
                    {
                        result.Remove(item_id);
                    }
                }
            }
            timer.Stop();
            Debug.WriteLine("Filter Time: {0} ms", timer.ElapsedMilliseconds);
            return result;
        }
        private async Task<List<Item.Item>> GetCollaborativeFilterItemsAsync(long user_id)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            List<string> collaborativeFilteringCandidates = new List<string>();
            List<Item.Item> collaborativeFilteringList = new List<Item.Item>();

            string result = await AzureML_CollaborativeFilter.SendPOSTRequest(user_id);

            dynamic jsonObj = JsonConvert.DeserializeObject(result);
            JArray values = (JArray)jsonObj.Results.ScoringOutput.value.Values[0];
            collaborativeFilteringCandidates = values.ToObject<List<string>>();
            collaborativeFilteringCandidates.RemoveAt(0);

            if (collaborativeFilteringCandidates.Count > 0)
            {
                collaborativeFilteringCandidates = (await ItemFilterAsync(user_id, collaborativeFilteringCandidates)).Take(5).ToList();
                collaborativeFilteringList = await GetItemsFromSQLServerAsync(collaborativeFilteringCandidates, 1);
            }
            timer.Stop();
            Debug.WriteLine("Get Collaborative Filter items time: {0} ms", timer.ElapsedMilliseconds);
            return collaborativeFilteringList;
        }
        private async Task<List<Item.Item>> GetContentBasedItemsAsync(long user_id)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            List<string> contentBasedCandidates = new List<string>();
            List<Item.Item> contentBasedList = new List<Item.Item>();

            // Get last item 
            string last_item_id = cacheid.StringGet(user_id.ToString());

            // Get Content-based result
            if (last_item_id != null)
            {
                string contentbasedResult = await AzureML_ContentBasedFilter.SendPOSTRequest(user_id, Convert.ToInt64(last_item_id), 3);
                dynamic jsonContentbasedResultObj = JsonConvert.DeserializeObject(contentbasedResult);
                JArray values = (JArray)jsonContentbasedResultObj.Results.output1.value.Values[0];
                contentBasedCandidates = values.ToObject<List<string>>();
                contentBasedCandidates.RemoveAt(0);
            }

            // Filter Contect-based items
            if (contentBasedCandidates.Count > 0)
            {
                contentBasedCandidates = (await ItemFilterAsync(user_id, contentBasedCandidates)).Take(5).ToList();
                contentBasedList = await GetItemsFromSQLServerAsync(contentBasedCandidates, 2);
            }

            timer.Stop();
            Debug.WriteLine("Get Content based items time: {0} ms", timer.ElapsedMilliseconds);
            return contentBasedList;
        }
        private async Task<List<Item.Item>> GetPopularItemsAsync(int choices = 50, int top = 5)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            List<Item.Item> resultList = await GetItemsFromSQLServerAsync(new List<string>(), 3);
            timer.Stop();
            Debug.WriteLine("Get popular items time: {0} ms", timer.ElapsedMilliseconds);
            return resultList;
        }
        private void LogRecommendHistory(long user_id, List<Item.Item> items)
        {
            foreach (var v in items)
            {
                cachefilter.SetAdd(user_id.ToString(), v.item_id.ToString());
            }
        }

        [HttpGet]
        public async Task<JsonResult> Candidates(long user_id)
        {
            Stopwatch timerTotal = new Stopwatch();
            timerTotal.Start();

            List<Item.Item> collaborativeFilteringList = new List<Item.Item>();
            List<Item.Item> contentBasedList = new List<Item.Item>();
            List<Item.Item> popularityList = new List<Item.Item>();
            List<Item.Item> resultList = new List<Item.Item>();

            // Run get popularity items task
            Task<List<Item.Item>> taskGetCollaborativeFilterItems = GetCollaborativeFilterItemsAsync(user_id);
            Task<List<Item.Item>> taskGetContentBasedItems = GetContentBasedItemsAsync(user_id);
            Task<List<Item.Item>> taskGetPopularityItems = GetPopularItemsAsync(50, 5);

            // Get task results
            contentBasedList = await taskGetContentBasedItems;
            collaborativeFilteringList = await taskGetCollaborativeFilterItems;
            popularityList = await taskGetPopularityItems;

            // Combine result
            resultList.AddRange(collaborativeFilteringList);
            resultList.AddRange(contentBasedList);
            resultList.AddRange(popularityList);

            // Get distinct items
            List<Item.Item> distinctList = resultList.GroupBy(x => x.item_id).Select(g => g.First()).ToList();
            distinctList.Shuffle();
            distinctList = distinctList.Take(10).ToList();

            // Post History
            Task logRecommendHistory = Task.Run(() => LogRecommendHistory(user_id, distinctList));

            timerTotal.Stop();
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