using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;

namespace BusinessApplication.CustomFilter
{
    public class UserAuth : ActionFilterAttribute, IAuthenticationFilter
    {
        
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            if (Convert.ToString(filterContext.HttpContext.Session["UserId"]) == null)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            if (Convert.ToString(filterContext.HttpContext.Session["UserId"]) == null || filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action" , "Index" }, { "controller" , "Login" } });
            }
        }
    }
}