using BusinessApplication.CustomFilter;
using BusinessApplication.Models;
using BusinessApplication.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;

namespace BusinessApplication.Areas.Actuals.Controllers
{
    [SessionAuth]
    public class ActualController : Controller
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
        // GET: Actuals/Actual
        [HttpGet]
        public ActionResult Index(int year, string jobno,string PORA)
        {
            ViewBag.Year = year;
            ViewBag.JobNo = jobno;
            List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            TBL_MN_ACTUALS existingEntity = cn.TBL_MN_ACTUALS.FirstOrDefault(e => e.ITM_YEAR == year && e.JOB_NO == jobno);
            string User = Session["UserId"].ToString();

            if (existingEntity == null)
            {
                     var record = (from a in cn.TBL_MN_HEADS
                              select new IncomeVM
                              {
                                  ID = a.ITM_ID,
                                  DESCRIPTION = a.ITM_DESCRIPTION,
                                  Type = a.ITM_TYPE,

                              }).ToList();
                model.AInList = record.ToList();
            }
            else
            {
                //using (var dbContext = new BusinessPlanEntities())
                //{
                //    var storedProcedureResult = dbContext.Database.SqlQuery<IncomeVM>(
                //        "EXEC PROC_MN_SHOW_PORA @PORA, @JOB_NO, @ITM_YEAR",
                //        new SqlParameter("@PORA", "actuals"),
                //        new SqlParameter("@JOB_NO", jobno),
                //        new SqlParameter("@ITM_YEAR", year)
                //    ).ToList();

                //    model.InList = storedProcedureResult;
                //}
                var res = (from resource in cn.TBL_MN_HEADS
                           join actuals in cn.TBL_MN_ACTUALS
                           on new {  Id = resource.ITM_ID} equals new {  Id = actuals.ITM_ID }
                           where actuals.ITM_YEAR == year && actuals.JOB_NO == jobno
                           group new { resource, actuals } by new
                           {
                               resource.ITM_ID,
                               resource.ITM_DESCRIPTION,
                               actuals.ITM_TYPE
                           } into grp
                           orderby grp.Key.ITM_TYPE
                           select new IncomeVM
                           {
                               ID = grp.Key.ITM_ID,
                               Type = grp.Key.ITM_TYPE,
                               DESCRIPTION = grp.Key.ITM_DESCRIPTION,
                               JAN = grp.Sum(x => x.actuals.ITM_MONTH == 1 ? x.actuals.ITM_VALUE : 0),
                               FEB = grp.Sum(x => x.actuals.ITM_MONTH == 2 ? x.actuals.ITM_VALUE : 0),
                               MAR = grp.Sum(x => x.actuals.ITM_MONTH == 3 ? x.actuals.ITM_VALUE : 0),
                               APR = grp.Sum(x => x.actuals.ITM_MONTH == 4 ? x.actuals.ITM_VALUE : 0),
                               MAY = grp.Sum(x => x.actuals.ITM_MONTH == 5 ? x.actuals.ITM_VALUE : 0),
                               JUN = grp.Sum(x => x.actuals.ITM_MONTH == 6 ? x.actuals.ITM_VALUE : 0),
                               JUL = grp.Sum(x => x.actuals.ITM_MONTH == 7 ? x.actuals.ITM_VALUE : 0),
                               AUG = grp.Sum(x => x.actuals.ITM_MONTH == 8 ? x.actuals.ITM_VALUE : 0),
                               SEP = grp.Sum(x => x.actuals.ITM_MONTH == 9 ? x.actuals.ITM_VALUE : 0),
                               OCT = grp.Sum(x => x.actuals.ITM_MONTH == 10 ? x.actuals.ITM_VALUE : 0),
                               NOV = grp.Sum(x => x.actuals.ITM_MONTH == 11 ? x.actuals.ITM_VALUE : 0),
                               DEC = grp.Sum(x => x.actuals.ITM_MONTH == 12 ? x.actuals.ITM_VALUE : 0),
                               TOTAL = grp.Sum(x => x.actuals.ITM_MONTH >= 1 && x.actuals.ITM_MONTH <= 12 ? x.actuals.ITM_VALUE : 0)
                           }).ToList();
            

                 model.InList = res.ToList();
            }
            model.DictListVM = result;
            return View(model);
        }

        ////Old Edited 

