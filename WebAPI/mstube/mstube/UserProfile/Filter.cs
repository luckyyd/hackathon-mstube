using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data;
using Microsoft.WindowsAzure.Storage;
using System.Text;

namespace mstube.UserProfile
{
    public class Filter
    {
        public List<Item.Item> FilterItem(List<string> rawRecommendItemId, long user_id, string network, string time) {

            //Step1: Ger Raw Recommendation Data from DB
            #region Step1
            List<Item.Item> rawReconData = new List<Item.Item>();
            //Return items from db
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MstubeConnection"].ToString());

            foreach (var item_id in rawRecommendItemId)
            {
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
                                rawReconData.Add(new Item.Item
                                {
                                    item_id = Convert.ToInt64(reader["item_id"]),
                                    image_src = reader["image_src"].ToString(),
                                    video_src = reader["video_src"].ToString(),
                                    title = reader["title"].ToString(),
                                    url = reader["url"].ToString(),
                                    description = reader["description"].ToString(),
                                    full_description = reader["full_description"].ToString(),
                                    topic = reader["topic"].ToString(),
                                    category = reader["category"].ToString(),
                                    posted_time = reader["posted_time"].ToString(),
                                    video_time = reader["video_time"].ToString(),
                                    views = reader["views"].ToString(),
                                    quality = reader["quality"].ToString(),
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
            #endregion

            //Step2: Get User Interest Data from DB
            #region Step2
            List<Interest.Interest> InterestList = new List<Interest.Interest>();
            //Return interests from db
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT keywords, time FROM Interest WHERE user_id = @user_id";
                command.Parameters.AddWithValue("@user_id", user_id);
                try
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InterestList.Add(new Interest.Interest
                            {
                                keywords = reader["keyword"].ToString(),
                                time = Convert.ToInt64(reader["time"]),
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
            #endregion

            //Step3: Get Tags from DB
            #region Step3
            foreach (var item_id in rawRecommendItemId)
            {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT tag FROM Tag WHERE item_id = @item_id";
                    command.Parameters.AddWithValue("@item_id", item_id);
                    try
                    {
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                rawReconData.Find(x => x.item_id.ToString() == item_id).tags.Add(reader["tag"].ToString());
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

                #endregion

                //Step4: Ratings
                #region Step4
                long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            foreach (Item.Item item in rawReconData) {
                foreach (string tag in item.tags) {
                    foreach (var interest in InterestList) {
                        if (tag == interest.keywords) {
                            item.score += 5 / (milliseconds - interest.time); //weight
                        }
                    }
                }
            }

            // Filter network
            if (network == "4G") {
                foreach (Item.Item item in rawReconData) {
                    if (item.category == "webpage") {
                        item.score += 50;
                    }
                }
            }

            //TODO: filter time

            #endregion

            //Sort and return top 10 items
            rawReconData.Sort(delegate (Item.Item i1, Item.Item i2) { return i1.score.CompareTo(i2.score); });
            List<Item.Item> filterReconData = rawReconData.GetRange(0, 10);

            return filterReconData;

        }
    }
}