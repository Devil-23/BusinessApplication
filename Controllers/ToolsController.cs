using BusinessApplication.CustomFilter;
using BusinessApplication.Models;
using BusinessApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Infrastructure;

namespace BusinessApplication.Controllers
{
   [SessionAuth]
    public class ToolsController : Controller
    {
        BusinessPlanEntities cn = new BusinessPlanEntities();
        // GET: Tools
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult ResourceTools(int year, string jobno, string resotype)
        {
             var model = TempData["SearchResults"] as MultipleVM;

            if (model == null)
            {
                // Handle the case where TempData["SearchResults"] is null
                model = new MultipleVM();
                var res = (from a in cn.TBL_RESOURCE_GLOBAL
                           where a.RESO_TYPE == "MP"
                           orderby a.RESO_PTR, a.RESO_TITLE
                           select new IncomeVM
                           {
                               ID = a.RESO_ID,
                               DESCRIPTION = a.RESO_TITLE,
                               Unit = a.RESO_UNIT,
                               Rate = a.RESO_RATE,
                           }).ToList();

                model.InList = res.ToList();
            }

//year = TempData["Year"] as int?;
            ViewBag.Year = year;
            ViewBag.JobNo = jobno;
            ViewBag.ResoType = resotype;
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            //MultipleVM model = new MultipleVM();
            
           
           //model.GlobalList = cn.PROC_BP_SHOW_MASTER("MT");
            model.DictListVM = result;
            return View(model);
            //return View(GobleData.mulVm);

        }
        [HttpPost,ActionName("ResourceTools")]
        public ActionResult ResourceT(MultipleVM rec, int year, string jobno,string resotype)
        {
            ViewBag.Year = year;
            ViewBag.JobNo = jobno;
            ViewBag.ResoType = resotype;
            using (var transaction = this.cn.Database.BeginTransaction())
            {
                try
                {
                    var selectedRows = rec.InList.Where(item => item.IsSelected).ToList();

                    foreach (var data in selectedRows)
                    {
                        int ind = GobleData.mulVm.InList.FindIndex(z => z.ID == data.ID);
                        if (ind < 0)
                        {
                            IncomeVM x = new IncomeVM();
                            x.DESCRIPTION = data.DESCRIPTION;
                            x.ID = data.ID;
                            x.Rate = data.Rate;
                            x.Unit = data.Unit;
                            x.JAN = 0;x.FEB = 0;x.MAR = 0;x.APR = 0;x.MAY = 0;x.JUN = 0;x.JUL = 0;x.AUG = 0;x.SEP = 0;x.OCT = 0;x.NOV = 0;x.DEC = 0;
                            GobleData.mulVm.InList.Add(x);
                        }
                    

                    }                    
                  
                }    
                catch (Exception ex)
                {
                    transaction.Rollback();
             }
            }
            if (resotype == "MP")
            {
                return View("~/Areas/BussinessPlan/Views/BP/Manpower.cshtml", GobleData.mulVm);
            }
            else if (resotype == "PG")
            {
                return View("~/Areas/BussinessPlan/Views/BP/PlantGalfar.cshtml", GobleData.mulVm);
            }

            return View("~/Areas/BussinessPlan/Views/BP/PlantHire.cshtml", GobleData.mulVm);
        }


        public ActionResult SearchByTitle(string Title, int year)
        {
            
            MultipleVM model = new MultipleVM();
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            model.DictListVM = result;
            var res = (from a in cn.TBL_RESOURCE_GLOBAL
                       where a.RESO_TYPE == "MP"
                       orderby a.RESO_PTR, a.RESO_TITLE
                       select new IncomeVM
                       {
                           ID = a.RESO_ID,
                           DESCRIPTION = a.RESO_TITLE,
                           Unit = a.RESO_UNIT,
                           Rate = a.RESO_RATE,
                       }).ToList();

            model.InList = res;
            TempData["SearchResults"] = model;
           // TempData["Year"] = year;
            if (Title == "")
            {
                return RedirectToAction("ResourceTools",new { year = year });
            }
            var v = (from a in cn.TBL_RESOURCE_GLOBAL
                     where a.RESO_TYPE == "MP" && a.RESO_TITLE.StartsWith(Title) || a.RESO_ID.StartsWith(Title)
                     orderby a.RESO_PTR, a.RESO_TITLE
                     select new IncomeVM
                     {
                         ID = a.RESO_ID,
                         DESCRIPTION = a.RESO_TITLE,
                         Unit = a.RESO_UNIT,
                         Rate = a.RESO_RATE,
                     }).ToList();

            model.InList = v;
            TempData["SearchResults"] = model;
            //TempData["Year"] = year;

            return RedirectToAction("ResourceTools", new { year = year });
        }
        //public PartialViewResult ResourceTools1(int year,string jobno,string resotype)
        //{
        //    List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
        //    result = this.cn.TBL_BP_DICTIONARY.ToList();
        //    MultipleVM model = new MultipleVM();
        //    model.GlobalList = cn.PROC_BP_SHOW_MASTER("MT");
        //    model.DictListVM = result;
        //    return PartialView(model);

        //}
        public PartialViewResult ResourceTools1()
        {
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.GlobalList = cn.PROC_BP_SHOW_MASTER("MT");
            model.DictListVM = result;
            return PartialView(model);
        }
        //[HttpPost]
        [HttpGet]
        public ActionResult Applyreso()
        {
            return View("../Year/GetManpower", GobleData.mulVm);
        }
    }
}