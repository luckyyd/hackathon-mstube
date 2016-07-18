using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MStube.Items
{
    public class VideoItem
    {
        // Used for json object.
        public int id { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public string image_src { get; set; }
        public string title { get; set; }
    }

    public class VideoBrief
    {
        // Used for view.
        public string ImageSourceUri { get; set; }
        public string VideoTitle { get; set; }
        public string Description { get; set; }
    }
}
