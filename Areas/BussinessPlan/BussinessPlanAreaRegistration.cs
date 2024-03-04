using System.Web.Mvc;

namespace BusinessApplication.Areas.BussinessPlan
{
    public class BussinessPlanAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "BussinessPlan";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "BussinessPlan_default",
                "BussinessPlan/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}