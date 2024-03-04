using BusinessApplication.Models;
using BusinessApplication.ViewModels;
using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Data;
using System.Reflection;
using System.Data.SqlClient;


namespace BusinessApplication.Controllers
{
    public class HomeController : Controller
    {
        BusinessPlanEntities cn = new BusinessPlanEntities();
        public ActionResult Index()
        {
            
            if (Session["UserId"] != null)
            {
                List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
                result = this.cn.TBL_BP_DICTIONARY.ToList();
                MultipleVM model = new MultipleVM();
                model.DictListVM = result;
                return View(model);              
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpGet]
        public ActionResult Report(string reportType)
        {
            ViewBag.ReportType = reportType;
            var bpYears = this.cn.TBL_BP_DICTIONARY.Select(x => x.BP_YEAR).Distinct().ToList();
            ViewBag.BPYEAR = new SelectList(bpYears.Select(x => new SelectListItem
            {
                Text = x.ToString(),
                Value = x.ToString()
            }).ToList(), "Value", "Text");
            ViewBag.JOBNO = new SelectList(this.cn.TBL_BP_DICTIONARY.Select(x => new SelectListItem
            {
                Text = x.JOB_NO,
                Value = x.JOB_NO
            }).ToList(), "Value", "Text");
            ViewBag.User = new SelectList(this.cn.TBL_BP_USERMASTER.Select(x => new SelectListItem
            {
                Text = x.BP_USER,
                Value = x.BP_USER
            }).ToList(), "Value", "Text");
            ViewBag.Message = "Your Report page.";
            ViewBag.Message = "Your Report page.";

            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.DictListVM = result;

            if (reportType == "J2D" || reportType == "JSY")
            {
                return View("JobwiseReport", model);
            }
            if (reportType == "BPA")
            {
                return View("Divisionwisesummary", model);
            }
            if (reportType == "JSC")
            {
                return View("JobwiseReport", model);
            }
            if (reportType == "VOWD")
            {
                return View("VOWD", model);
            }


            return View(model);
        }
        [HttpPost,ActionName("Report")]
        public ActionResult Reports()
        {
           
            return RedirectToAction("ReportData");
        }
        [HttpGet]
        public ActionResult ReportA()
        {
            ViewBag.Message = "Your Report page.";

            return View(this.cn.TBL_BP_DICTIONARY.ToList());
        }
        [HttpPost, ActionName("ReportA")]
        public ActionResult ReportZ()
        {
            ViewBag.Message = "Your Report page.";

            return RedirectToAction("ReportData");
        }
        //public ActionResult ReportData()
        //{
        //    ViewBag.Message = "Your Report page.";

        //    return View();
        //}
        public ActionResult ReportData(string year, string month, string jobno)
        {
            ViewBag.Message = "Your Report page.";
            //var a = cn.PROC_BP_REPT_MONTHWISE(DepartmentCode1, JobNos);
            //var b = cn.PROC_MN_REPT_GE4MONTH(year, Months, JobNos);
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.DictListVM = result;
            model.MonthRptList = cn.PROC_MN_REPT_GE4MONTH("2007","12", "16061700").ToList();
            return View(model);
        }
        public ActionResult Monthwise()
        {
            ViewBag.Message = "Your Report page.";

            return View();
        }
        public ActionResult Quaterwise()
        {
            ViewBag.Message = "Your Report page.";

            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.Where(x => x.BP_YEAR > 2021).ToList();
            MultipleVM model = new MultipleVM();
            model.DictListVM = result;
            return View(model);
        }
        public ActionResult Divisionwise()
        {
            ViewBag.Message = "Your Report page.";

            return View();
        }

        public ActionResult PlantvsActual()
        {
            ViewBag.Message = "Your Report page.";

            return View();
        }

        public ActionResult M_Index()
        {
            ViewBag.Message = "Your Report page.";

            return View();
        }

        public ActionResult Demoreport()
        {
            //List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            //result = this.cn.TBL_BP_DICTIONARY.Where(x => x.BP_YEAR > 2021).ToList();
            //MultipleVM model = new MultipleVM();
            //model.DictListVM = result;
            //return View(model);

            List<TBL_BP_DICTIONARY> result = this.cn.TBL_BP_DICTIONARY.Where(x => x.BP_YEAR > 2021).ToList();
            MultipleVM model = new MultipleVM();
            model.DictListVM = result;
            return View(model);

        }

        public ActionResult Exportrpt()
        {
            MultipleVM model = new MultipleVM();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BP_MONTHWISE.rpt"));
            rd.SetDataSource(ListToDataTable(model.DictListVM.ToList()));
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                return File(stream, "application/pdf", "Demopdf.pdf");
            }
            catch
            {
                throw;
            }
        }
        private DataTable ListToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach(PropertyInfo prop in Props)
            {
                dataTable.Columns.Add(prop.Name);

            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for(int i=0; i<Props.Length; i++)
                {
                    values[i] = Props[i].GetValue(item, null);

                }
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        //Master Data Client CRUD Operations
        public ActionResult GetAllClient( )
        {
            MultipleVM model = new MultipleVM();
            model.DictListVM = this.cn.TBL_BP_DICTIONARY.ToList();
            ClientVM lst = new ClientVM();
            var res = (from t in cn.TBL_BP_CLIENT
                       select new ClientVM
                       {
                           ID = t.CLIENT,
                           ClientName = t.ClientName
                       }).ToList();
          
            model.CVM = res.ToList();
            return View(model);
        }
       
        [HttpPost]
        public ActionResult AddClient(List<ClientVM> CVM)
        {
            string User = Session["UserId"].ToString();          
                    foreach (var data in CVM)
                    {
                            TBL_BP_CLIENT existingEntity = cn.TBL_BP_CLIENT
                                .SingleOrDefault(e => e.CLIENT == data.ID);
             
                           if (existingEntity == null )
                            {                           
                                TBL_BP_CLIENT newEntity = new TBL_BP_CLIENT
                                {
                                    CLIENT = data.ID,
                                    ClientName=data.ClientName,
                                    CreatedBy=User,
                                    CreatedOn= DateTime.Now
                                };

                                this.cn.TBL_BP_CLIENT.Add(newEntity);
                            }                
                    }
                    this.cn.SaveChanges();
              return View();
        }
      
        [HttpPost]
        public ActionResult EditClient( int Id, string Name)
        {
            string User = Session["UserId"].ToString();
            TBL_BP_CLIENT existingEntity = cn.TBL_BP_CLIENT
             .SingleOrDefault(e => e.CLIENT==Id);
            existingEntity.ClientName = Name;
            existingEntity.ModifiedBy = User;
            existingEntity.ModifiedOn = DateTime.Now;
             this.cn.SaveChanges();
            return View();
        }
      
        [HttpPost]
        public ActionResult DeleteClient(int Id)
        {
              TBL_BP_CLIENT existingEntity = cn.TBL_BP_CLIENT
                    .SingleOrDefault(e => e.CLIENT == Id);              
              this.cn.TBL_BP_CLIENT.Remove(existingEntity);                               
            
            this.cn.SaveChanges();
            return View();
        }

        //Master Data Location CRUD Operations
        public ActionResult GetAllLocation()
        {
            MultipleVM model = new MultipleVM();
            model.DictListVM = this.cn.TBL_BP_DICTIONARY.ToList();
            var res = (from t in cn.TBL_BP_LOCATION
                       select new LocationVM
                       {
                           ID = t.ID,
                           Location = t.Location
                       }).ToList();
            model.LVM = res.ToList();
            return View(model);
        }
     
        [HttpPost]
        public ActionResult AddLocation(List<LocationVM> LVM)
        {
            string User = Session["UserId"].ToString();
            foreach (var data in LVM)
            {
                TBL_BP_LOCATION existingEntity = cn.TBL_BP_LOCATION
                    .SingleOrDefault(e => e.ID == data.ID);

                if (existingEntity == null)
                {
                    TBL_BP_LOCATION newEntity = new TBL_BP_LOCATION
                    {
                        ID = data.ID,
                        Location = data.Location,
                        CreatedBy = User,
                        CreatedOn = DateTime.Now
                    };

                    this.cn.TBL_BP_LOCATION.Add(newEntity);
                }
            }
            this.cn.SaveChanges();
            return View();
        }

        [HttpPost]
        public ActionResult EditLocation(int Id, string Name)
        {
            string User = Session["UserId"].ToString();
            TBL_BP_LOCATION existingEntity = cn.TBL_BP_LOCATION
                    .SingleOrDefault(e => e.ID == Id);
            existingEntity.Location = Name;
            existingEntity.ModifiedBy = User;
            existingEntity.ModifiedOn = DateTime.Now;
            this.cn.SaveChanges();
            return View();
        }

        [HttpPost]
        public ActionResult DeleteLocation(int Id)
        {
            TBL_BP_LOCATION existingEntity = cn.TBL_BP_LOCATION
                    .SingleOrDefault(e => e.ID == Id);
            this.cn.TBL_BP_LOCATION.Remove(existingEntity);

            this.cn.SaveChanges();
            return View();
        }
        public ActionResult Error()
        {
            return View();
        }
        public ActionResult Success()
        {
            return View();
        }
    }
}