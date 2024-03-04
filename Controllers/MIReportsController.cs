using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BusinessApplication.Controllers
{
    public class MIReportsController : Controller
    {
        // GET: MIReports
        public ActionResult Index()
        {
            return View();
        }

        // GET: MIReports/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: MIReports/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MIReports/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: MIReports/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: MIReports/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: MIReports/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MIReports/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
