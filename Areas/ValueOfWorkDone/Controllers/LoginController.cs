using BusinessApplication.Models;
using BusinessApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BusinessApplication.Areas.ValueOfWorkDone.Controllers
{
    public class LoginController : Controller
    {
        BusinessPlanEntities cn = new BusinessPlanEntities();
        [HttpGet]
        public ActionResult Index()
        {
            HttpCookie cookie = Request.Cookies["BP"];
            if (cookie != null)
            {
                ViewBag.User = cookie["User"].ToString();
                ViewBag.Password = cookie["Pass"].ToString();
            }
            return View();
        }
        [HttpPost]
        public ActionResult Index(LoginVM rec)
        {
            HttpCookie cookie = new HttpCookie("BP");
            if (ModelState.IsValid)
            {
                if (rec.RememberMe == true)
                {
                    cookie["User"] = rec.User;
                    cookie["Pass"] = rec.Password;
                    cookie.Expires = DateTime.Now.AddDays(1);
                    HttpContext.Response.Cookies.Add(cookie);
                }

                var result = this.cn.TBL_BP_USERMASTER.SingleOrDefault(p => p.BP_USER == rec.User && p.BP_PASSWD == rec.Password);
                if (result != null)
                {
                    Session["UserId"] = rec.User;
                    return RedirectToAction("Index", "Home");
                }
                {
                    ModelState.AddModelError("", "Invalid Email ID or Password!");
                }
            }

            return View(rec);
        }
        public ActionResult LogOut()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Login");

        }
    
        [HttpGet]
        public ActionResult ChangePassword()
        {
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.DictListVM = result;
            return View(model);
        }
        [HttpPost]
        public ActionResult ChangePassword(MultipleVM rec)
        {
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.DictListVM = result;
            if (ModelState.IsValid)
            {
                string uid = Session["UserId"].ToString();
                if (!string.IsNullOrEmpty(uid))
                {

                    var oldrec = this.cn.TBL_BP_USERMASTER.SingleOrDefault(p => p.BP_PASSWD == rec.changePassword.OldPassword && p.BP_USER == uid);
                    if (oldrec != null)
                    {
                        oldrec.BP_PASSWD = rec.changePassword.NewPassword;
                        this.cn.SaveChanges();
                        TempData["msg"] = "Password Changed Successfully";
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid old password");
                    }
                }
                else
                {
                    TempData["msg"] = "User not authenticated.";
                    return RedirectToAction("Login"); // Redirect to login or handle as per your application flow
                }

            }

            return View(model);
        }
    }
}