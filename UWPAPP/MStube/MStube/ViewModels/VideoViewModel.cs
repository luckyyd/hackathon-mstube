using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MStube.ViewModels
{
    public class VideoViewModel
    {
        public long user_id { get; set; }
        public long item_id { get; set; }
        public string Title { get; set; }
        public string Topic { get; set; }
        public string ImageSourceUri { get; set; }
        public string VideoSourceUri { get; set; }
        public string Description { get; set; }
        public string FullDescription { get; set; }
        public int Views { get; set; }
        public string UploadDate { get; set; }
        public int Brand { get; set; }
        public string BrandIcon
        {
            get
            {
                if (Brand == 1)
                    // CF recommend
                    return "\xe60a";
                if (Brand == 2)
                    // Content-based recommend
                    return "\xe609";
                if (Brand == 3)
                    // Random recommend
                    return "\xe60b";
                return "";
            }
        }
    }
}
