using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace mstube
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Start the backend job (10min per round)
            System.Timers.Timer Timer = new System.Timers.Timer(600000);
            Timer.Elapsed += new System.Timers.ElapsedEventHandler(Utils.RetrainModel.InvokeBatchExecutionService1);
            Timer.Enabled = true;
            Timer.AutoReset = true;
        }
    }
}
