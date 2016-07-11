using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.IO;

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
    }
}