using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class BackupDatabaseController : Controller
    {
        private ConnectionController cc = new ConnectionController();
        private AccountController acm = new AccountController();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();

        [HttpGet]
        public async Task<ActionResult> next()
        {
            ViewBag.next = "a";
            //}

            return PartialView("_PartialPagePop2");
        }


        [HttpGet]
        public async Task<ActionResult> backupDB()
        {
            string bakDir = Server.MapPath(cc.backupPath);
            bool isSuccess = cc.backupDB(bakDir);
            //if(isSuccess == true)
            //{
                ViewBag.Backup = isSuccess;
            //}

            return PartialView("_PartialPageBackupDB");
        }

        //
        // GET: /BackupDatabase/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            return View();
        }

        //
        // GET: /BackupDatabase/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /BackupDatabase/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /BackupDatabase/Create
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

        //
        // GET: /BackupDatabase/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /BackupDatabase/Edit/5
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

        //
        // GET: /BackupDatabase/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /BackupDatabase/Delete/5
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