        [HttpPost]
        public ActionResult Index(int year, string jobno, List<IncomeVM> AInList, List<IncomeVM> InList)
        {

            using (var dbContext = new BusinessPlanEntities())
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    if (AInList != null)
                    {
                        for (int index = 0; index < AInList.Count; index++)
                        {
                            var items = AInList[index];
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
                                string sql = "MERGE INTO TBL_MN_ACTUALS " +
                                         "USING(VALUES(" + year + "," + jobno + ",'" + items.ID + "','" + items.Type + "'," + k + "," + monthValue[j] + ")) AS source(ITM_YEAR, JOB_NO,  ITM_ID, ITM_TYPE, ITM_MONTH, ITM_VALUE) " +
                                         "ON TBL_MN_ACTUALS.ITM_YEAR = source.ITM_YEAR and TBL_MN_ACTUALS.JOB_NO = source.JOB_NO and TBL_MN_ACTUALS.ITM_ID = source.ITM_ID and TBL_MN_ACTUALS.ITM_TYPE = source.ITM_TYPE and TBL_MN_ACTUALS.ITM_MONTH = source.ITM_MONTH and TBL_MN_ACTUALS.ITM_VALUE = source.ITM_VALUE " +
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

                                string sql = "MERGE INTO TBL_MN_ACTUALS AS target " +
                                             "USING (VALUES(" + year + ",'" + jobno + "','" + items.ID + "','" + items.Type + "'," + k + "," + monthValue[j] + ")) AS source(ITM_YEAR, JOB_NO, ITM_ID, ITM_TYPE, ITM_MONTH, ITM_VALUE) " +
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
            return RedirectToAction("Index", new { year = year, jobno = jobno });
        }


        /////Edited by Sohel with SP and JSON


        //[HttpPost]
        //public ActionResult Index(int year, string jobno, List<IncomeVM> AInList, List<IncomeVM> InList)
        //{
        //    using (var dbContext = new BusinessPlanEntities())
        //    using (var transaction = dbContext.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var incomeList = AInList ?? InList;

        //            foreach (var item in incomeList)
        //            {
        //                var monthValues = new Dictionary<int, decimal?>
        //            {
        //                { 1, item.JAN },
        //                { 2, item.FEB },
        //                { 3, item.MAR },
        //                { 4, item.APR },
        //                { 5, item.MAY },
        //                { 6, item.JUN },
        //                { 7, item.JUL },
        //                { 8, item.AUG },
        //                { 9, item.SEP },
        //                { 10, item.OCT },
        //                { 11, item.NOV },
        //                { 12, item.DEC }
        //            };

        //                var jsonMonthValues = JsonConvert.SerializeObject(monthValues);

        //                // Update the database using Entity Framework
        //                dbContext.Database.ExecuteSqlCommand(
        //                    "EXEC PROC_MN_UPDATE_WORKDONE_ @JOB_NO, @ITM_YEAR, @MONTH_VALUES",
        //                    new SqlParameter("@JOB_NO", jobno),
        //                    new SqlParameter("@ITM_YEAR", year),
        //                    new SqlParameter("@MONTH_VALUES", SqlDbType.NVarChar) { Value = jsonMonthValues }
        //                );
        //            }

        //            transaction.Commit();
        //        }
        //        catch (Exception ex)
        //        {
        //            transaction.Rollback();
        //            Console.WriteLine(ex.Message);
        //            return RedirectToAction("Error", "Home");
        //        }
        //    }
        //    return RedirectToAction("Index", new { year = year, jobno = jobno });
        //}
    








    //public ActionResult Index(int year, string jobno, List<IncomeVM> AInList)
    //{

    //    using (var dbContext = new BusinessPlanEntities())
    //    using (var transaction = dbContext.Database.BeginTransaction())
    //    {
    //        try
    //        {
    //            for (int index = 0; index < AInList.Count; index++)
    //            {
    //                var items = AInList[index];
    //                decimal?[] monthValue = new decimal?[12];
    //                monthValue[0] = items.JAN  ;
    //                monthValue[1] = items.FEB  ;
    //                monthValue[2] = items.MAR  ;
    //                monthValue[3] = items.APR  ;
    //                monthValue[4] = items.MAY  ;
    //                monthValue[5] = items.JUN  ;
    //                monthValue[6] = items.JUL  ;
    //                monthValue[7] = items.AUG  ;
    //                monthValue[8] = items.SEP  ;
    //                monthValue[9] = items.OCT  ;
    //                monthValue[10] = items.NOV  ;
    //                monthValue[11] = items.DEC  ;
    //                for (int j = 0; j < 12; j++)
    //                {
    //                    int k = j + 1;
    //                    if (monthValue[j] != null)
    //                    {
    //                        string sql = "MERGE INTO TBL_MN_ACTUALS " +
    //                             "USING(VALUES(" + year + "," + jobno + ",'" + items.ID + "','" + items.Type + "'," + k + "," + monthValue[j] + ")) AS source(ITM_YEAR, JOB_NO,  ITM_ID, ITM_TYPE, ITM_MONTH, ITM_VALUE) " +
    //                             "ON TBL_MN_ACTUALS.ITM_YEAR = source.ITM_YEAR and TBL_MN_ACTUALS.JOB_NO = source.JOB_NO and TBL_MN_ACTUALS.ITM_ID = source.ITM_ID and TBL_MN_ACTUALS.ITM_TYPE = source.ITM_TYPE and TBL_MN_ACTUALS.ITM_MONTH = source.ITM_MONTH and TBL_MN_ACTUALS.ITM_VALUE = source.ITM_VALUE " +
    //                             "WHEN MATCHED THEN " +
    //                             " UPDATE SET ITM_YEAR = source.ITM_YEAR, JOB_NO = source.JOB_NO,ITM_ID = source.ITM_ID, ITM_TYPE = source.ITM_TYPE,ITM_MONTH= source.ITM_MONTH,ITM_VALUE=source.ITM_VALUE " +
    //                             "WHEN NOT MATCHED THEN " +
    //                             " INSERT(ITM_YEAR, JOB_NO,  ITM_ID, ITM_TYPE, ITM_MONTH, ITM_VALUE) " +
    //                             " VALUES(source.ITM_YEAR, source.JOB_NO, source.ITM_ID, source.ITM_TYPE, source.ITM_MONTH, source.ITM_VALUE); ";
    //                        dbContext.Database.ExecuteSqlCommand(sql);
    //                    }

    //                }
    //            }
    //            transaction.Commit();
    //        }
    //        catch (Exception ex)
    //        {
    //            transaction.Rollback();
    //            Console.WriteLine(ex.Message);
    //            return RedirectToAction("Error", "Home");
    //        }
    //    }
    //    return RedirectToAction("Index", new { year = year, jobno = jobno });
    //}
}
}
