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

namespace BusinessApplication.Controllers
{
    [SessionAuth]
    public class YearController : Controller
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
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Income(int year, string jobno)
        {
            ViewBag.Year = year;
            ViewBag.JobNo = jobno;
            List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            TBL_BP_RESOURCE_ existingEntity = cn.TBL_BP_RESOURCE_.FirstOrDefault(e => e.BP_YEAR == year && e.JOB_NO == jobno && e.RESO_TYPE == "IN");

            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }
            else
            {
                if (existingEntity == null)
                {
                    var res = (from a in cn.TBL_BP_RESOURCE
                               where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "IN"
                               select new IncomeVM
                               {
                                   ID = a.RESO_ID,
                                   DESCRIPTION = a.RESO_DESCRIPTION,
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
                    model.InList = res.ToList();
                }
                else
                {
                    var res = (from a in cn.TBL_BP_RESOURCE
                               join b in cn.TBL_BP_RESOURCE_
                               on new { a.BP_YEAR, a.JOB_NO, a.RESO_ID, a.RESO_TYPE } equals new { b.BP_YEAR, b.JOB_NO, b.RESO_ID, b.RESO_TYPE } into joinedData
                               from ab in joinedData.DefaultIfEmpty()
                               where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "IN"
                               group new { a, ab } by new
                               {
                                   a.RESO_ID,
                                   a.RESO_DESCRIPTION,
                               } into grp
                               select new IncomeVM
                               {
                                   ID = grp.Key.RESO_ID,
                                   DESCRIPTION = grp.Key.RESO_DESCRIPTION,
                                   JAN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 1 ? x.ab.RESO_QTY : 0),
                                   FEB = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 2 ? x.ab.RESO_QTY : 0),
                                   MAR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 3 ? x.ab.RESO_QTY : 0),
                                   APR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 4 ? x.ab.RESO_QTY : 0),
                                   MAY = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 5 ? x.ab.RESO_QTY : 0),
                                   JUN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 6 ? x.ab.RESO_QTY : 0),
                                   JUL = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 7 ? x.ab.RESO_QTY : 0),
                                   AUG = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 8 ? x.ab.RESO_QTY : 0),
                                   SEP = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 9 ? x.ab.RESO_QTY : 0),
                                   OCT = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 10 ? x.ab.RESO_QTY : 0),
                                   NOV = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 11 ? x.ab.RESO_QTY : 0),
                                   DEC = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 12 ? x.ab.RESO_QTY : 0)
                               }).ToList();

                    model.InList = res.ToList();
                }
                model.DictListVM = result;
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult Income(int year, string jobno, MultipleVM rec)
        {
            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_WRITE == 1 && x.BP_USER == User).ToList();

            //if (check.Count() == 0)
            //{
            //    return RedirectToAction("AccessDenied", "UserAuthorization");
            //}

            using (var transaction = this.cn.Database.BeginTransaction())
            {
                try
                {
                    foreach (var data in rec.InList)
                    {
                        for (int month = 1; month <= 12; month++)
                        {
                            decimal? monthValue = null;

                            switch (month)
                            {
                                case 1:
                                    monthValue = data.JAN;
                                    break;
                                case 2:
                                    monthValue = data.FEB;
                                    break;
                                case 3:
                                    monthValue = data.MAR;
                                    break;
                                case 4:
                                    monthValue = data.APR;
                                    break;
                                case 5:
                                    monthValue = data.MAY;
                                    break;
                                case 6:
                                    monthValue = data.JUN;
                                    break;
                                case 7:
                                    monthValue = data.JUL;
                                    break;
                                case 8:
                                    monthValue = data.AUG;
                                    break;
                                case 9:
                                    monthValue = data.SEP;
                                    break;
                                case 10:
                                    monthValue = data.OCT;
                                    break;
                                case 11:
                                    monthValue = data.NOV;
                                    break;
                                case 12:
                                    monthValue = data.DEC;
                                    break;
                            }

                            TBL_BP_RESOURCE_ existingEntity = cn.TBL_BP_RESOURCE_
                                .SingleOrDefault(e => e.BP_YEAR == year && e.JOB_NO == jobno && e.BP_MONTH == month && e.RESO_ID == data.ID);

                            if (existingEntity == null)
                            {
                                TBL_BP_RESOURCE_ newEntity = new TBL_BP_RESOURCE_
                                {
                                    BP_MONTH = month,
                                    JOB_NO = jobno,
                                    BP_YEAR = year,
                                    RESO_ID = data.ID,
                                    RESO_TYPE = "IN",
                                    RESO_QTY = monthValue
                                };

                                this.cn.TBL_BP_RESOURCE_.Add(newEntity);
                            }
                            else
                            {
                                existingEntity.RESO_QTY = monthValue;
                            }
                        }
                    }

                    this.cn.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            }

            List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            var record = (from a in cn.TBL_BP_RESOURCE
                          join b in cn.TBL_BP_RESOURCE_
                          on new { a.BP_YEAR, a.JOB_NO, a.RESO_ID, a.RESO_TYPE } equals new { b.BP_YEAR, b.JOB_NO, b.RESO_ID, b.RESO_TYPE }
                          where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "IN"
                          group new { a, b } by new
                          {
                              a.RESO_ID,
                              a.RESO_DESCRIPTION,
                          } into grp
                          select new IncomeVM
                          {
                              ID = grp.Key.RESO_ID,
                              DESCRIPTION = grp.Key.RESO_DESCRIPTION,
                              JAN = grp.Sum(x => x.b.BP_MONTH == 1 ? x.b.RESO_QTY : 0),
                              FEB = grp.Sum(x => x.b.BP_MONTH == 2 ? x.b.RESO_QTY : 0),
                              MAR = grp.Sum(x => x.b.BP_MONTH == 3 ? x.b.RESO_QTY : 0),
                              APR = grp.Sum(x => x.b.BP_MONTH == 4 ? x.b.RESO_QTY : 0),
                              MAY = grp.Sum(x => x.b.BP_MONTH == 5 ? x.b.RESO_QTY : 0),
                              JUN = grp.Sum(x => x.b.BP_MONTH == 6 ? x.b.RESO_QTY : 0),
                              JUL = grp.Sum(x => x.b.BP_MONTH == 7 ? x.b.RESO_QTY : 0),
                              AUG = grp.Sum(x => x.b.BP_MONTH == 8 ? x.b.RESO_QTY : 0),
                              SEP = grp.Sum(x => x.b.BP_MONTH == 9 ? x.b.RESO_QTY : 0),
                              OCT = grp.Sum(x => x.b.BP_MONTH == 10 ? x.b.RESO_QTY : 0),
                              NOV = grp.Sum(x => x.b.BP_MONTH == 11 ? x.b.RESO_QTY : 0),
                              DEC = grp.Sum(x => x.b.BP_MONTH == 12 ? x.b.RESO_QTY : 0)
                          }).ToList();

            model.InList = record.ToList();
            model.DictListVM = result;
            return View(model);
        }

        [HttpGet]
        public ActionResult MaterialInHouse(int year, string jobno)
        {
            ViewBag.Year = year;
            ViewBag.JobNo = jobno;
            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }

            ViewBag.GRP = new SelectList(this.cn.TBL_RESOURCE_GLOBAL.Select(x => new SelectListItem
            {
                Text = x.RESO_ID + "-" + x.RESO_TITLE,
                Value = x.RESO_ID
            }).Distinct().ToList(), "Value", "Text");
            List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            //model.MaterialsIH = cn.PROC_BP_SHOW_RESOURCE(year, jobno, "MB");
            var record = (from a in cn.TBL_BP_RESOURCE
                          join b in cn.TBL_BP_RESOURCE_
                          on new { a.BP_YEAR, a.JOB_NO, a.RESO_ID, a.RESO_TYPE } equals new { b.BP_YEAR, b.JOB_NO, b.RESO_ID, b.RESO_TYPE } into joinedData
                          from ab in joinedData.DefaultIfEmpty()
                          where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "IH"
                          group new { a, ab } by new
                          {
                              a.RESO_GROUP,
                              a.RESO_ID,
                              a.RESO_DESCRIPTION,
                              a.RESO_UNIT,
                              a.RESO_RATE,
                              a.RESO_EQTY
                          } into grp
                          select new IncomeVM
                          {
                              GRP = grp.Key.RESO_GROUP,
                              ID = grp.Key.RESO_ID,
                              DESCRIPTION = grp.Key.RESO_DESCRIPTION,
                              Unit = grp.Key.RESO_UNIT,
                              Rate = grp.Key.RESO_RATE,
                              EST__QTY = grp.Key.RESO_EQTY,
                              //Amount = grp.Sum(x => x.a != null ? x.a.RESO_UNIT * x.a.RESO_RATE : 0),
                              JAN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 1 ? x.ab.RESO_QTY : 0),
                              FEB = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 2 ? x.ab.RESO_QTY : 0),
                              MAR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 3 ? x.ab.RESO_QTY : 0),
                              APR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 4 ? x.ab.RESO_QTY : 0),
                              MAY = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 5 ? x.ab.RESO_QTY : 0),
                              JUN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 6 ? x.ab.RESO_QTY : 0),
                              JUL = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 7 ? x.ab.RESO_QTY : 0),
                              AUG = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 8 ? x.ab.RESO_QTY : 0),
                              SEP = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 9 ? x.ab.RESO_QTY : 0),
                              OCT = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 10 ? x.ab.RESO_QTY : 0),
                              NOV = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 11 ? x.ab.RESO_QTY : 0),
                              DEC = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 12 ? x.ab.RESO_QTY : 0)
                          }).ToList();

            model.InList = record.ToList();
            model.DictListVM = result;
            return View(model);
        }


        [HttpPost]
        public ActionResult MaterialInHouse(int year, string jobno, MultipleVM rec, List<IncomeVM> InList)
        {
            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_WRITE == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }
            ViewBag.GRP = new SelectList(this.cn.TBL_RESOURCE_GLOBAL.Select(x => new SelectListItem
            {
                Text = x.RESO_ID + "-" + x.RESO_TITLE,
                Value = x.RESO_ID
            }).Distinct().ToList(), "Value", "Text", InList.Select(x => x.GRP));
            int i = 1;
            using (var dbContext = new BusinessPlanEntities())
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    for (int index = 0; index < InList.Count; index++)
                    {
                        var items = InList[index];
                        decimal[] monthValue = new decimal[12];
                        decimal? rate = items.Rate ?? 0;
                        decimal? qty = items.EST__QTY ?? 0;
                        string grp = items.GRP ?? "NULL";

                        string sql = "MERGE INTO TBL_BP_RESOURCE " +
                                     "USING(VALUES(" + year + "," + jobno + "," + grp + ",'" + items.ID + "','IH','" + items.DESCRIPTION + "','" + items.Unit + "'," + rate + "," + qty + "," + i + ")) AS source(BP_YEAR, JOB_NO,  RESO_GROUP, RESO_ID, RESO_TYPE, RESO_DESCRIPTION,	RESO_UNIT,	RESO_RATE,	RESO_EQTY,	ROW_ID) " +
                                     "ON TBL_BP_RESOURCE.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE.RESO_TYPE = source.RESO_TYPE " +
                                     "WHEN MATCHED THEN " +
                                     " UPDATE SET BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO, RESO_GROUP = source.RESO_GROUP, RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_DESCRIPTION = source.RESO_DESCRIPTION, RESO_UNIT = source.RESO_UNIT, RESO_RATE = source.RESO_RATE, RESO_EQTY = source.RESO_EQTY, ROW_ID = source.ROW_ID " +
                                     "WHEN NOT MATCHED THEN " +
                                     " INSERT(BP_YEAR, JOB_NO, RESO_GROUP, RESO_ID, RESO_TYPE, RESO_DESCRIPTION, RESO_UNIT, RESO_RATE, RESO_EQTY, ROW_ID) " +
                                     " VALUES(source.BP_YEAR, source.JOB_NO, source.RESO_GROUP, source.RESO_ID, source.RESO_TYPE, source.RESO_DESCRIPTION, source.RESO_UNIT, source.RESO_RATE, source.RESO_EQTY,source.ROW_ID); ";
                        dbContext.Database.ExecuteSqlCommand(sql);

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
                            sql = "MERGE INTO TBL_BP_RESOURCE_ " +
                                         "USING(VALUES(" + k + "," + year + "," + jobno + ",'" + items.ID + "','IH'," + monthValue[j] + ")) AS source(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                         "ON TBL_BP_RESOURCE_.BP_MONTH = source.BP_MONTH and TBL_BP_RESOURCE_.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE_.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE_.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE_.RESO_TYPE = source.RESO_TYPE " +
                                         "WHEN MATCHED THEN " +
                                         " UPDATE SET BP_MONTH = source.BP_MONTH,	BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO, RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_QTY = source.RESO_QTY " +
                                         "WHEN NOT MATCHED THEN " +
                                         " INSERT(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                         " VALUES(source.BP_MONTH,	source.BP_YEAR, source.JOB_NO, source.RESO_ID, source.RESO_TYPE, source.RESO_QTY); ";

                            dbContext.Database.ExecuteSqlCommand(sql);
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
            return RedirectToAction("MaterialInHouse", new { year = year, jobno = jobno });
        }


        [HttpGet]
        public ActionResult MaterialBoughtOut(int year, string jobno)
        {
            ViewBag.Year = year;
            ViewBag.JobNo = jobno;
            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }

            ViewBag.GRP = new SelectList(this.cn.TBL_RESOURCE_GLOBAL.Select(x => new SelectListItem
            {
                Text = x.RESO_ID + "-" + x.RESO_TITLE,
                Value = x.RESO_ID
            }).Distinct().ToList(), "Value", "Text");
            List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            //model.MaterialsIH = cn.PROC_BP_SHOW_RESOURCE(year, jobno, "MB");
            var record = (from a in cn.TBL_BP_RESOURCE
                          join b in cn.TBL_BP_RESOURCE_
                          on new { a.BP_YEAR, a.JOB_NO, a.RESO_ID, a.RESO_TYPE } equals new { b.BP_YEAR, b.JOB_NO, b.RESO_ID, b.RESO_TYPE } into joinedData
                          from ab in joinedData.DefaultIfEmpty()
                          where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "BO"
                          group new { a, ab } by new
                          {
                              a.RESO_GROUP,
                              a.RESO_ID,
                              a.RESO_DESCRIPTION,
                              a.RESO_UNIT,
                              a.RESO_RATE,
                              a.RESO_EQTY
                          } into grp
                          select new IncomeVM
                          {
                              GRP = grp.Key.RESO_GROUP,
                              ID = grp.Key.RESO_ID,
                              DESCRIPTION = grp.Key.RESO_DESCRIPTION,
                              Unit = grp.Key.RESO_UNIT,
                              Rate = grp.Key.RESO_RATE,
                              EST__QTY = grp.Key.RESO_EQTY,
                              //Amount = grp.Sum(x => x.a != null ? x.a.RESO_UNIT * x.a.RESO_RATE : 0),
                              JAN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 1 ? x.ab.RESO_QTY : 0),
                              FEB = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 2 ? x.ab.RESO_QTY : 0),
                              MAR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 3 ? x.ab.RESO_QTY : 0),
                              APR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 4 ? x.ab.RESO_QTY : 0),
                              MAY = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 5 ? x.ab.RESO_QTY : 0),
                              JUN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 6 ? x.ab.RESO_QTY : 0),
                              JUL = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 7 ? x.ab.RESO_QTY : 0),
                              AUG = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 8 ? x.ab.RESO_QTY : 0),
                              SEP = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 9 ? x.ab.RESO_QTY : 0),
                              OCT = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 10 ? x.ab.RESO_QTY : 0),
                              NOV = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 11 ? x.ab.RESO_QTY : 0),
                              DEC = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 12 ? x.ab.RESO_QTY : 0)
                          }).ToList();

            model.InList = record.ToList();
            model.DictListVM = result;
            return View(model);
        }

        [HttpPost]
        public ActionResult MaterialBoughtOut(int year, string jobno, MultipleVM rec, List<IncomeVM> InList)
        {
            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_WRITE == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }
            ViewBag.GRP = new SelectList(this.cn.TBL_RESOURCE_GLOBAL.Select(x => new SelectListItem
            {
                Text = x.RESO_ID + "-" + x.RESO_TITLE,
                Value = x.RESO_ID.ToString() 
            }).Distinct().ToList(), "Value", "Text", InList.Select(x => x.GRP));
            int i = 1;
            using (var dbContext = new BusinessPlanEntities())
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    for (int index = 0; index < InList.Count; index++)
                    {
                        var items = InList[index];
                        decimal[] monthValue = new decimal[12];
                        decimal? rate = items.Rate ?? 0;
                        decimal? qty = items.EST__QTY ?? 0;
                        string grp = items.GRP ?? "NULL"; 

                        string sql = "MERGE INTO TBL_BP_RESOURCE " +
                                     "USING(VALUES(" + year + "," + jobno + "," + grp + ",'" + items.ID + "','BO','" + items.DESCRIPTION + "','" + items.Unit + "'," + rate + "," + qty + "," + i + ")) AS source(BP_YEAR, JOB_NO,  RESO_GROUP, RESO_ID, RESO_TYPE, RESO_DESCRIPTION,	RESO_UNIT,	RESO_RATE,	RESO_EQTY,	ROW_ID) " +
                                     "ON TBL_BP_RESOURCE.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE.RESO_TYPE = source.RESO_TYPE " +
                                     "WHEN MATCHED THEN " +
                                     " UPDATE SET BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO, RESO_GROUP = source.RESO_GROUP, RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_DESCRIPTION = source.RESO_DESCRIPTION, RESO_UNIT = source.RESO_UNIT, RESO_RATE = source.RESO_RATE, RESO_EQTY = source.RESO_EQTY, ROW_ID = source.ROW_ID " +
                                     "WHEN NOT MATCHED THEN " +
                                     " INSERT(BP_YEAR, JOB_NO, RESO_GROUP, RESO_ID, RESO_TYPE, RESO_DESCRIPTION, RESO_UNIT, RESO_RATE, RESO_EQTY, ROW_ID) " +
                                     " VALUES(source.BP_YEAR, source.JOB_NO, source.RESO_GROUP, source.RESO_ID, source.RESO_TYPE, source.RESO_DESCRIPTION, source.RESO_UNIT, source.RESO_RATE, source.RESO_EQTY,source.ROW_ID); ";
                        Console.WriteLine("Executing SQL query: " + sql);
                        dbContext.Database.ExecuteSqlCommand(sql);

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
                            sql = "MERGE INTO TBL_BP_RESOURCE_ " +
                                     "USING(VALUES(" + k + "," + year + "," + jobno + ",'" + items.ID + "','BO'," + monthValue[j] + ")) AS source(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                     "ON TBL_BP_RESOURCE_.BP_MONTH = source.BP_MONTH and TBL_BP_RESOURCE_.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE_.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE_.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE_.RESO_TYPE = source.RESO_TYPE " +
                                     "WHEN MATCHED THEN " +
                                     " UPDATE SET BP_MONTH = source.BP_MONTH,	BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO, RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_QTY = source.RESO_QTY " +
                                     "WHEN NOT MATCHED THEN " +
                                     " INSERT(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                     " VALUES(source.BP_MONTH,	source.BP_YEAR, source.JOB_NO, source.RESO_ID, source.RESO_TYPE, source.RESO_QTY); ";

                            dbContext.Database.ExecuteSqlCommand(sql);
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
            return RedirectToAction("MaterialBoughtOut", new { year = year, jobno = jobno });
        }

        [HttpGet]
        public ActionResult Manpower(int year, string jobno)
        {
            ViewBag.Year = year;
            ViewBag.JobNo = jobno;
            List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.GlobalList = cn.PROC_BP_SHOW_MASTER("MT").ToList();
            TBL_BP_RESOURCE_ existingEntity = cn.TBL_BP_RESOURCE_.FirstOrDefault(e => e.BP_YEAR == year && e.JOB_NO == jobno && e.RESO_TYPE == "MP");


            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }

            if (existingEntity == null)
            {
                var res = (from a in cn.TBL_BP_RESOURCE
                           where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "MP"
                           select new IncomeVM
                           {
                               ID = a.RESO_ID,
                               DESCRIPTION = a.RESO_DESCRIPTION,
                               Unit = a.RESO_UNIT,
                               Rate = a.RESO_RATE,
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
                model.InList = res.ToList();
            }
            else
            {
                //var res = (from a in cn.TBL_BP_RESOURCE
                //           join b in cn.TBL_BP_RESOURCE_
                //           on new { a.BP_YEAR, a.JOB_NO, a.RESO_ID, a.RESO_TYPE } equals new { b.BP_YEAR, b.JOB_NO, b.RESO_ID, b.RESO_TYPE }
                //           where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "MP"
                //           group new { a, b } by new
                //           {
                //               a.RESO_ID,
                //               a.RESO_DESCRIPTION,
                //               a.RESO_UNIT,
                //               a.RESO_RATE
                //           } into grp
                //           select new IncomeVM
                //           {
                //               ID = grp.Key.RESO_ID,
                //               DESCRIPTION = grp.Key.RESO_DESCRIPTION,
                //               Unit = grp.Key.RESO_UNIT,
                //               Rate = grp.Key.RESO_RATE,
                //               JAN = grp.Sum(x => x.b.BP_MONTH == 1 ? x.b.RESO_QTY : 0),
                //               FEB = grp.Sum(x => x.b.BP_MONTH == 2 ? x.b.RESO_QTY : 0),
                //               MAR = grp.Sum(x => x.b.BP_MONTH == 3 ? x.b.RESO_QTY : 0),
                //               APR = grp.Sum(x => x.b.BP_MONTH == 4 ? x.b.RESO_QTY : 0),
                //               MAY = grp.Sum(x => x.b.BP_MONTH == 5 ? x.b.RESO_QTY : 0),
                //               JUN = grp.Sum(x => x.b.BP_MONTH == 6 ? x.b.RESO_QTY : 0),
                //               JUL = grp.Sum(x => x.b.BP_MONTH == 7 ? x.b.RESO_QTY : 0),
                //               AUG = grp.Sum(x => x.b.BP_MONTH == 8 ? x.b.RESO_QTY : 0),
                //               SEP = grp.Sum(x => x.b.BP_MONTH == 9 ? x.b.RESO_QTY : 0),
                //               OCT = grp.Sum(x => x.b.BP_MONTH == 10 ? x.b.RESO_QTY : 0),
                //               NOV = grp.Sum(x => x.b.BP_MONTH == 11 ? x.b.RESO_QTY : 0),
                //               DEC = grp.Sum(x => x.b.BP_MONTH == 12 ? x.b.RESO_QTY : 0)
                //           }).ToList();
                var res = (from a in cn.TBL_BP_RESOURCE
                           join b in cn.TBL_BP_RESOURCE_
                           on new { a.BP_YEAR, a.JOB_NO, a.RESO_ID, a.RESO_TYPE } equals new { b.BP_YEAR, b.JOB_NO, b.RESO_ID, b.RESO_TYPE } into joinedData
                           from ab in joinedData.DefaultIfEmpty()
                           where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "MP"
                           group new { a, ab } by new
                           {
                               a.RESO_ID,
                               a.RESO_DESCRIPTION,
                               a.RESO_UNIT,
                               a.RESO_RATE
                           } into grp
                           select new IncomeVM
                           {
                               ID = grp.Key.RESO_ID,
                               DESCRIPTION = grp.Key.RESO_DESCRIPTION,
                               Unit = grp.Key.RESO_UNIT,
                               Rate = grp.Key.RESO_RATE,
                               JAN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 1 ? x.ab.RESO_QTY : 0),
                               FEB = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 2 ? x.ab.RESO_QTY : 0),
                               MAR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 3 ? x.ab.RESO_QTY : 0),
                               APR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 4 ? x.ab.RESO_QTY : 0),
                               MAY = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 5 ? x.ab.RESO_QTY : 0),
                               JUN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 6 ? x.ab.RESO_QTY : 0),
                               JUL = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 7 ? x.ab.RESO_QTY : 0),
                               AUG = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 8 ? x.ab.RESO_QTY : 0),
                               SEP = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 9 ? x.ab.RESO_QTY : 0),
                               OCT = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 10 ? x.ab.RESO_QTY : 0),
                               NOV = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 11 ? x.ab.RESO_QTY : 0),
                               DEC = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 12 ? x.ab.RESO_QTY : 0)
                           }).ToList();
                model.InList = res.ToList();
            }
            model.DictListVM = result;

            model.DictListVM = result;
            GobleData.mulVm = null;

            GobleData.mulVm = new MultipleVM()
            {
                UserTbl = model.UserTbl,
                DictVM = model.DictVM,
                InList = model.InList,
                DictListVM = model.DictListVM,
                MaterialIH = model.MaterialIH,
                MaterialsIH = model.MaterialsIH,
                ResourceTbl = model.ResourceTbl,
                BPResoTbl = model.BPResoTbl,
                ResoTbl = model.ResoTbl,
                BResoTbl = model.BResoTbl,
                ClientListVM = model.ClientListVM,
                LocListVM = model.LocListVM,
                GlobalList = model.GlobalList,
                PermissionList = model.PermissionList,
                PermList = model.PermList,
                ReviTbl = model.ReviTbl
            };
            GobleData.mulVm.chkPart = new List<bool>();
            for (int i = 0; i < GobleData.mulVm.GlobalList.Count(); i++)
                GobleData.mulVm.chkPart.Add(true);



            return View(model);
        }

       
        [HttpPost]
        public ActionResult Manpower(int year, string jobno, MultipleVM rec)
        {

            ViewBag.Year = year;
            ViewBag.JobNo = jobno;
            string User = Session["UserId"].ToString();
            var xxx = GobleData.mulVm;
            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_WRITE == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }
            int i = 1;

            using (var dbContext = new BusinessPlanEntities())
            using (var transaction = dbContext.Database.BeginTransaction())
            {

                try
                {
                    if (rec != null)
                    {
                        
                            for (int a = 0; a < rec.InList.Count; a++)
                                for (int b = 0; b < GobleData.mulVm.InList.Count; b++)
                                {
                                    decimal[] monthValue = new decimal[12];

                                    string sql = "MERGE INTO TBL_BP_RESOURCE " +
                                                 "USING(VALUES(" + year + "," + jobno + ",'" + GobleData.mulVm.InList[b].ID + "','MP','" + GobleData.mulVm.InList[b].DESCRIPTION + "','" + GobleData.mulVm.InList[b].Unit + "'," + GobleData.mulVm.InList[b].Rate + "," + i + ")) AS source(BP_YEAR, JOB_NO,   RESO_ID, RESO_TYPE, RESO_DESCRIPTION,	RESO_UNIT,	RESO_RATE,	ROW_ID) " +
                                                 "ON TBL_BP_RESOURCE.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE.RESO_TYPE = source.RESO_TYPE " +
                                                 "WHEN MATCHED THEN " +
                                                 " UPDATE SET BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO,  RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_DESCRIPTION = source.RESO_DESCRIPTION, RESO_UNIT = source.RESO_UNIT, RESO_RATE = source.RESO_RATE, ROW_ID = source.ROW_ID " +
                                                 "WHEN NOT MATCHED THEN " +
                                                 " INSERT(BP_YEAR, JOB_NO,  RESO_ID, RESO_TYPE, RESO_DESCRIPTION, RESO_UNIT, RESO_RATE, ROW_ID) " +
                                                 " VALUES(source.BP_YEAR, source.JOB_NO, source.RESO_ID, source.RESO_TYPE, source.RESO_DESCRIPTION, source.RESO_UNIT, source.RESO_RATE,source.ROW_ID); ";
                                    dbContext.Database.ExecuteSqlCommand(sql);
                                    monthValue[0] = rec.InList[a].JAN.Value;
                                    monthValue[1] = rec.InList[a].FEB.Value;
                                    monthValue[2] = rec.InList[a].MAR.Value;
                                    monthValue[3] = rec.InList[a].APR.Value;
                                    monthValue[4] = rec.InList[a].MAY.Value;
                                    monthValue[5] = rec.InList[a].JUN.Value;
                                    monthValue[6] = rec.InList[a].JUL.Value;
                                    monthValue[7] = rec.InList[a].AUG.Value;
                                    monthValue[8] = rec.InList[a].SEP.Value;
                                    monthValue[9] = rec.InList[a].OCT.Value;
                                    monthValue[10] = rec.InList[a].NOV.Value;
                                    monthValue[11] = rec.InList[a].DEC.Value;
                                    for (int j = 0; j < 12; j++)
                                    {
                                        int k = j + 1;
                                        sql = "MERGE INTO TBL_BP_RESOURCE_ " +
                                                     "USING(VALUES(" + k + "," + year + "," + jobno + ",'" + GobleData.mulVm.InList[a].ID + "','MP'," + monthValue[j] + ")) AS source(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                                     "ON TBL_BP_RESOURCE_.BP_MONTH = source.BP_MONTH and TBL_BP_RESOURCE_.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE_.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE_.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE_.RESO_TYPE = source.RESO_TYPE " +
                                                     "WHEN MATCHED THEN " +
                                                     " UPDATE SET BP_MONTH = source.BP_MONTH,	BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO, RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_QTY = source.RESO_QTY " +
                                                     "WHEN NOT MATCHED THEN " +
                                                     " INSERT(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                                     " VALUES(source.BP_MONTH,	source.BP_YEAR, source.JOB_NO, source.RESO_ID, source.RESO_TYPE, source.RESO_QTY); ";

                                        dbContext.Database.ExecuteSqlCommand(sql);
                                    }
                                }
                        
                    }
                    //else
                    //{
                    //    foreach (var items in GobleData.mulVm.InList)
                    //    {
                    //        decimal[] monthValue = new decimal[12];

                    //        decimal? qty = items.EST__QTY;
                    //        string grp = items.GRP;
                    //        if (items.GRP == null)
                    //            grp = "NULL";
                    //        if (items.EST__QTY == null)
                    //            qty = 0;
                    //        string sql = "MERGE INTO TBL_BP_RESOURCE " +
                    //                     "USING(VALUES(" + year + "," + jobno + "," + grp + ",'" + items.ID + "','MP','" + items.DESCRIPTION + "','" + items.Unit + "'," + items.Rate + "," + qty + "," + i + ")) AS source(BP_YEAR, JOB_NO,  RESO_GROUP, RESO_ID, RESO_TYPE, RESO_DESCRIPTION,	RESO_UNIT,	RESO_RATE,	RESO_EQTY,	ROW_ID) " +
                    //                     "ON TBL_BP_RESOURCE.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE.RESO_TYPE = source.RESO_TYPE " +
                    //                     "WHEN MATCHED THEN " +
                    //                     " UPDATE SET BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO, RESO_GROUP = source.RESO_GROUP, RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_DESCRIPTION = source.RESO_DESCRIPTION, RESO_UNIT = source.RESO_UNIT, RESO_RATE = source.RESO_RATE, RESO_EQTY = source.RESO_EQTY, ROW_ID = source.ROW_ID " +
                    //                     "WHEN NOT MATCHED THEN " +
                    //                     " INSERT(BP_YEAR, JOB_NO, RESO_GROUP, RESO_ID, RESO_TYPE, RESO_DESCRIPTION, RESO_UNIT, RESO_RATE, RESO_EQTY, ROW_ID) " +
                    //                     " VALUES(source.BP_YEAR, source.JOB_NO, source.RESO_GROUP, source.RESO_ID, source.RESO_TYPE, source.RESO_DESCRIPTION, source.RESO_UNIT, source.RESO_RATE, source.RESO_EQTY,source.ROW_ID); ";
                    //        dbContext.Database.ExecuteSqlCommand(sql);
                    //        monthValue[0] = items.JAN.Value;
                    //        monthValue[1] = items.FEB.Value;
                    //        monthValue[2] = items.MAR.Value;
                    //        monthValue[3] = items.APR.Value;
                    //        monthValue[4] = items.MAY.Value;
                    //        monthValue[5] = items.JUL.Value;
                    //        monthValue[6] = items.JUL.Value;
                    //        monthValue[7] = items.AUG.Value;
                    //        monthValue[8] = items.SEP.Value;
                    //        monthValue[9] = items.OCT.Value;
                    //        monthValue[10] = items.NOV.Value;
                    //        monthValue[11] = items.DEC.Value;
                    //        for (int j = 0; j < 12; j++)
                    //        {
                    //            int k = j + 1;
                    //            sql = "MERGE INTO TBL_BP_RESOURCE_ " +
                    //                         "USING(VALUES(" + k + "," + year + "," + jobno + ",'" + items.ID + "','MP'," + monthValue[j] + ")) AS source(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                    //                         "ON TBL_BP_RESOURCE_.BP_MONTH = source.BP_MONTH and TBL_BP_RESOURCE_.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE_.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE_.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE_.RESO_TYPE = source.RESO_TYPE " +
                    //                         "WHEN MATCHED THEN " +
                    //                         " UPDATE SET BP_MONTH = source.BP_MONTH,	BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO, RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_QTY = source.RESO_QTY " +
                    //                         "WHEN NOT MATCHED THEN " +
                    //                         " INSERT(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                    //                         " VALUES(source.BP_MONTH,	source.BP_YEAR, source.JOB_NO, source.RESO_ID, source.RESO_TYPE, source.RESO_QTY); ";

                    //            dbContext.Database.ExecuteSqlCommand(sql);
                    //        }

                    //    }

                    //}
                    //x.Commit();
                    transaction.Commit();
                    //}
                }
                catch (Exception ex)
                {

                    transaction.Rollback();
                    Console.WriteLine(ex.Message);
                    return RedirectToAction("Error", "Home");
                }


            }
            //var res = (from a in cn.TBL_BP_RESOURCE
            //           join b in cn.TBL_BP_RESOURCE_
            //           on new { a.BP_YEAR, a.JOB_NO, a.RESO_ID, a.RESO_TYPE } equals new { b.BP_YEAR, b.JOB_NO, b.RESO_ID, b.RESO_TYPE } into joinedData
            //           from ab in joinedData.DefaultIfEmpty()
            //           where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "MP"
            //           group new { a, ab } by new
            //           {
            //               a.RESO_ID,
            //               a.RESO_DESCRIPTION,
            //               a.RESO_UNIT,
            //               a.RESO_RATE
            //           } into grp
            //           select new IncomeVM
            //           {
            //               ID = grp.Key.RESO_ID,
            //               DESCRIPTION = grp.Key.RESO_DESCRIPTION,
            //               Unit = grp.Key.RESO_UNIT,
            //               Rate = grp.Key.RESO_RATE,
            //               JAN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 1 ? x.ab.RESO_QTY : 0),
            //               FEB = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 2 ? x.ab.RESO_QTY : 0),
            //               MAR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 3 ? x.ab.RESO_QTY : 0),
            //               APR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 4 ? x.ab.RESO_QTY : 0),
            //               MAY = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 5 ? x.ab.RESO_QTY : 0),
            //               JUN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 6 ? x.ab.RESO_QTY : 0),
            //               JUL = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 7 ? x.ab.RESO_QTY : 0),
            //               AUG = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 8 ? x.ab.RESO_QTY : 0),
            //               SEP = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 9 ? x.ab.RESO_QTY : 0),
            //               OCT = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 10 ? x.ab.RESO_QTY : 0),
            //               NOV = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 11 ? x.ab.RESO_QTY : 0),
            //               DEC = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 12 ? x.ab.RESO_QTY : 0)
            //           }).ToList();
            //List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            //MultipleVM model = new MultipleVM();
            //model.DictListVM = result;
            //model.InList = res.ToList();
            //return View(model);
            return RedirectToAction("Manpower", new { year = year, jobno = jobno });
        }

        [HttpGet]
        public ActionResult PlantGalfar(int year, string jobno)
        {

            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }

            ViewBag.Year = year;
            ViewBag.JobNo = jobno;
            List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.GlobalList = cn.PROC_BP_SHOW_MASTER("MT").ToList();
            TBL_BP_RESOURCE_ existingEntity = cn.TBL_BP_RESOURCE_.FirstOrDefault(e => e.BP_YEAR == year && e.JOB_NO == jobno && e.RESO_TYPE == "PG");
            if (existingEntity == null)
            {
                var res = (from a in cn.TBL_BP_RESOURCE
                           where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "PG"
                           select new IncomeVM
                           {
                               ID = a.RESO_ID,
                               DESCRIPTION = a.RESO_DESCRIPTION,
                               Unit = a.RESO_UNIT,
                               Rate = a.RESO_RATE,
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
                model.InList = res.ToList();
            }
            else
            {

                var res = (from a in cn.TBL_BP_RESOURCE
                           join b in cn.TBL_BP_RESOURCE_
                           on new { a.BP_YEAR, a.JOB_NO, a.RESO_ID, a.RESO_TYPE } equals new { b.BP_YEAR, b.JOB_NO, b.RESO_ID, b.RESO_TYPE } into joinedData
                           from ab in joinedData.DefaultIfEmpty()
                           where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "PG"
                           group new { a, ab } by new
                           {
                               a.RESO_ID,
                               a.RESO_DESCRIPTION,
                               a.RESO_UNIT,
                               a.RESO_RATE
                           } into grp
                           select new IncomeVM
                           {
                               ID = grp.Key.RESO_ID,
                               DESCRIPTION = grp.Key.RESO_DESCRIPTION,
                               Unit = grp.Key.RESO_UNIT,
                               Rate = grp.Key.RESO_RATE,
                               JAN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 1 ? x.ab.RESO_QTY : 0),
                               FEB = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 2 ? x.ab.RESO_QTY : 0),
                               MAR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 3 ? x.ab.RESO_QTY : 0),
                               APR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 4 ? x.ab.RESO_QTY : 0),
                               MAY = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 5 ? x.ab.RESO_QTY : 0),
                               JUN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 6 ? x.ab.RESO_QTY : 0),
                               JUL = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 7 ? x.ab.RESO_QTY : 0),
                               AUG = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 8 ? x.ab.RESO_QTY : 0),
                               SEP = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 9 ? x.ab.RESO_QTY : 0),
                               OCT = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 10 ? x.ab.RESO_QTY : 0),
                               NOV = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 11 ? x.ab.RESO_QTY : 0),
                               DEC = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 12 ? x.ab.RESO_QTY : 0)
                           }).ToList();
                model.InList = res.ToList();
            }
            model.DictListVM = result;
            GobleData.mulVm = null;

            GobleData.mulVm = new MultipleVM()
            {
                UserTbl = model.UserTbl,
                DictVM = model.DictVM,
                InList = model.InList,
                DictListVM = model.DictListVM,
                MaterialIH = model.MaterialIH,
                MaterialsIH = model.MaterialsIH,
                ResourceTbl = model.ResourceTbl,
                BPResoTbl = model.BPResoTbl,
                ResoTbl = model.ResoTbl,
                BResoTbl = model.BResoTbl,
                ClientListVM = model.ClientListVM,
                LocListVM = model.LocListVM,
                GlobalList = model.GlobalList,
                PermissionList = model.PermissionList,
                PermList = model.PermList,
                ReviTbl = model.ReviTbl
            };
            GobleData.mulVm.chkPart = new List<bool>();
            for (int i = 0; i < GobleData.mulVm.GlobalList.Count(); i++)
                GobleData.mulVm.chkPart.Add(true);
            return View(model);
        }
        [HttpPost]
        public ActionResult PlantGalfar(int year, string jobno, MultipleVM rec)
        {

            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_WRITE == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }


            int i = 1;

            using (var dbContext = new BusinessPlanEntities())
            using (var transaction = dbContext.Database.BeginTransaction())
            {

                try
                {
                    for (int a = 0; a < rec.InList.Count; a++)
                        for (int b = 0; b < GobleData.mulVm.InList.Count; b++)
                        {
                            decimal[] monthValue = new decimal[12];

                            string sql = "MERGE INTO TBL_BP_RESOURCE " +
                                         "USING(VALUES(" + year + "," + jobno + ",'" + GobleData.mulVm.InList[b].ID + "','PG','" + GobleData.mulVm.InList[b].DESCRIPTION + "','" + GobleData.mulVm.InList[b].Unit + "'," + GobleData.mulVm.InList[b].Rate + "," + i + ")) AS source(BP_YEAR, JOB_NO,   RESO_ID, RESO_TYPE, RESO_DESCRIPTION,	RESO_UNIT,	RESO_RATE,	ROW_ID) " +
                                         "ON TBL_BP_RESOURCE.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE.RESO_TYPE = source.RESO_TYPE " +
                                         "WHEN MATCHED THEN " +
                                         " UPDATE SET BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO,  RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_DESCRIPTION = source.RESO_DESCRIPTION, RESO_UNIT = source.RESO_UNIT, RESO_RATE = source.RESO_RATE, ROW_ID = source.ROW_ID " +
                                         "WHEN NOT MATCHED THEN " +
                                         " INSERT(BP_YEAR, JOB_NO,  RESO_ID, RESO_TYPE, RESO_DESCRIPTION, RESO_UNIT, RESO_RATE, ROW_ID) " +
                                         " VALUES(source.BP_YEAR, source.JOB_NO, source.RESO_ID, source.RESO_TYPE, source.RESO_DESCRIPTION, source.RESO_UNIT, source.RESO_RATE,source.ROW_ID); ";
                            dbContext.Database.ExecuteSqlCommand(sql);
                            monthValue[0] = rec.InList[a].JAN.Value;
                            monthValue[1] = rec.InList[a].FEB.Value;
                            monthValue[2] = rec.InList[a].MAR.Value;
                            monthValue[3] = rec.InList[a].APR.Value;
                            monthValue[4] = rec.InList[a].MAY.Value;
                            monthValue[5] = rec.InList[a].JUN.Value;
                            monthValue[6] = rec.InList[a].JUL.Value;
                            monthValue[7] = rec.InList[a].AUG.Value;
                            monthValue[8] = rec.InList[a].SEP.Value;
                            monthValue[9] = rec.InList[a].OCT.Value;
                            monthValue[10] = rec.InList[a].NOV.Value;
                            monthValue[11] = rec.InList[a].DEC.Value;
                            for (int j = 0; j < 12; j++)
                            {
                                int k = j + 1;
                                sql = "MERGE INTO TBL_BP_RESOURCE_ " +
                                             "USING(VALUES(" + k + "," + year + "," + jobno + ",'" + GobleData.mulVm.InList[a].ID + "','PG'," + monthValue[j] + ")) AS source(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                             "ON TBL_BP_RESOURCE_.BP_MONTH = source.BP_MONTH and TBL_BP_RESOURCE_.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE_.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE_.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE_.RESO_TYPE = source.RESO_TYPE " +
                                             "WHEN MATCHED THEN " +
                                             " UPDATE SET BP_MONTH = source.BP_MONTH,	BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO, RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_QTY = source.RESO_QTY " +
                                             "WHEN NOT MATCHED THEN " +
                                             " INSERT(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                             " VALUES(source.BP_MONTH,	source.BP_YEAR, source.JOB_NO, source.RESO_ID, source.RESO_TYPE, source.RESO_QTY); ";

                                dbContext.Database.ExecuteSqlCommand(sql);
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

            //List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            //MultipleVM model = new MultipleVM();
            //var record = (from a in cn.TBL_BP_RESOURCE
            //              join b in cn.TBL_BP_RESOURCE_
            //              on new { a.BP_YEAR, a.JOB_NO, a.RESO_ID, a.RESO_TYPE } equals new { b.BP_YEAR, b.JOB_NO, b.RESO_ID, b.RESO_TYPE }
            //              where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "PG"
            //              group new { a, b } by new
            //              {
            //                  a.RESO_ID,
            //                  a.RESO_DESCRIPTION,
            //                  a.RESO_UNIT,
            //                  a.RESO_RATE
            //              } into grp
            //              select new IncomeVM
            //              {
            //                  ID = grp.Key.RESO_ID,
            //                  DESCRIPTION = grp.Key.RESO_DESCRIPTION,
            //                  Unit = grp.Key.RESO_UNIT,
            //                  Rate = grp.Key.RESO_RATE,
            //                  JAN = grp.Sum(x => x.b.BP_MONTH == 1 ? x.b.RESO_QTY : 0),
            //                  FEB = grp.Sum(x => x.b.BP_MONTH == 2 ? x.b.RESO_QTY : 0),
            //                  MAR = grp.Sum(x => x.b.BP_MONTH == 3 ? x.b.RESO_QTY : 0),
            //                  APR = grp.Sum(x => x.b.BP_MONTH == 4 ? x.b.RESO_QTY : 0),
            //                  MAY = grp.Sum(x => x.b.BP_MONTH == 5 ? x.b.RESO_QTY : 0),
            //                  JUN = grp.Sum(x => x.b.BP_MONTH == 6 ? x.b.RESO_QTY : 0),
            //                  JUL = grp.Sum(x => x.b.BP_MONTH == 7 ? x.b.RESO_QTY : 0),
            //                  AUG = grp.Sum(x => x.b.BP_MONTH == 8 ? x.b.RESO_QTY : 0),
            //                  SEP = grp.Sum(x => x.b.BP_MONTH == 9 ? x.b.RESO_QTY : 0),
            //                  OCT = grp.Sum(x => x.b.BP_MONTH == 10 ? x.b.RESO_QTY : 0),
            //                  NOV = grp.Sum(x => x.b.BP_MONTH == 11 ? x.b.RESO_QTY : 0),
            //                  DEC = grp.Sum(x => x.b.BP_MONTH == 12 ? x.b.RESO_QTY : 0)
            //              }).ToList();

            //model.InList = record.ToList();
            //model.DictListVM = result;
            //return View(model);
            return RedirectToAction("PlantGalfar", new { year = year, jobno = jobno });
        }

        [HttpGet]
        public ActionResult PlantHire(int year, string jobno)
        {

            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }

            ViewBag.Year = year;
            ViewBag.JobNo = jobno;
            List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.GlobalList = cn.PROC_BP_SHOW_MASTER("MT").ToList();
            TBL_BP_RESOURCE_ existingEntity = cn.TBL_BP_RESOURCE_.FirstOrDefault(e => e.BP_YEAR == year && e.JOB_NO == jobno && e.RESO_TYPE == "PH");
            if (existingEntity == null)
            {
                var res = (from a in cn.TBL_BP_RESOURCE
                           where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "PH"
                           select new IncomeVM
                           {
                               ID = a.RESO_ID,
                               DESCRIPTION = a.RESO_DESCRIPTION,
                               Unit = a.RESO_UNIT,
                               Rate = a.RESO_RATE,
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
                model.InList = res.ToList();
            }
            else
            {
                var res = (from a in cn.TBL_BP_RESOURCE
                           join b in cn.TBL_BP_RESOURCE_
                           on new { a.BP_YEAR, a.JOB_NO, a.RESO_ID, a.RESO_TYPE } equals new { b.BP_YEAR, b.JOB_NO, b.RESO_ID, b.RESO_TYPE } into joinedData
                           from ab in joinedData.DefaultIfEmpty()
                           where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "PH"
                           group new { a, ab } by new
                           {
                               a.RESO_ID,
                               a.RESO_DESCRIPTION,
                               a.RESO_UNIT,
                               a.RESO_RATE
                           } into grp
                           select new IncomeVM
                           {
                               ID = grp.Key.RESO_ID,
                               DESCRIPTION = grp.Key.RESO_DESCRIPTION,
                               Unit = grp.Key.RESO_UNIT,
                               Rate = grp.Key.RESO_RATE,
                               JAN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 1 ? x.ab.RESO_QTY : 0),
                               FEB = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 2 ? x.ab.RESO_QTY : 0),
                               MAR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 3 ? x.ab.RESO_QTY : 0),
                               APR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 4 ? x.ab.RESO_QTY : 0),
                               MAY = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 5 ? x.ab.RESO_QTY : 0),
                               JUN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 6 ? x.ab.RESO_QTY : 0),
                               JUL = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 7 ? x.ab.RESO_QTY : 0),
                               AUG = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 8 ? x.ab.RESO_QTY : 0),
                               SEP = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 9 ? x.ab.RESO_QTY : 0),
                               OCT = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 10 ? x.ab.RESO_QTY : 0),
                               NOV = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 11 ? x.ab.RESO_QTY : 0),
                               DEC = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 12 ? x.ab.RESO_QTY : 0)
                           }).ToList();
                model.InList = res.ToList();
            }
            model.DictListVM = result;
            GobleData.mulVm = null;

            GobleData.mulVm = new MultipleVM()
            {
                UserTbl = model.UserTbl,
                DictVM = model.DictVM,
                InList = model.InList,
                DictListVM = model.DictListVM,
                MaterialIH = model.MaterialIH,
                MaterialsIH = model.MaterialsIH,
                ResourceTbl = model.ResourceTbl,
                BPResoTbl = model.BPResoTbl,
                ResoTbl = model.ResoTbl,
                BResoTbl = model.BResoTbl,
                ClientListVM = model.ClientListVM,
                LocListVM = model.LocListVM,
                GlobalList = model.GlobalList,
                PermissionList = model.PermissionList,
                PermList = model.PermList,
                ReviTbl = model.ReviTbl
            };
            GobleData.mulVm.chkPart = new List<bool>();
            for (int i = 0; i < GobleData.mulVm.GlobalList.Count(); i++)
                GobleData.mulVm.chkPart.Add(true);
            return View(model);
        }
        [HttpPost]
        public ActionResult PlantHire(int year, string jobno, MultipleVM rec)
        {

            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_WRITE == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }

            int i = 1;

            using (var dbContext = new BusinessPlanEntities())
            using (var transaction = dbContext.Database.BeginTransaction())
            {

                try
                {
                    for (int a = 0; a < rec.InList.Count; a++)
                        for (int b = 0; b < GobleData.mulVm.InList.Count; b++)
                        {
                            decimal[] monthValue = new decimal[12];

                            string sql = "MERGE INTO TBL_BP_RESOURCE " +
                                         "USING(VALUES(" + year + "," + jobno + ",'" + GobleData.mulVm.InList[b].ID + "','PH','" + GobleData.mulVm.InList[b].DESCRIPTION + "','" + GobleData.mulVm.InList[b].Unit + "'," + GobleData.mulVm.InList[b].Rate + "," + i + ")) AS source(BP_YEAR, JOB_NO,   RESO_ID, RESO_TYPE, RESO_DESCRIPTION,	RESO_UNIT,	RESO_RATE,	ROW_ID) " +
                                         "ON TBL_BP_RESOURCE.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE.RESO_TYPE = source.RESO_TYPE " +
                                         "WHEN MATCHED THEN " +
                                         " UPDATE SET BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO,  RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_DESCRIPTION = source.RESO_DESCRIPTION, RESO_UNIT = source.RESO_UNIT, RESO_RATE = source.RESO_RATE, ROW_ID = source.ROW_ID " +
                                         "WHEN NOT MATCHED THEN " +
                                         " INSERT(BP_YEAR, JOB_NO,  RESO_ID, RESO_TYPE, RESO_DESCRIPTION, RESO_UNIT, RESO_RATE, ROW_ID) " +
                                         " VALUES(source.BP_YEAR, source.JOB_NO, source.RESO_ID, source.RESO_TYPE, source.RESO_DESCRIPTION, source.RESO_UNIT, source.RESO_RATE,source.ROW_ID); ";
                            dbContext.Database.ExecuteSqlCommand(sql);
                            monthValue[0] = rec.InList[a].JAN.Value;
                            monthValue[1] = rec.InList[a].FEB.Value;
                            monthValue[2] = rec.InList[a].MAR.Value;
                            monthValue[3] = rec.InList[a].APR.Value;
                            monthValue[4] = rec.InList[a].MAY.Value;
                            monthValue[5] = rec.InList[a].JUN.Value;
                            monthValue[6] = rec.InList[a].JUL.Value;
                            monthValue[7] = rec.InList[a].AUG.Value;
                            monthValue[8] = rec.InList[a].SEP.Value;
                            monthValue[9] = rec.InList[a].OCT.Value;
                            monthValue[10] = rec.InList[a].NOV.Value;
                            monthValue[11] = rec.InList[a].DEC.Value;
                            for (int j = 0; j < 12; j++)
                            {
                                int k = j + 1;
                                sql = "MERGE INTO TBL_BP_RESOURCE_ " +
                                             "USING(VALUES(" + k + "," + year + "," + jobno + ",'" + GobleData.mulVm.InList[a].ID + "','PH'," + monthValue[j] + ")) AS source(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                             "ON TBL_BP_RESOURCE_.BP_MONTH = source.BP_MONTH and TBL_BP_RESOURCE_.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE_.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE_.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE_.RESO_TYPE = source.RESO_TYPE " +
                                             "WHEN MATCHED THEN " +
                                             " UPDATE SET BP_MONTH = source.BP_MONTH,	BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO, RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_QTY = source.RESO_QTY " +
                                             "WHEN NOT MATCHED THEN " +
                                             " INSERT(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                             " VALUES(source.BP_MONTH,	source.BP_YEAR, source.JOB_NO, source.RESO_ID, source.RESO_TYPE, source.RESO_QTY); ";

                                dbContext.Database.ExecuteSqlCommand(sql);
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

            //List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            //MultipleVM model = new MultipleVM();
            //var record = (from a in cn.TBL_BP_RESOURCE
            //              join b in cn.TBL_BP_RESOURCE_
            //              on new { a.BP_YEAR, a.JOB_NO, a.RESO_ID, a.RESO_TYPE } equals new { b.BP_YEAR, b.JOB_NO, b.RESO_ID, b.RESO_TYPE }
            //              where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "PH"
            //              group new { a, b } by new
            //              {
            //                  a.RESO_ID,
            //                  a.RESO_DESCRIPTION,
            //                  a.RESO_UNIT,
            //                  a.RESO_RATE
            //              } into grp
            //              select new IncomeVM
            //              {
            //                  ID = grp.Key.RESO_ID,
            //                  DESCRIPTION = grp.Key.RESO_DESCRIPTION,
            //                  Unit = grp.Key.RESO_UNIT,
            //                  Rate = grp.Key.RESO_RATE,
            //                  JAN = grp.Sum(x => x.b.BP_MONTH == 1 ? x.b.RESO_QTY : 0),
            //                  FEB = grp.Sum(x => x.b.BP_MONTH == 2 ? x.b.RESO_QTY : 0),
            //                  MAR = grp.Sum(x => x.b.BP_MONTH == 3 ? x.b.RESO_QTY : 0),
            //                  APR = grp.Sum(x => x.b.BP_MONTH == 4 ? x.b.RESO_QTY : 0),
            //                  MAY = grp.Sum(x => x.b.BP_MONTH == 5 ? x.b.RESO_QTY : 0),
            //                  JUN = grp.Sum(x => x.b.BP_MONTH == 6 ? x.b.RESO_QTY : 0),
            //                  JUL = grp.Sum(x => x.b.BP_MONTH == 7 ? x.b.RESO_QTY : 0),
            //                  AUG = grp.Sum(x => x.b.BP_MONTH == 8 ? x.b.RESO_QTY : 0),
            //                  SEP = grp.Sum(x => x.b.BP_MONTH == 9 ? x.b.RESO_QTY : 0),
            //                  OCT = grp.Sum(x => x.b.BP_MONTH == 10 ? x.b.RESO_QTY : 0),
            //                  NOV = grp.Sum(x => x.b.BP_MONTH == 11 ? x.b.RESO_QTY : 0),
            //                  DEC = grp.Sum(x => x.b.BP_MONTH == 12 ? x.b.RESO_QTY : 0)
            //              }).ToList();

            //model.InList = record.ToList();
            //model.DictListVM = result;
            //return View(model);
            return RedirectToAction("PlantHire",new { year=year,jobno=jobno});
        }

        [HttpGet]
        public ActionResult Subcontract(int year, string jobno)
        {
            ViewBag.Year = year;
            ViewBag.JobNo = jobno;
            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }

            List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            var record = (from a in cn.TBL_BP_RESOURCE
                          join b in cn.TBL_BP_RESOURCE_
                          on new { a.BP_YEAR, a.JOB_NO, a.RESO_ID, a.RESO_TYPE } equals new { b.BP_YEAR, b.JOB_NO, b.RESO_ID, b.RESO_TYPE } into joinedData
                          from ab in joinedData.DefaultIfEmpty()
                          where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "SB"
                          group new { a, ab } by new
                          {
                              a.RESO_ID,
                              a.RESO_DESCRIPTION,
                          } into grp
                          select new IncomeVM
                          {
                              ID = grp.Key.RESO_ID,
                              DESCRIPTION = grp.Key.RESO_DESCRIPTION,
                              JAN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 1 ? x.ab.RESO_QTY : 0),
                              FEB = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 2 ? x.ab.RESO_QTY : 0),
                              MAR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 3 ? x.ab.RESO_QTY : 0),
                              APR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 4 ? x.ab.RESO_QTY : 0),
                              MAY = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 5 ? x.ab.RESO_QTY : 0),
                              JUN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 6 ? x.ab.RESO_QTY : 0),
                              JUL = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 7 ? x.ab.RESO_QTY : 0),
                              AUG = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 8 ? x.ab.RESO_QTY : 0),
                              SEP = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 9 ? x.ab.RESO_QTY : 0),
                              OCT = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 10 ? x.ab.RESO_QTY : 0),
                              NOV = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 11 ? x.ab.RESO_QTY : 0),
                              DEC = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 12 ? x.ab.RESO_QTY : 0)
                          }).ToList();

            model.InList = record.ToList();
            model.DictListVM = result;
            return View(model);
        }

        [HttpPost]
        public ActionResult Subcontract(int year, string jobno, MultipleVM rec, List<IncomeVM> InList)
        {
            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_WRITE == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }
            int i = 1;
            using (var dbContext = new BusinessPlanEntities())
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    for (int index = 0; index < InList.Count; index++)
                    {
                        var items = InList[index];
                        decimal[] monthValue = new decimal[12];
                      string sql = "MERGE INTO TBL_BP_RESOURCE " +
                                     "USING(VALUES(" + year + "," + jobno +  ",'" + items.ID + "','SB','" + items.DESCRIPTION + "','" + 1 + "'," + 0 + "," + i + ")) AS source(BP_YEAR, JOB_NO,  RESO_ID, RESO_TYPE, RESO_DESCRIPTION,		RESO_RATE,	RESO_EQTY,	ROW_ID) " +
                                     "ON TBL_BP_RESOURCE.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE.RESO_TYPE = source.RESO_TYPE " +
                                     "WHEN MATCHED THEN " +
                                     " UPDATE SET BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO,  RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_DESCRIPTION = source.RESO_DESCRIPTION,  RESO_RATE = source.RESO_RATE, RESO_EQTY = source.RESO_EQTY, ROW_ID = source.ROW_ID " +
                                     "WHEN NOT MATCHED THEN " +
                                     " INSERT(BP_YEAR, JOB_NO,  RESO_ID, RESO_TYPE, RESO_DESCRIPTION,  RESO_RATE, RESO_EQTY, ROW_ID) " +
                                     " VALUES(source.BP_YEAR, source.JOB_NO,  source.RESO_ID, source.RESO_TYPE, source.RESO_DESCRIPTION,  source.RESO_RATE, source.RESO_EQTY,source.ROW_ID); ";
                        Console.WriteLine("Executing SQL query: " + sql);
                        dbContext.Database.ExecuteSqlCommand(sql);

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
                            sql = "MERGE INTO TBL_BP_RESOURCE_ " +
                                     "USING(VALUES(" + k + "," + year + "," + jobno + ",'" + items.ID + "','SB'," + monthValue[j] + ")) AS source(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                     "ON TBL_BP_RESOURCE_.BP_MONTH = source.BP_MONTH and TBL_BP_RESOURCE_.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE_.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE_.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE_.RESO_TYPE = source.RESO_TYPE " +
                                     "WHEN MATCHED THEN " +
                                     " UPDATE SET BP_MONTH = source.BP_MONTH,	BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO, RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_QTY = source.RESO_QTY " +
                                     "WHEN NOT MATCHED THEN " +
                                     " INSERT(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                     " VALUES(source.BP_MONTH,	source.BP_YEAR, source.JOB_NO, source.RESO_ID, source.RESO_TYPE, source.RESO_QTY); ";

                            dbContext.Database.ExecuteSqlCommand(sql);
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
            return RedirectToAction("Subcontract", new { year = year, jobno = jobno });
        }
        [HttpGet]
        public ActionResult GeneralExpenditure(int year, string jobno)
        {
            ViewBag.Year = year;
            ViewBag.JobNo = jobno;
            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }

            List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            var record = (from a in cn.TBL_BP_RESOURCE
                          join b in cn.TBL_BP_RESOURCE_
                          on new { a.BP_YEAR, a.JOB_NO, a.RESO_ID, a.RESO_TYPE } equals new { b.BP_YEAR, b.JOB_NO, b.RESO_ID, b.RESO_TYPE } into joinedData
                          from ab in joinedData.DefaultIfEmpty()
                          where a.BP_YEAR == year && a.JOB_NO == jobno && a.RESO_TYPE == "GE"
                          group new { a, ab } by new
                          {
                              a.RESO_ID,
                              a.RESO_DESCRIPTION,
                          } into grp
                          select new IncomeVM
                          {
                              ID = grp.Key.RESO_ID,
                              DESCRIPTION = grp.Key.RESO_DESCRIPTION,
                              JAN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 1 ? x.ab.RESO_QTY : 0),
                              FEB = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 2 ? x.ab.RESO_QTY : 0),
                              MAR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 3 ? x.ab.RESO_QTY : 0),
                              APR = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 4 ? x.ab.RESO_QTY : 0),
                              MAY = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 5 ? x.ab.RESO_QTY : 0),
                              JUN = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 6 ? x.ab.RESO_QTY : 0),
                              JUL = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 7 ? x.ab.RESO_QTY : 0),
                              AUG = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 8 ? x.ab.RESO_QTY : 0),
                              SEP = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 9 ? x.ab.RESO_QTY : 0),
                              OCT = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 10 ? x.ab.RESO_QTY : 0),
                              NOV = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 11 ? x.ab.RESO_QTY : 0),
                              DEC = grp.Sum(x => x.ab != null && x.ab.BP_MONTH == 12 ? x.ab.RESO_QTY : 0)
                          }).ToList();

            model.InList = record.ToList();
            model.DictListVM = result;
            return View(model);
        }
        [HttpPost]
        public ActionResult GeneralExpenditure(int year, string jobno, MultipleVM rec,List<IncomeVM> InList)
        {
            string User = Session["UserId"].ToString();

            var check = cn.TBL_BP_PERMISSION.Where(x => x.JOB_NO == jobno && x.BP_READ == 1 && x.BP_WRITE == 1 && x.BP_USER == User).ToList();

            if (check.Count() == 0)
            {
                return RedirectToAction("AccessDenied", "UserAuthorization");
            }
            int i = 1;
            using (var dbContext = new BusinessPlanEntities())
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    for (int index = 0; index < InList.Count; index++)
                    {
                        var items = InList[index];
                        decimal[] monthValue = new decimal[12];

                        //decimal? qty = items.EST__QTY ?? 0;
                        // string grp = items.GRP ?? "NULL"; // Replace '0' with the default value if GRP is null

                        string sql = "MERGE INTO TBL_BP_RESOURCE " +
                                     "USING(VALUES(" + year + "," + jobno + ",'" + items.ID + "','GE','" + items.DESCRIPTION + "','" + 1 + "'," + 0 + "," + i + ")) AS source(BP_YEAR, JOB_NO,  RESO_ID, RESO_TYPE, RESO_DESCRIPTION,		RESO_RATE,	RESO_EQTY,	ROW_ID) " +
                                     "ON TBL_BP_RESOURCE.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE.RESO_TYPE = source.RESO_TYPE " +
                                     "WHEN MATCHED THEN " +
                                     " UPDATE SET BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO,  RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_DESCRIPTION = source.RESO_DESCRIPTION,  RESO_RATE = source.RESO_RATE, RESO_EQTY = source.RESO_EQTY, ROW_ID = source.ROW_ID " +
                                     "WHEN NOT MATCHED THEN " +
                                     " INSERT(BP_YEAR, JOB_NO,  RESO_ID, RESO_TYPE, RESO_DESCRIPTION,  RESO_RATE, RESO_EQTY, ROW_ID) " +
                                     " VALUES(source.BP_YEAR, source.JOB_NO,  source.RESO_ID, source.RESO_TYPE, source.RESO_DESCRIPTION,  source.RESO_RATE, source.RESO_EQTY,source.ROW_ID); ";
                        Console.WriteLine("Executing SQL query: " + sql);
                        dbContext.Database.ExecuteSqlCommand(sql);

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
                            sql = "MERGE INTO TBL_BP_RESOURCE_ " +
                                     "USING(VALUES(" + k + "," + year + "," + jobno + ",'" + items.ID + "','GE'," + monthValue[j] + ")) AS source(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                     "ON TBL_BP_RESOURCE_.BP_MONTH = source.BP_MONTH and TBL_BP_RESOURCE_.BP_YEAR = source.BP_YEAR and TBL_BP_RESOURCE_.JOB_NO = source.JOB_NO and TBL_BP_RESOURCE_.RESO_ID = source.RESO_ID and TBL_BP_RESOURCE_.RESO_TYPE = source.RESO_TYPE " +
                                     "WHEN MATCHED THEN " +
                                     " UPDATE SET BP_MONTH = source.BP_MONTH,	BP_YEAR = source.BP_YEAR, JOB_NO = source.JOB_NO, RESO_ID = source.RESO_ID, RESO_TYPE = source.RESO_TYPE, RESO_QTY = source.RESO_QTY " +
                                     "WHEN NOT MATCHED THEN " +
                                     " INSERT(BP_MONTH,	BP_YEAR, JOB_NO, RESO_ID, RESO_TYPE, RESO_QTY) " +
                                     " VALUES(source.BP_MONTH,	source.BP_YEAR, source.JOB_NO, source.RESO_ID, source.RESO_TYPE, source.RESO_QTY); ";

                            dbContext.Database.ExecuteSqlCommand(sql);
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
            return RedirectToAction("GeneralExpenditure", new { year = year, jobno = jobno });
        }

    }
}