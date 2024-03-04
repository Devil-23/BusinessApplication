using BusinessApplication.CustomFilter;
using BusinessApplication.Models;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BusinessApplication.Controllers
{ 
    [SessionAuth]
    public class ReportController : Controller
    {
        BusinessPlanEntities cn = new BusinessPlanEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MonthwiseReport()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ExportReport()
        {
            // Load the Crystal Report
            ReportDocument report = new ReportDocument();
            
            report.Load(@"E:\Work\New folder\BusinessApplication\Reports\BP_MONTHWISE.rpt");

            // Set export options
            ExportOptions exportOptions = new ExportOptions();
            exportOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
            exportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
            //exportOptions.ExportDestinationOptions.DiskFilePath = @"E:\Work\New folder\BusinessApplication\Reports\YourReport.pdf";

            // Export the report to PDF
            report.Export(exportOptions);

            // Handle the exported PDF file
            // You can display the PDF in a viewer, send it as an email attachment, or save it to a permanent location.

            return View();
        }

      
      public ActionResult MonthReport()
        {
            return View(this.cn.TBL_MN_ACTUALS.ToList());
        }
        //public ActionResult ExportMonthWiseData()
        //{
        //    ReportDocument rd = new ReportDocument();
        //    rd.Load(Path.Combine(Server.MapPath("~/Report"), "Demo"));
        //}
    }
}