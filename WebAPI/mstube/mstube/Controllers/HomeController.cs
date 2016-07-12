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
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public JsonResult Candidates()
        {
            StreamReader sr = new StreamReader(Server.MapPath(@"~/App_Data/items.json"));

            var json = JsonConvert.DeserializeObject<List<Item.Item>>(sr.ReadToEnd());

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Redis()
        {
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect("mstube-dotnet.redis.cache.windows.net,abortConnect=false,ssl=true,password=6/Cq0R6Wh+L6PJeYI80KEMVyYVGUjqZFEnNS6iJHl1A=");

            IDatabase cache = connection.GetDatabase();

            cache.StringSet("1 1", "1");
            cache.StringSet("2 2", "2");

            string key1 = cache.StringGet("1 1");
            ViewBag.Message = key1;
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> Preference(Preference.Preference pre) {
            //Send POST request to Azure ML
            string result = await Utils.AzureML.SendPOSTRequest(pre.user_id);

            return Json(result);
        }
    }
}