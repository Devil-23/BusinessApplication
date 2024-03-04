using BusinessApplication.Models;
using BusinessApplication.ViewModels;
using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BusinessApplication.Controllers
{
    public class TestReportController : Controller
    {
        private BusinessPlanEntities cn = new BusinessPlanEntities();
        ReportDocument rd = new ReportDocument();
        public ActionResult Index()
        {
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);

            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            ViewBag.FilePDF = Convert.ToBase64String(ms.ToArray());
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.DictListVM = result;
            return View(model);
        }


        public ActionResult ExportReport()
        {

            ////string Constring1 = "Password=Galfar@12345;Persist Security Info=True;User ID=sa;Initial Catalog=BusinessPlan;Data Source=GALFAR-WEU-DEV-\\SQLEXPRESS";
            ////SqlConnection conn = new SqlConnection(Constring1);
            ////conn.Open();

            ReportDocument rd = new ReportDocument();
            //rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BP_MONTHWISE_Test.rpt"));
           
            //rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BP_QUARTERWISE_Test.rpt"));

            //rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BP_QUARTERWISE_Test.rpt"));

            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BP_JOBWISEPLAN_Test.rpt"));

            //rd.Load(Path.Combine(Server.MapPath("~/Reports"), "QuarterWise_Test.rpt"));

            rd.SetDatabaseLogon("sa", "Galfar@12345");
            rd.VerifyDatabase();
            rd.Refresh();
            //rd.SetParameterValue(0, "2008");
            //rd.SetParameterValue(1, "17072500");
            //rd.SetParameterValue(2, "%");

            ////For QUARTERWISE testing use below parms
            rd.SetParameterValue(0, "2017");
            rd.SetParameterValue(1, "12222200");
            rd.SetParameterValue(2, "%");

            ////For Jobwise testing use below parms
            rd.SetParameterValue(0, "2017");
            rd.SetParameterValue(1, "14041000");
            rd.SetParameterValue(2, "%");


            //rd.SetDataSource(allCustomer);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            //string fname =  "Yearly report " + 
            return File(stream, "application/pdf", "CustomerList.pdf");
        }
        [HttpPost]
        [ActionName("ExportReport")]
        public ActionResult ExportCust(int BPYEAR, string JOBNO, string reportType, string[] selectedJobNumbers,int? month,string USER)
        {

            ////string Constring1 = "Password=Galfar@12345;Persist Security Info=True;User ID=sa;Initial Catalog=BusinessPlan;Data Source=GALFAR-WEU-DEV-\\SQLEXPRESS";
            ////SqlConnection conn = new SqlConnection(Constring1);
            ////conn.Open();
            string jobno = JOBNO.Substring(0,2);
            ReportDocument rd = new ReportDocument();
            if (reportType == "Monthwise")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BP_MONTHWISE_Test.rpt"));
            }
            else if (reportType == "Quarterwise")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BP_QUARTERWISE_Test.rpt"));
            }
            else if (reportType == "Divisionwise")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BP_DIVISIONWISE.rpt"));
            }
            else if (reportType == "Jobwise")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BP_JOBWISEPLAN_Test.rpt"));
            }
            else if (reportType == "MP")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MpghSummary.rpt"));
            }
            else if (reportType == "IH" || reportType == "BO" || reportType == "PG" || reportType == "PH" || reportType == "SB" || reportType == "GE")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MpghSummary.rpt"));
            }


            else if (reportType == "WK")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "WLOAD.rpt"));
            }
            else if (reportType == "VOWD")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "VOWD.rpt"));
            }
            else if (reportType == "EG")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "EXGLANCE.rpt"));
            }
            else if (reportType == "J2D")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MN_JOB2DATE.rpt"));
            } 
            else if (reportType == "DS")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MN_DIVISION.rpt"));
            }
            else if (reportType == "MS")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MN_4MONTH.rpt"));
            }
            else if (reportType == "BPRA" || reportType == "DEM")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MN_4MONTH_REVISION.rpt"));
            }
            else if (reportType == "DEQ")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MN_QUARTER_CHAIRMAN.rpt"));
            }
            else if (reportType == "SCP")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MN_PROVISION.rpt"));
            }
            else if (reportType == "DC")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MN_CUMDIVISION.rpt"));
            }
            else if (reportType == "JSY")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MN_JOB4CUMULATIVE.rpt"));
            }
            else if (reportType == "PF")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "CumGrp.rpt"));
            }
            else if (reportType == "JSM")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MN_JB4MONTH.rpt"));
            }
            else if (reportType == "TO")
            {
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "TOCUM.rpt"));
            }
            rd.SetDatabaseLogon("sa", "Admin@123");
            rd.VerifyDatabase();
            rd.Refresh();
            //rd.SetParameterValue(0, "2008");
            //rd.SetParameterValue(1, "17072500");
            //rd.SetParameterValue(2, "%");


            #region ParametersForReports

            if (reportType == "Monthwise" || reportType == "Jobwise" || reportType == "Divisionwise" || reportType == "DEQ" || reportType == "Quarterwise" || reportType == "TO" || reportType == "EG" || reportType == "PF" || reportType == "WK")
            {
                rd.SetParameterValue(0, BPYEAR);
                rd.SetParameterValue(1, JOBNO);
                rd.SetParameterValue(2, "%");
            }
            else if (reportType == "IH" || reportType == "BO" || reportType == "MP" || reportType == "PG" || reportType == "PH" || reportType == "GE" || reportType == "SB")
            {
                rd.SetParameterValue(0, BPYEAR);
                rd.SetParameterValue(1, reportType);
                rd.SetParameterValue(2, JOBNO);
                rd.SetParameterValue(3, "%");
            }
            else if (reportType == "IH" || reportType == "BO" || reportType == "MP" || reportType == "PG" || reportType == "PH" || reportType == "GE" || reportType == "SP")
            {
                rd.SetParameterValue(0, BPYEAR);
                rd.SetParameterValue(1, reportType);
                rd.SetParameterValue(2, JOBNO);
                rd.SetParameterValue(3, "%");
            }
            else if (reportType == "MS" || reportType == "BPRA" || reportType == "JSM" || reportType == "DS")
            {
                rd.SetParameterValue(0, BPYEAR);
                rd.SetParameterValue(1, month);
                rd.SetParameterValue(2, JOBNO);
                rd.SetParameterValue(3, "%");
            }
            else if (reportType == "DC" )
            {
                rd.SetParameterValue(0, BPYEAR);
                rd.SetParameterValue(1, month);
                rd.SetParameterValue(2, JOBNO);
                rd.SetParameterValue(3, "G");
            }
            else if ( reportType == "SCP")
            {
                rd.SetParameterValue(0, BPYEAR);
                rd.SetParameterValue(1, month);
                rd.SetParameterValue(2, JOBNO);
            }
            else if ( reportType == "VOWD")
            {
                rd.SetParameterValue(0, jobno);
                rd.SetParameterValue(1, BPYEAR);
                rd.SetParameterValue(2, month);
                rd.SetParameterValue(3, USER);
            }

            else if (reportType == "J2D" )
            {
                string[] jobNumbers = selectedJobNumbers;
                string formattedJobNumbers = string.Join(",", jobNumbers.Select(jobNumber => $"'{jobNumber.Trim()}'"));
                rd.SetParameterValue(0, formattedJobNumbers);
            }
            else if ( reportType == "JSY")
            {
                rd.SetParameterValue(0, BPYEAR);
                string[] jobNumbers = selectedJobNumbers;
                string formattedJobNumbers = string.Join(",", jobNumbers.Select(jobNumber => $"'{jobNumber.Trim()}'"));
                rd.SetParameterValue(1, formattedJobNumbers);
            }
            ////For Jobwise testing use below parms
            //rd.SetParameterValue(0, "2017");
            //rd.SetParameterValue(1, "14041000");
            //rd.SetParameterValue(2, "%");
            #endregion

            //rd.SetDataSource(allCustomer);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);

            MemoryStream ms = new MemoryStream();
            stream.CopyTo(ms);
            ViewBag.FilePDF = Convert.ToBase64String(ms.ToArray());

            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.DictListVM = result;
            return View(model);


            //Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            //stream.Seek(0, SeekOrigin.Begin);
            //MemoryStream ms = new MemoryStream();
            //stream.CopyTo(ms);
            //string pdfurl = Convert.ToBase64String(ms.ToArray());
            //ViewBag.PDFfile = pdfurl;
            ////return View("Index","", pdfurl);
            ////string fname =  "Yearly report " + 
            ////string fname = $"Report_{BPYEAR}_{JOBNO.Replace(",", "_")}.pdf";

            //return File(stream, "application/pdf", "Report.pdf");
            ////return File(stream, "application/pdf", fname);
        }
    }
    
}