using BusinessApplication.CustomFilter;
using BusinessApplication.Models;
using BusinessApplication.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;
using System.Web.UI.WebControls;

namespace BusinessApplication.Areas.ValueOfWorkDone.Controllers
{
    [SessionAuth]
  
    public class VOWDController : Controller
    {
        BusinessPlanEntities cn = new BusinessPlanEntities();
        protected override void OnAuthentication(AuthenticationContext filterContext)
        {
            if (Convert.ToString(filterContext.HttpContext.Session["UserId"]) == null)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        protected override void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            if (Convert.ToString(filterContext.HttpContext.Session["UserId"]) == null || filterContext.Result is HttpUnauthorizedResult)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "action", "Index" }, { "controller", "Login" } });
            }
        }
        [HttpGet]
        public ActionResult Index(int year, string job)
        {
            List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            ViewBag.Year = year;
            ViewBag.Job = job;
            TBL_MN_WORKDONE existingEntity = cn.TBL_MN_WORKDONE.FirstOrDefault(e => e.ITM_YEAR == year && e.JOB_NO.StartsWith(job));
            if (existingEntity == null)
            {
                var res = (from t in cn.TBL_BP_DICTIONARY
                           where t.BP_YEAR == year && t.JOB_NO.StartsWith(job)
                           select new IncomeVM
                           {
                               JobNo = t.JOB_NO,
                               ID="T",
                               Type="IN",
                               JAN = 0,
                               FEB = 0,
                               MAR = 0,
                               APR = 0,
                               MAY = 0,
                               JUN = 0,
                               JUL = 0,
                               AUG = 0,
                               SEP = 0,
                               OCT = 0,
                               NOV = 0,
                               DEC = 0
                           }).ToList();
                model.VInList = res.ToList();
            }
            if (existingEntity != null)
            {
                var Tres = (from a in cn.TBL_MN_WORKDONE
                            join b in cn.TBL_BP_DICTIONARY
                            on new { a.JOB_NO } equals new { b.JOB_NO }
                            where a.ITM_YEAR == year && a.JOB_NO.StartsWith(job) && a.ITM_TYPE == "IN" && a.ITM_ID == "T"
                            group new { a, b } by new
                            {
                                b.BP_YEAR,
                                b.JOB_NO,
                                a.ITM_ID,
                                a.ITM_TYPE
                            } into grp
                            select new IncomeVM
                            {
                                JobNo = grp.Key.JOB_NO,
                                ID = grp.Key.ITM_ID,
                                Type = grp.Key.ITM_TYPE,
                                JAN = grp.Sum(x => x.a != null && x.a.ITM_MONTH == 1 ? x.a.ITM_VALUE : 0),
                                FEB = grp.Sum(x => x.a != null && x.a.ITM_MONTH == 2 ? x.a.ITM_VALUE : 0),
                                MAR = grp.Sum(x => x.a != null && x.a.ITM_MONTH == 3 ? x.a.ITM_VALUE : 0),
                                APR = grp.Sum(x => x.a != null && x.a.ITM_MONTH == 4 ? x.a.ITM_VALUE : 0),
                                MAY = grp.Sum(x => x.a != null && x.a.ITM_MONTH == 5 ? x.a.ITM_VALUE : 0),
                                JUN = grp.Sum(x => x.a != null && x.a.ITM_MONTH == 6 ? x.a.ITM_VALUE : 0),
                                JUL = grp.Sum(x => x.a != null && x.a.ITM_MONTH == 7 ? x.a.ITM_VALUE : 0),
                                AUG = grp.Sum(x => x.a != null && x.a.ITM_MONTH == 8 ? x.a.ITM_VALUE : 0),
                                SEP = grp.Sum(x => x.a != null && x.a.ITM_MONTH == 9 ? x.a.ITM_VALUE : 0),
                                OCT = grp.Sum(x => x.a != null && x.a.ITM_MONTH == 10 ? x.a.ITM_VALUE : 0),
                                NOV = grp.Sum(x => x.a != null && x.a.ITM_MONTH == 11 ? x.a.ITM_VALUE : 0),
                                DEC = grp.Sum(x => x.a != null && x.a.ITM_MONTH == 12 ? x.a.ITM_VALUE : 0),
                                TOTAL = grp.Sum(x => x.a.ITM_MONTH >= 1 && x.a.ITM_MONTH <= 12 ? x.a.ITM_VALUE : 0)
                            }).GroupBy(x => x.JobNo).Select(g => g.FirstOrDefault()).ToList();
                model.InList = Tres.ToList();
            }
            model.DictListVM = result;
            return View(model);
        }
        [HttpPost]
        public ActionResult Index(int year, string jobNo, string job, List<IncomeVM> InList,List<IncomeVM> VInList)
        {

            using (var dbContext = new BusinessPlanEntities())
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {

                    if (VInList != null)
                    {
                        for (int index = 0; index < VInList.Count; index++)
                        {
                            var items = VInList[index];
                            decimal?[] monthValue = new decimal?[12];
                            monthValue[0] = items.JAN ?? 0;
                            monthValue[1] = items.FEB ?? 0;
                            monthValue[2] = items.MAR ?? 0;
                            monthValue[3] = items.APR ?? 0;
                            monthValue[4] = items.MAY ?? 0;
                            monthValue[5] = items.JUN ?? 0;
                            monthValue[6] = items.JUL ?? 0;
                            monthValue[7] = items.AUG ?? 0;
                            monthValue[8] = items.SEP ?? 0;
                            monthValue[9] = items.OCT ?? 0;
                            monthValue[10] = items.NOV ?? 0;
                            monthValue[11] = items.DEC ?? 0;
                            for (int j = 0; j < 12; j++)
                            {
                                int k = j + 1;
                                string sql = "MERGE INTO TBL_MN_WORKDONE " +
                                         "USING(VALUES(" + year + "," + items.JobNo + ",'" + items.ID + "','" + items.Type + "'," + k + "," + monthValue[j] + ")) AS source(ITM_YEAR, JOB_NO,  ITM_ID, ITM_TYPE, ITM_MONTH, ITM_VALUE) " +
                                         "ON TBL_MN_WORKDONE.ITM_YEAR = source.ITM_YEAR and TBL_MN_WORKDONE.JOB_NO = source.JOB_NO and TBL_MN_WORKDONE.ITM_ID = source.ITM_ID and TBL_MN_WORKDONE.ITM_TYPE = source.ITM_TYPE and TBL_MN_WORKDONE.ITM_MONTH = source.ITM_MONTH and TBL_MN_WORKDONE.ITM_VALUE = source.ITM_VALUE " +
                                         "WHEN NOT MATCHED THEN " +
                                         " INSERT(ITM_YEAR, JOB_NO,  ITM_ID, ITM_TYPE, ITM_MONTH, ITM_VALUE) " +
                                         " VALUES(source.ITM_YEAR, source.JOB_NO, source.ITM_ID, source.ITM_TYPE, source.ITM_MONTH, source.ITM_VALUE); ";
                                dbContext.Database.ExecuteSqlCommand(sql);
                            }
                        }
                    }

                    else
                    {
                        for (int index = 0; index < InList.Count; index++)
                        {
                            var items = InList[index];
                            decimal?[] monthValue = new decimal?[12];
                            monthValue[0] = items.JAN ?? 0;
                            monthValue[1] = items.FEB ?? 0;
                            monthValue[2] = items.MAR ?? 0;
                            monthValue[3] = items.APR ?? 0 ;
                            monthValue[4] = items.MAY ?? 0;
                            monthValue[5] = items.JUN ?? 0;
                            monthValue[6] = items.JUL ?? 0;
                            monthValue[7] = items.AUG ?? 0;
                            monthValue[8] = items.SEP ?? 0;
                            monthValue[9] = items.OCT ?? 0;
                            monthValue[10] = items.NOV ?? 0;
                            monthValue[11] = items.DEC ?? 0;
                            for (int j = 0; j < 12; j++)
                            {
                                int k = j + 1;

                                string sql = "MERGE INTO TBL_MN_WORKDONE AS target " +
                                             "USING (VALUES(" + year + ",'" + jobNo + "','" + items.ID + "','" + items.Type + "'," + k + "," + monthValue[j] + ")) AS source(ITM_YEAR, JOB_NO, ITM_ID, ITM_TYPE, ITM_MONTH, ITM_VALUE) " +
                                             "ON target.ITM_YEAR = source.ITM_YEAR AND target.JOB_NO = source.JOB_NO AND target.ITM_ID = source.ITM_ID AND target.ITM_TYPE = source.ITM_TYPE AND target.ITM_MONTH = source.ITM_MONTH " +
                                             "WHEN MATCHED THEN " +
                                             "UPDATE SET target.ITM_VALUE = source.ITM_VALUE " +
                                             "WHEN NOT MATCHED THEN " +
                                             " INSERT(ITM_YEAR, JOB_NO,  ITM_ID, ITM_TYPE, ITM_MONTH, ITM_VALUE) " +
                                             " VALUES(source.ITM_YEAR, source.JOB_NO, source.ITM_ID, source.ITM_TYPE, source.ITM_MONTH, source.ITM_VALUE); ";

                                dbContext.Database.ExecuteSqlCommand(sql);

                            }
                        }
                    }

                  

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine(ex.Message);
                    return RedirectToAction("Error", "Home");
                }
            }
            return RedirectToAction("Index", new { year = year, job = job, jobno = jobNo });
        }


        ////Edited by sohel with SP
        ////[HttpPost]
        ////public ActionResult Index(int year, string jobno, string job, List<IncomeVM> InList)
        ////{
        ////    using (var dbContext = new BusinessPlanEntities())
        ////    using (var transaction = dbContext.Database.BeginTransaction())
        ////    {
        ////        try
        ////        {
        ////            for (int index = 0; index < InList.Count; index++)
        ////            {
        ////                var items = InList[index];
        ////                decimal?[] monthValue = new decimal?[12];
        ////                monthValue[0] = items.JAN;
        ////                monthValue[1] = items.FEB;
        ////                monthValue[2] = items.MAR;
        ////                monthValue[3] = items.APR;
        ////                monthValue[4] = items.MAY;
        ////                monthValue[5] = items.JUN;
        ////                monthValue[6] = items.JUL;
        ////                monthValue[7] = items.AUG;
        ////                monthValue[8] = items.SEP;
        ////                monthValue[9] = items.OCT;
        ////                monthValue[10] = items.NOV;
        ////                monthValue[11] = items.DEC;
        ////                for (int j = 0; j < 12; j++)
        ////                {
        ////                    int k = j + 1;

        ////                    string sql = "MERGE INTO TBL_MN_WORKDONE AS target " +
        ////                                 "USING (VALUES(" + year + "," + jobno + ",'" + items.ID + "','" + items.Type + "'," + k + "," + monthValue[j] + ")) AS source(ITM_YEAR, JOB_NO, ITM_ID, ITM_TYPE, ITM_MONTH, ITM_VALUE) " +
        ////                                 "ON target.ITM_YEAR = source.ITM_YEAR AND target.JOB_NO = source.JOB_NO AND target.ITM_ID = source.ITM_ID AND target.ITM_TYPE = source.ITM_TYPE AND target.ITM_MONTH = source.ITM_MONTH " +
        ////                                 "WHEN MATCHED THEN " +
        ////                                 "UPDATE SET target.ITM_VALUE = source.ITM_VALUE " +
        ////                                 "WHEN NOT MATCHED THEN " +
        ////                                 " INSERT(ITM_YEAR, JOB_NO,  ITM_ID, ITM_TYPE, ITM_MONTH, ITM_VALUE) " +
        ////                                 " VALUES(source.ITM_YEAR, source.JOB_NO, source.ITM_ID, source.ITM_TYPE, source.ITM_MONTH, source.ITM_VALUE); ";

        ////                    dbContext.Database.ExecuteSqlCommand(sql);
        ////                }

        ////                // Call the stored procedure after updating TBL_MN_WORKDONE
        ////                dbContext.Database.SqlQuery<object>("EXEC PROC_MN_UPDATE_WORKDONE @JOB_NO, @ITM_YEAR, @ITM_MONTH",
        ////                    new SqlParameter("@JOB_NO", jobno),
        ////                    new SqlParameter("@ITM_YEAR", year),
        ////                    new SqlParameter("@ITM_MONTH", index + 1)
        ////                ).ToList();
        ////            }

        ////            transaction.Commit();
        ////        }
        ////        catch (Exception ex)
        ////        {
        ////            transaction.Rollback();
        ////            Console.WriteLine(ex.Message);
        ////            return RedirectToAction("Error", "Home");
        ////        }
        ////    }
        ////    return RedirectToAction("Index", new { year = year, job = job, jobno = jobno });
        ////}




        [HttpGet]
        public ActionResult GetExpandedData(int year, string jobNo)
        {
            try
            {
                MultipleVM model = new MultipleVM();
                var groupedData = (from a in cn.TBL_MN_WORKDONE
                                   where a.JOB_NO == jobNo &&
                                         a.ITM_TYPE == "IN" &&
                                         a.ITM_YEAR == year &&
                                         (a.ITM_ID == "O" || a.ITM_ID == "L")
                                   group a by new { a.ITM_ID, a.ITM_YEAR, a.JOB_NO } into grp1
                                   select new IncomeVM
                                   {
                                       ID = grp1.Key.ITM_ID,
                                       JAN = grp1.Sum(x => x.ITM_MONTH == 1 ? x.ITM_VALUE : 0),
                                       FEB = grp1.Sum(x => x.ITM_MONTH == 2 ? x.ITM_VALUE : 0),
                                       MAR = grp1.Sum(x => x.ITM_MONTH == 3 ? x.ITM_VALUE : 0),
                                       APR = grp1.Sum(x => x.ITM_MONTH == 4 ? x.ITM_VALUE : 0),
                                       MAY = grp1.Sum(x => x.ITM_MONTH == 5 ? x.ITM_VALUE : 0),
                                       JUN = grp1.Sum(x => x.ITM_MONTH == 6 ? x.ITM_VALUE : 0),
                                       JUL = grp1.Sum(x => x.ITM_MONTH == 7 ? x.ITM_VALUE : 0),
                                       AUG = grp1.Sum(x => x.ITM_MONTH == 8 ? x.ITM_VALUE : 0),
                                       SEP = grp1.Sum(x => x.ITM_MONTH == 9 ? x.ITM_VALUE : 0),
                                       OCT = grp1.Sum(x => x.ITM_MONTH == 10 ? x.ITM_VALUE : 0),
                                       NOV = grp1.Sum(x => x.ITM_MONTH == 11 ? x.ITM_VALUE : 0),
                                       DEC = grp1.Sum(x => x.ITM_MONTH == 12 ? x.ITM_VALUE : 0),
                                       TOTAL = grp1.Sum(x => x.ITM_MONTH >= 1 && x.ITM_MONTH <= 12 ? x.ITM_VALUE : 0)
                                   }).ToList();

                var rec = groupedData.ToList();
                model.IncomeList = rec.ToList();
                return Json(new { success = true, data = model.IncomeList }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { success = false, message = "Error fetching data" }, JsonRequestBehavior.AllowGet);
            }
          
        }
    }
}