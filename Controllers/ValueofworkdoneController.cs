using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BusinessApplication.Controllers
{
    public class ValueofworkdoneController : Controller
    {
        // GET: Valueofworkdone
        public ActionResult Index()
        {
            return View();
        }

        // GET: Valueofworkdone/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Valueofworkdone/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Valueofworkdone/Create
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

        // GET: Valueofworkdone/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Valueofworkdone/Edit/5
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

        // GET: Valueofworkdone/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Valueofworkdone/Delete/5
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
