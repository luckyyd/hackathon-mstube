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
        public string title { get; set; }
        public long id { get; set; }
        public string url { get; set; }
        public string image_src { get; set; }
        public string description { get; set; }
    }
}