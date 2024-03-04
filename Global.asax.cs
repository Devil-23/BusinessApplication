using BusinessApplication.CustomFilter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BusinessApplication
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        //protected void Session_Start(object sender, EventArgs e)
        //{
        //    Session.Timeout = 1;
        //    Session["SessionStartTime"] = DateTime.Now;
        //}
       
        //protected void Session_End(object sender, EventArgs e)
        //{
        //    DateTime sessionStartTime = (DateTime)Session["SessionStartTime"];
        //    DateTime currentTime = DateTime.Now;
        //    TimeSpan sessionDuration = currentTime - sessionStartTime;

        //    int timeoutInSeconds = 60;
        //    if (sessionDuration.TotalSeconds >= timeoutInSeconds)
        //    {

        //        Session.Abandon();
        //        HttpContext.Current.Response.RedirectToRoute(new { controller = "Login", action = "Index" });


        //    }
        //}

    }
}
