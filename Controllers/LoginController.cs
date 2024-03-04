using BusinessApplication.Models;
using BusinessApplication.ViewModels;
//using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace BusinessApplication.Controllers
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
                    ModelState.AddModelError("", "Invalid User or Password!");
                }
            }

            return View(rec);
        }
        public ActionResult LogOut()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Login",new { area=""});

        }
        //[HttpGet]
        //public ActionResult ChangePassword()
        //{
        //    List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
        //    result = this.cn.TBL_BP_DICTIONARY.ToList();
        //    MultipleVM model = new MultipleVM();
        //   // model.changePassword = new ChangePasswordVM();
        //    model.DictListVM = result;

        //    //List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();

        //    //// Initialize the MultipleVM model with an instance of ChangePasswordVM
        //    //MultipleVM model = new MultipleVM
        //    //{
        //    //    DictListVM = result,
        //    //    changePassword = new ChangePasswordVM()
        //    //};

        //    return View(model);
        //}




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
                        return RedirectToAction("Index", "Home");
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

            return RedirectToAction("Index", "Home");

        }






        //[HttpPost]
        //public ActionResult ChangePassword(ChangePasswordVM rec)
        //{
        //    //List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
        //    //result = this.cn.TBL_BP_DICTIONARY.ToList();
        //    //MultipleVM model = new MultipleVM();
        //    ////model.changePassword = new ChangePasswordVM();
        //    //model.DictListVM = result;

        //    if (ModelState.IsValid)
        //    {
        //        string uid = Session["UserId"].ToString();
        //        var oldrec = this.cn.TBL_BP_USERMASTER.SingleOrDefault(p => p.BP_PASSWD == rec.OldPassword && p.BP_USER == uid);
        //        if (oldrec != null)
        //        {
        //            oldrec.BP_PASSWD = rec.NewPassword;
        //            this.cn.SaveChanges();
        //            TempData["msg"] = "Password Changed Successfully";
        //        }

        //    }
        //    return View();
        //}


        //[HttpPost]
        //public ActionResult ChangePassword(ChangePasswordVM rec)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Assuming you have a session for UserId
        //        string uid = Session["UserId"]?.ToString();

        //        if (!string.IsNullOrEmpty(uid))
        //        {
        //            var user = this.cn.TBL_BP_USERMASTER.SingleOrDefault(p => p.BP_PASSWD == rec.OldPassword && p.BP_USER == uid);

        //            if (user != null)
        //            {
        //                user.BP_PASSWD = rec.NewPassword;
        //                this.cn.SaveChanges();
        //                TempData["msg"] = "Password Changed Successfully";
        //            }
        //            else
        //            {
        //                ModelState.AddModelError("OldPassword", "Invalid old password");
        //            }
        //        }
        //        else
        //        {
        //            TempData["msg"] = "User not authenticated.";
        //            return RedirectToAction("Login"); // Redirect to login or handle as per your application flow
        //        }
        //    }

        //    // Repopulate model with dictionary and password change instance
        //    List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
        //    MultipleVM model = new MultipleVM
        //    {
        //        DictListVM = result,
        //        changePassword = new ChangePasswordVM() // Assuming ChangePassword is the property in MultipleVM for ChangePasswordVM instance
        //    };

        //    return View(model);
        //}


    }
}