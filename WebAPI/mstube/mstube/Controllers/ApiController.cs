using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.IO;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace mstube.Controllers
{
    public class ApiController : Controller
    {
        // GET: Api
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult Candidates()
        {
            //Return a random recommend list for test
            StreamReader sr = new StreamReader(Server.MapPath(@"~/App_Data/items.json"));
            var json = JsonConvert.DeserializeObject<List<Item.Item>>(sr.ReadToEnd());
            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> Candidates(long id)
        {
            //Get new id for user_id

            //Send POST request to Azure ML
            string result = await Utils.AzureML.SendPOSTRequest(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Preference(Preference.Preference pre)
        {
            //Write preference data to Redis
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect("mstube-dotnet.redis.cache.windows.net,abortConnect=false,ssl=true,password=6/Cq0R6Wh+L6PJeYI80KEMVyYVGUjqZFEnNS6iJHl1A=");
            IDatabase cache = connection.GetDatabase();
            string preStr = pre.user_id.ToString() + pre.item_id.ToString();
            cache.StringSet(preStr, pre.score);
            return Json(pre);
        }
    }
}