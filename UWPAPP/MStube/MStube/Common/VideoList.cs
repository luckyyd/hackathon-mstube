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
        private static VideoList _Instance;
        private static readonly object padlock = new object();
        private List<VideoViewModel> videoList { get; set; }
        public static VideoList Instance
        {
            get
            {
                if (_Instance == null)
                {
                    lock(padlock)
                    {
                        if (_Instance == null)
                        {
                            _Instance = new VideoList();
                        }
                    }
                }
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
