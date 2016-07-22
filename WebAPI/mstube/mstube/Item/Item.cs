using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Text;
using System.Threading.Tasks;

namespace mstube.Item
{
    public class Item
    {
        public long item_id { get; set; }
        public string image_src { get; set; }
        public string video_src { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public string full_description { get; set; }
        public string topic { get; set; }
        public string category { get; set; }
        public string posted_time { get; set; }
        public string video_time { get; set; }
        public int views { get; set; }
        public double quality { get; set; }
        public List<string> tags { get; set; }
        public double score { get; set; }
    }
}