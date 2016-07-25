using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MStube.Items
{
    //public class ShowItem
    //{
    //    // Used for json object.
    //    public int id { get; set; }
    //    public string url { get; set; }
    //    public string description { get; set; }
    //    public string image_src { get; set; }
    //    public string title { get; set; }
    //}
    public class VideoDetailItem
    {
        // Used for json object.
        public int item_id { get; set; }
        public string image_src { get; set; }
        public string video_src { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string topic { get; set; }
        public string category { get; set; }
        public string url { get; set; }
        public string full_description { get; set; }
        public string upload_date { get; set; }
        public float avg_rating { get; set;}
        public int views { get; set; }
    }
}
