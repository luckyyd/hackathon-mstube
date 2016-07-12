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
            StreamReader sr = new StreamReader(Server.MapPath(@"~/App_Data/items.json"));

            var json = JsonConvert.DeserializeObject<List<Item.Item>>(sr.ReadToEnd());

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> Preference(Preference.Preference pre)
        {
            //Send POST request to Azure ML
            string result = await Utils.AzureML.SendPOSTRequest(pre.user_id);
            return Json(result);
        }
    }
}