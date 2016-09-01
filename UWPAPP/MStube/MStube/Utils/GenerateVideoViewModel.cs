using Microsoft.HockeyApp;
using MStube.Items;
using MStube.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Web.Http;


namespace MStube.Utils
{
    class GenerateVideoViewModel
    {
        public static List<VideoViewModel> GenerateVideoViewFromVideoDetail(List<VideoDetailItem> videoDetailItemCandidates, bool reverseInsert = true)
        {
            List<VideoViewModel> result = new List<VideoViewModel>();
            foreach (VideoDetailItem item in videoDetailItemCandidates)
            {
                VideoViewModel temp = new VideoViewModel
                {
                    item_id = item.item_id,
                    Title = item.title,
                    ImageSourceUri = item.image_src,
                    VideoSourceUri = item.video_src,
                    Description = item.description,
                    FullDescription = item.full_description,
                    Url = item.url,
                    Views = item.views,
                    UploadDate = item.posted_time,
                    Source = item.source,
                    Brand = item.brand
                };
                if (reverseInsert)
                {
                    result.Insert(0, temp);
                }
                else
                {
                    result.Add(temp);
                }
            }
            return result;
        }

    }
}
