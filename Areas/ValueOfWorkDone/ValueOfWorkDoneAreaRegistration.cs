using System.Web.Mvc;

namespace BusinessApplication.Areas.ValueOfWorkDone
{
    public class ValueOfWorkDoneAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "ValueOfWorkDone";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "ValueOfWorkDone_default",
                "ValueOfWorkDone/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}