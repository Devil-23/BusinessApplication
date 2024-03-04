using BusinessApplication.CustomFilter;
using BusinessApplication.Models;
using BusinessApplication.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BusinessApplication.Controllers
{
    [SessionAuth]
    public class UserAuthorizationController : Controller
    {
        BusinessPlanEntities cn = new BusinessPlanEntities();
        // GET: UserAuthorization
        [HttpGet]
        public ActionResult Index()
        {
            string user = Convert.ToString(Session["UserId"]);
            ViewBag.BP_USER = new SelectList(cn.TBL_BP_USERMASTER.ToList(), "BP_USER", "BP_USER");
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();

            var query = from a in cn.TBL_BP_DICTIONARY
                        join b in cn.TBL_BP_PERMISSION
                        on a.JOB_NO equals b.JOB_NO into permissionGroup
                        from b in permissionGroup.Where(x => x.BP_USER == user).DefaultIfEmpty()
                        select new
                        {
                            modifiedon = a.ModifiedOn,
                            JOB_NO = a.JOB_NO,
                            BP_READ = (b != null) ? b.BP_READ : 0,
                            BP_WRITE = (b != null) ? b.BP_WRITE : 0,
                            MN_READ = (b != null) ? b.MN_READ : 0,
                            MN_WRITE = (b != null) ? b.MN_WRITE : 0,
                            PJ_READ = (b != null) ? b.PJ_READ : 0,
                            PJ_WRITE = (b != null) ? b.PJ_WRITE : 0
                        };


            var res = query.AsEnumerable().Select(x => new PermissionVM
            {
                ModifiedOn = x.modifiedon,
                JOB_NO = x.JOB_NO,
                BP_READ = Convert.ToBoolean(x.BP_READ),
                BP_WRITE = Convert.ToBoolean(x.BP_WRITE),
                MN_READ = Convert.ToBoolean(x.MN_READ),
                MN_WRITE = Convert.ToBoolean(x.MN_WRITE),
                PJ_READ = Convert.ToBoolean(x.PJ_READ),
                PJ_WRITE = Convert.ToBoolean(x.PJ_WRITE)
            }).OrderBy(x=>x.JOB_NO).ToList();
            model.PermList = res.ToList();
            model.DictListVM = result;
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(MultipleVM rec)
        {
           
            string BP_USER = Convert.ToString(Session["User"]);
            ViewBag.BP_USER = new SelectList(cn.TBL_BP_USERMASTER.ToList(), "BP_USER", "BP_USER",BP_USER);
            if (ModelState.IsValid)
            {
                using (var transaction = this.cn.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var data in rec.PermList)
                        {
                            TBL_BP_PERMISSION existingEntity = cn.TBL_BP_PERMISSION
                                .SingleOrDefault(e => e.BP_USER == BP_USER && e.JOB_NO == data.JOB_NO);

                            if (existingEntity == null)
                            {
                                TBL_BP_PERMISSION newEntity = new TBL_BP_PERMISSION
                                {
                                    JOB_NO = data.JOB_NO,
                                    BP_USER = BP_USER,
                                    BP_READ = data.BP_READ ? 1 : 0,
                                    BP_WRITE = data.BP_WRITE ? 1 : 0,
                                    MN_READ = data.MN_READ ? 1 : 0,
                                    MN_WRITE = data.MN_WRITE ? 1 : 0,
                                    PJ_READ = data.PJ_READ ? 1 : 0,
                                    PJ_WRITE = data.PJ_WRITE ? 1 : 0
                                };

                                this.cn.TBL_BP_PERMISSION.Add(newEntity);
                            }
                            else
                            {
                                existingEntity.BP_READ = data.BP_READ ? 1 : 0;
                                existingEntity.BP_WRITE = data.BP_WRITE ? 1 : 0;
                                existingEntity.MN_READ = data.MN_READ ? 1 : 0;
                                existingEntity.MN_WRITE = data.MN_WRITE ? 1 : 0;
                                existingEntity.PJ_READ = data.PJ_READ ? 1 : 0;
                                existingEntity.PJ_WRITE = data.PJ_WRITE ? 1 : 0;
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
            }

            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.DictListVM = result;
            var query = from a in cn.TBL_BP_DICTIONARY
                        join b in cn.TBL_BP_PERMISSION
                        on a.JOB_NO equals b.JOB_NO into permissionGroup
                        from b in permissionGroup.Where(x => x.BP_USER == BP_USER).DefaultIfEmpty()
                        select new
                        {
                            modifiedon = a.ModifiedOn,
                            JOB_NO = a.JOB_NO,
                            BP_READ = (b != null) ? b.BP_READ : 0,
                            BP_WRITE = (b != null) ? b.BP_WRITE : 0,
                            MN_READ = (b != null) ? b.MN_READ : 0,
                            MN_WRITE = (b != null) ? b.MN_WRITE : 0,
                            PJ_READ = (b != null) ? b.PJ_READ : 0,
                            PJ_WRITE = (b != null) ? b.PJ_WRITE : 0
                        };


            var res = query.AsEnumerable().Select(x => new PermissionVM
            {
                ModifiedOn = x.modifiedon,
                JOB_NO = x.JOB_NO,
                BP_READ = Convert.ToBoolean(x.BP_READ),
                BP_WRITE = Convert.ToBoolean(x.BP_WRITE),
                MN_READ = Convert.ToBoolean(x.MN_READ),
                MN_WRITE = Convert.ToBoolean(x.MN_WRITE),
                PJ_READ = Convert.ToBoolean(x.PJ_READ),
                PJ_WRITE = Convert.ToBoolean(x.PJ_WRITE)
            }).OrderByDescending(x => x.ModifiedOn).ToList();

            model.PermList = res.ToList();
            return View(model);
        }
        public ActionResult SearchByName(string BP_USER)
        {
            Session["User"] = BP_USER;
            ViewBag.BP_USER = new SelectList(cn.TBL_BP_USERMASTER.ToList(), "BP_USER", "BP_USER");
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            var query = from a in cn.TBL_BP_DICTIONARY
                        join b in cn.TBL_BP_PERMISSION
                        on a.JOB_NO equals b.JOB_NO into permissionGroup
                        from b in permissionGroup.Where(x=>x.BP_USER==BP_USER).DefaultIfEmpty()
                        select new
                        {
                            modifiedon=a.ModifiedOn,
                            JOB_NO = a.JOB_NO,
                            BP_READ = (b != null) ? b.BP_READ : 0,
                            BP_WRITE = (b != null) ? b.BP_WRITE : 0,
                            MN_READ = (b != null) ? b.MN_READ : 0,
                            MN_WRITE = (b != null) ? b.MN_WRITE : 0,
                            PJ_READ = (b != null) ? b.PJ_READ : 0,
                            PJ_WRITE = (b != null) ? b.PJ_WRITE : 0
                        };


            var res = query.AsEnumerable().Select(x => new PermissionVM
            {
                ModifiedOn=x.modifiedon,
                JOB_NO = x.JOB_NO,
                BP_READ = Convert.ToBoolean(x.BP_READ),
                BP_WRITE = Convert.ToBoolean(x.BP_WRITE),
                MN_READ = Convert.ToBoolean(x.MN_READ),
                MN_WRITE = Convert.ToBoolean(x.MN_WRITE),
                PJ_READ = Convert.ToBoolean(x.PJ_READ),
                PJ_WRITE = Convert.ToBoolean(x.PJ_WRITE)
            }).OrderByDescending(x=>x.ModifiedOn).ToList();

            model.PermList = res.ToList();
            model.DictListVM = result;
            return View("Index", model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.DictListVM = result;
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(MultipleVM rec)
        {
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.DictListVM = result;
            TBL_BP_USERMASTER existingEntity = cn.TBL_BP_USERMASTER.FirstOrDefault(e => e.BP_USER == rec.UserTbl.BP_USER);
            if (ModelState.IsValid)
            {
                if (existingEntity == null)
                {
                    TBL_BP_USERMASTER res = new TBL_BP_USERMASTER();
                    res.BP_USER = rec.UserTbl.BP_USER;
                    res.BP_PASSWD = rec.UserTbl.BP_PASSWD;
                    cn.TBL_BP_USERMASTER.Add(res);
                    this.cn.SaveChanges();
                    return View();
                }
                {
                    ModelState.AddModelError("", "User Already Exist");
                }
            }
            return View(model);
        }



        public ActionResult AccessDenied()
        {
            List<TBL_BP_DICTIONARY> result = new List<TBL_BP_DICTIONARY>();
            result = this.cn.TBL_BP_DICTIONARY.ToList();
            MultipleVM model = new MultipleVM();
            model.GlobalList = cn.PROC_BP_SHOW_MASTER("MT");
            model.DictListVM = result;
            return PartialView(model);
        }
    }
}