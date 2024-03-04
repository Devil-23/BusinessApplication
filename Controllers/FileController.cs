using BusinessApplication.Models;
using BusinessApplication.ViewModels;
using Microsoft.Ajax.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BusinessApplication.Controllers
{
    public class FileController : Controller
    {
        BusinessPlanEntities cn = new BusinessPlanEntities();
        [HttpGet]
        public ActionResult Index( )
        {
            ViewBag.CLIENT = new SelectList(this.cn.TBL_BP_CLIENT.ToList(), "CLIENT", "ClientName");
            ViewBag.LId = new SelectList(this.cn.TBL_BP_LOCATION.ToList(), "Id", "Location");
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.DictListVM = result;
            return View(model);
        }
        [HttpPost]
        public ActionResult Index(MultipleVM rec)
        {
            ViewBag.CLIENT = new SelectList(this.cn.TBL_BP_CLIENT.ToList(), "CLIENT", "ClientName");
            ViewBag.LId = new SelectList(this.cn.TBL_BP_LOCATION.ToList(), "Id", "Location");
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.DictListVM = result;
            if (ModelState.IsValid)
            {
                if (rec.DictVM.FDATE < rec.DictVM.SDATE)
                {
                    ModelState.AddModelError("DictVM.FDATE", "Finish date must be later than or equal to start date.");
                    return View(model);
                }
                using (var dbContext = new BusinessPlanEntities())
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        TBL_BP_DICTIONARY res = new TBL_BP_DICTIONARY();
                        res.BP_YEAR = rec.DictVM.BP_YEAR;
                        res.JOB_NO = rec.DictVM.JOB_NO;
                        res.SDATE = rec.DictVM.SDATE;
                        res.FDATE = rec.DictVM.FDATE;
                        res.JOB_VOWD = rec.DictVM.JOB_VOWD;
                        res.JOB_TITLE = rec.DictVM.JOB_TITLE;
                        rec.DictVM.DIVISION = rec.DictVM.DIVISION;
                        res.DIVISION = rec.DictVM.DIVISION;
                        res.BP_MANAGER = rec.DictVM.BP_MANAGER;
                        int cid = Convert.ToInt32(rec.DictVM.CLIENT);
                        var client = dbContext.TBL_BP_CLIENT.FirstOrDefault(x => x.CLIENT == cid);
                        res.CLIENT = client.ClientName;
                        int lid = Convert.ToInt32(rec.DictVM.LOCATION);
                        var location = dbContext.TBL_BP_LOCATION.FirstOrDefault(x => x.ID == lid);
                        res.LOCATION = location.Location;
                        res.OT_PERCENT = rec.DictVM.OT_PERCENT;
                        res.FUEL_PERCENT = rec.DictVM.FUEL_PERCENT;
                        res.FOOD_STAFF = rec.DictVM.FOOD_STAFF;
                        res.FOOD_WORKER = rec.DictVM.FOOD_WORKER;
                        res.ModifiedOn = DateTime.Now;
                        res.ModifiedBy = Convert.ToString(Session["UserId"]);
                        dbContext.TBL_BP_DICTIONARY.Add(res);
                        TempData["Division"] = rec.DictVM.DIVISION;
                        TempData["Year"] = rec.DictVM.BP_YEAR;
                        dbContext.SaveChanges();

                        var jobNo = rec.DictVM.JOB_NO;
                        dbContext.Database.ExecuteSqlCommand(
                            "EXEC PROC_BP_UPDATE_PERMISSION @JOB_NO",
                            new SqlParameter("@JOB_NO", jobNo)
                        );

                        transaction.Commit();

                      
                        ModelState.Clear();
                        return View(model);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "An error occurred while processing your request.");
                        return View(rec);
                    }
                }
            }
            if (rec.DictVM.FDATE < rec.DictVM.SDATE)
            {
                ModelState.AddModelError("DictVM.FDATE", "Finish date must be later than or equal to start date.");
                return View(model);
            }
            return View(model);
        }


        [HttpGet]
        public ActionResult GetDivision(string JOBNO)

        {
            try
            {
                if (JOBNO != null)
                    JOBNO = JOBNO.Trim();

                var data = (from t in this.cn.TBL_BP_DIVISION.Where(p => p.DIVNO == JOBNO)
                            select new { Div = t.DIVTX }).FirstOrDefault();
                if (data != null)
                {
                    return Json(data, JsonRequestBehavior.AllowGet);
                }


            }
            catch
            {

            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(int year,string jobno)
        {
            ViewBag.BP_YEAR = year;
            ViewBag.JOBNO = jobno;
            var rec = this.cn.TBL_BP_DICTIONARY.Where(p => p.JOB_NO == jobno && p.BP_YEAR==year).FirstOrDefault();
            if (rec != null)
            {
                List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
                result = this.cn.TBL_BP_DICTIONARY.ToList();
                MultipleVM model = new MultipleVM();
                model.DictListVM = result;
                model.DictVM = rec;
                ViewBag.CLIENT = new SelectList(this.cn.TBL_BP_CLIENT.ToList(), "CLIENT", "ClientName", rec.CLIENT);
                ViewBag.LId = new SelectList(this.cn.TBL_BP_LOCATION.ToList(), "Id", "Location", rec.LOCATION);
                return View(model);
            }
            return RedirectToAction("Index", new { year = year, jobno = jobno});
        }
        [HttpPost]
        public ActionResult Edit(MultipleVM rec, int year, string jobno)
        {
            ViewBag.CLIENT = new SelectList(this.cn.TBL_BP_CLIENT.ToList(), "CLIENT", "ClientName", rec.DictVM.CLIENT);
            ViewBag.LId = new SelectList(this.cn.TBL_BP_LOCATION.ToList(), "Id", "Location", rec.DictVM.LOCATION);
           // var existingRecord;
            if (ModelState.IsValid)
            {
                TBL_BP_DICTIONARY res = new TBL_BP_DICTIONARY();              

                if (rec.DictVM.CLIENT != null)
                {
                    int cid = Convert.ToInt32(rec.DictVM.CLIENT);
                    var client = this.cn.TBL_BP_CLIENT.FirstOrDefault(x => x.CLIENT == cid);
                    res.CLIENT = client.ClientName;
                }
                else
                {
                    var existingRecord = this.cn.TBL_BP_DICTIONARY
                        .AsNoTracking()
                        .Where(p => p.JOB_NO == jobno && p.BP_YEAR == year)
                        .FirstOrDefault();

                    if (existingRecord != null)
                    {
                        res.CLIENT = existingRecord.CLIENT;
                    }
                }

                if (rec.DictVM.LOCATION != null)
                {
                    int lid = Convert.ToInt32(rec.DictVM.LOCATION);
                    var location = this.cn.TBL_BP_LOCATION.FirstOrDefault(x => x.ID == lid);
                    res.LOCATION = location.Location;
                }
                else
                {
                    var existingRecord = this.cn.TBL_BP_DICTIONARY
                        .AsNoTracking()
                        .Where(p => p.JOB_NO == jobno && p.BP_YEAR == year)
                        .FirstOrDefault();

                    if (existingRecord != null)
                    {
                        res.LOCATION = existingRecord.LOCATION;
                    }
                }
                using (var dbContext = new BusinessPlanEntities())
                using (var transaction = dbContext.Database.BeginTransaction())
                {
                    try
                    {                 
                        
                            string sql =" MERGE INTO TBL_BP_DICTIONARY " +
                                        "USING(VALUES(" + rec.DictVM.BP_YEAR + ",'" + rec.DictVM.JOB_NO + "','" + rec.DictVM.DIVISION + "','" + rec.DictVM.JOB_VOWD + "','" + ((DateTime)rec.DictVM.SDATE).ToString("yyyy-MM-dd") + "','" + rec.DictVM.BP_MANAGER + "','" + ((DateTime)rec.DictVM.FDATE).ToString("yyyy-MM-dd") + "','" + res.CLIENT + "','" + res.LOCATION + "','" + rec.DictVM.JOB_TITLE + "'," + rec.DictVM.OT_PERCENT + "," + rec.DictVM.FOOD_STAFF + "," + rec.DictVM.FOOD_WORKER + "," + rec.DictVM.FUEL_PERCENT + ")) AS source(BP_YEAR, JOB_NO, DIVISION, JOB_VOWD, SDATE, BP_MANAGER, FDATE, CLIENT, LOCATION, JOB_TITLE, OT_PERCENT, FOOD_STAFF, FOOD_WORKER, FUEL_PERCENT)" +
                                        "ON TBL_BP_DICTIONARY.BP_YEAR = source.BP_YEAR AND TBL_BP_DICTIONARY.JOB_NO = source.JOB_NO " +
                                        "WHEN MATCHED THEN " +
                                        "UPDATE SET BP_YEAR = source.BP_YEAR,JOB_NO = source.JOB_NO,DIVISION = source.DIVISION,JOB_VOWD = source.JOB_VOWD,SDATE = source.SDATE,BP_MANAGER = source.BP_MANAGER,FDATE = source.FDATE,CLIENT = source.CLIENT,LOCATION = source.LOCATION,JOB_TITLE = source.JOB_TITLE,OT_PERCENT = source.OT_PERCENT,FOOD_STAFF = source.FOOD_STAFF,FOOD_WORKER = source.FOOD_WORKER,FUEL_PERCENT = source.FUEL_PERCENT " +
                                        "WHEN NOT MATCHED THEN " +
                                        "INSERT(BP_YEAR, JOB_NO, DIVISION, JOB_VOWD, SDATE, BP_MANAGER, FDATE, CLIENT, LOCATION, JOB_TITLE, OT_PERCENT, FOOD_STAFF, FOOD_WORKER, FUEL_PERCENT)" +
                                        "VALUES(source.BP_YEAR, source.JOB_NO, source.DIVISION, source.JOB_VOWD, source.SDATE, source.BP_MANAGER, source.FDATE, source.CLIENT, source.LOCATION, source.JOB_TITLE, source.OT_PERCENT, source.FOOD_STAFF, source.FOOD_WORKER, source.FUEL_PERCENT);" +
                                        "DELETE FROM TBL_BP_DICTIONARY WHERE BP_YEAR = " + year + " AND JOB_NO = '" + jobno + "';" ;
                        dbContext.Database.ExecuteSqlCommand(sql);                       
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine(ex.Message);
                        return RedirectToAction("Error", "Home");
                    }
                }
                List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
                result = this.cn.TBL_BP_DICTIONARY.ToList();
                MultipleVM model = new MultipleVM();
                model.DictListVM = result;
                ModelState.Clear();
                return RedirectToAction("Index");
            }
            return View();
        }


        //[HttpPost]
        //public ActionResult Edit(MultipleVM rec,int year,string jobno)
        //{
        //    ViewBag.CLIENT = new SelectList(this.cn.TBL_BP_CLIENT.ToList(), "CLIENT", "ClientName", rec.DictVM.CLIENT);
        //    ViewBag.LId = new SelectList(this.cn.TBL_BP_LOCATION.ToList(), "Id", "Location", rec.DictVM.LOCATION);
        //    if (ModelState.IsValid)
        //    {
        //        TBL_BP_DICTIONARY res = new TBL_BP_DICTIONARY();
        //        res.BP_YEAR = rec.DictVM.BP_YEAR;
        //        res.JOB_NO = rec.DictVM.JOB_NO;
        //        res.SDATE = rec.DictVM.SDATE;
        //        res.FDATE = rec.DictVM.FDATE;
        //        res.JOB_TITLE = rec.DictVM.JOB_TITLE;
        //        res.JOB_VOWD = rec.DictVM.JOB_VOWD;
        //        res.DIVISION = rec.DictVM.DIVISION;
        //        res.BP_MANAGER = rec.DictVM.BP_MANAGER;

        //        if (rec.DictVM.CLIENT != null)
        //        {
        //            int cid = Convert.ToInt32(rec.DictVM.CLIENT);
        //            var client = this.cn.TBL_BP_CLIENT.FirstOrDefault(x => x.CLIENT == cid);
        //            res.CLIENT = client.ClientName;
        //        }
        //        else
        //        {
        //            var existingRecord = this.cn.TBL_BP_DICTIONARY
        //                .AsNoTracking()
        //                .Where(p => p.JOB_NO == jobno && p.BP_YEAR == year)
        //                .FirstOrDefault();

        //            if (existingRecord != null)
        //            {
        //                res.CLIENT = existingRecord.CLIENT;
        //            }
        //        }

        //        if (rec.DictVM.LOCATION != null)
        //        {
        //            int lid = Convert.ToInt32(rec.DictVM.LOCATION);
        //            var location = this.cn.TBL_BP_LOCATION.FirstOrDefault(x => x.ID == lid);
        //            res.LOCATION = location.Location;
        //        }
        //        else
        //        {
        //            var existingRecord = this.cn.TBL_BP_DICTIONARY
        //                .AsNoTracking()
        //                .Where(p => p.JOB_NO == jobno && p.BP_YEAR == year)
        //                .FirstOrDefault();

        //            if (existingRecord != null)
        //            {
        //                res.LOCATION = existingRecord.LOCATION;
        //            }
        //        }
        //        res.OT_PERCENT = rec.DictVM.OT_PERCENT;
        //        res.FUEL_PERCENT = rec.DictVM.FUEL_PERCENT;
        //        res.FOOD_STAFF = rec.DictVM.FOOD_STAFF;
        //        res.FOOD_WORKER = rec.DictVM.FOOD_WORKER;
        //        res.ModifiedOn = DateTime.Now;
        //        res.ModifiedBy = Convert.ToString(Session["UserId"]);
        //        this.cn.Entry(res).State = System.Data.Entity.EntityState.Modified;
        //        this.cn.SaveChanges();
        //        List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
        //        result = this.cn.TBL_BP_DICTIONARY.ToList();
        //        MultipleVM model = new MultipleVM();
        //        model.DictListVM = result;
        //        ModelState.Clear();
        //        return RedirectToAction("Index");
        //    }
        //    return View();
        //}


    }
}