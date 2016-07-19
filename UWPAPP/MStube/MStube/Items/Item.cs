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
        public int Id { get; set; }
        public string ImageSourceUri { get; set; }
        public string VideoTitle { get; set; }
        public string Description { get; set; }
    }
    public class VideoDetailItem
    {
        // Used for json object.
        public int video_id { get; set; }
        public string image_src { get; set; }
        public string video_src { get; set; }
        public string title { get; set; }
        public string video_description { get; set; }
        public string topic { get; set; }
    }
    public class VideoDetailBrief
    {
        // Use for view.
        public string VideoTitle { get; set; }
        public string VideoSourceUri { get; set; }
        public string VideoDescription { get; set; }
    }
}
