using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MStube.ViewModels
{
    public class VideoViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Topic { get; set; }
        public string ImageSourceUri { get; set; }
        public string VideoSourceUri { get; set; }
        public string Description { get; set; }
        public string FullDescription { get; set; }
        public int Views { get; set; }
        public string UploadDate { get; set; }
        public int Brand { get; set; }
    }
}
