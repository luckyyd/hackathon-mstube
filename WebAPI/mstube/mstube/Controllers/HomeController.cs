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

        public ActionResult Redis()
        {
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(Properties.Settings.Default.RedisContentBased);

            IDatabase cache = connection.GetDatabase();

            cache.StringSet("1 1", "1");
            cache.StringSet("2 2", "2");

            string key1 = cache.StringGet("1 1");
            ViewBag.Message = key1;
            return View();
        }

    }
}