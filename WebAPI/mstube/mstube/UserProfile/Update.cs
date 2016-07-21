using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using Microsoft.WindowsAzure.Storage;
using System.Text;

namespace mstube.UserProfile
{
    public class Update
    {
        public static void updateUserProfile(Preference.Preference pre, long milliseconds) {
            //Update
            List<string> keywords = new List<string>();
            #region
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MstubeConnection"].ToString());

            //Get All keywords
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT keywords FROM Tag WHERE item_id = @item_id";
                command.Parameters.AddWithValue("@item_id", pre.item_id);
                try
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            keywords.Add(reader["tag"].ToString());
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

            //Update
            foreach (var keyword in keywords) {
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.CommandText = "UPDATE Interest SET time = @time WHERE item_id = @item_id AND keyword = @keyword";
                    command.Parameters.AddWithValue("@item_id", pre.item_id);
                    command.Parameters.AddWithValue("@keyword", keyword);
                    command.Parameters.AddWithValue("@time", milliseconds);
                    try
                    {
                        connection.Open();
                        int recordsAffected = command.ExecuteNonQuery();

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
        }

    }
}