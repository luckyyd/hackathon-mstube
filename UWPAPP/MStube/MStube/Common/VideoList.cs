using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MStube.ViewModels;
namespace MStube.Common
{
    public class VideoList
    {
        private static VideoList _Instance = new VideoList();
        private List<VideoViewModel> videoList { get; set; }
        public static VideoList Instance
        {
            get
            {
                return _Instance;
            }
        }
        private VideoList()
        {
            videoList = new List<VideoViewModel>();
        }
        public void Add(VideoViewModel video)
        {
            videoList.Insert(0, video);
            if (videoList.Count > 50)
            {
                videoList.RemoveAt(50);
            }
        }
        public List<VideoViewModel> GetList()
        {
            return videoList;
        }
    }
}
