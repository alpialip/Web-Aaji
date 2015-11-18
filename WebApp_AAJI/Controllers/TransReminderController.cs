using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class TransReminderController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private CommonController ccm = new CommonController();

        // GET: /TransReminder/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            ////Karena masuk Ke dalam menu personal jadi ga usah 
            //lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            //if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
            //    return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            return View(db.transReminders.Where(x=>x.createdUser == lvm.userID).ToList());
        }

        // GET: /TransReminder/Details/5
        public ActionResult Details(int? id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            ////Karena masuk Ke dalam menu personal jadi ga usah 
            //lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            //if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
            //    return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            transReminder transreminder = db.transReminders.Find(id);
            if (transreminder == null)
            {
                return HttpNotFound();
            }
            return View(transreminder);
        }

        // GET: /TransReminder/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            ////Karena masuk Ke dalam menu personal jadi ga usah 
            //lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            //if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
            //    return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.repeat = ccm.ddlReminderRepetition(string.Empty);
            ViewBag.remind = ccm.ddlReminder(string.Empty);
            return View();
        }

        // POST: /TransReminder/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,subject,email,reminderDate,reminderTime,repeatitionID,reminderID,createdUser,createdDate,modifiedUser,modifiedDate")] transReminder transreminder)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            ////Karena masuk Ke dalam menu personal jadi ga usah 
            //lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            //if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
            //    return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.repeat = ccm.ddlReminderRepetition(transreminder.repeatitionID);
            ViewBag.remind = ccm.ddlReminder(transreminder.reminderID);
            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                transreminder.createdUser = lvm.userID;
                transreminder.createdDate = DateTime.Now;
                db.transReminders.Add(transreminder);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(transreminder);
        }

        // GET: /TransReminder/Edit/5
        public ActionResult Edit(int? id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            ////Karena masuk Ke dalam menu personal jadi ga usah 
            //lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            //if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
            //    return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            transReminder transreminder = db.transReminders.Find(id);
            if (transreminder == null)
            {
                return HttpNotFound();
            }

            ViewBag.repeat = ccm.ddlReminderRepetition(transreminder.repeatitionID);
            ViewBag.remind = ccm.ddlReminder(transreminder.reminderID);
            return View(transreminder);
        }

        // POST: /TransReminder/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,subject,email,reminderDate,reminderTime,repeatitionID,reminderID,createdUser,createdDate,modifiedUser,modifiedDate")] transReminder transreminder)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            ////Karena masuk Ke dalam menu personal jadi ga usah 
            //lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            //if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
            //    return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.repeat = ccm.ddlReminderRepetition(transreminder.repeatitionID);
            ViewBag.remind = ccm.ddlReminder(transreminder.reminderID);
            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                transreminder.modifiedUser = lvm.userID;
                transreminder.modifiedDate = DateTime.Now;
                db.Entry(transreminder).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(transreminder);
        }

        // GET: /TransReminder/Delete/5
        public ActionResult Delete(int? id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            ////Karena masuk Ke dalam menu personal jadi ga usah 
            //lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            //if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
            //    return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            transReminder transreminder = db.transReminders.Find(id);
            if (transreminder == null)
            {
                return HttpNotFound();
            }

            ViewBag.menuExclusive = ccm.ddlReminderRepetition(transreminder.repeatitionID);
            ViewBag.menuExclusive = ccm.ddlReminder(transreminder.reminderID);
            return View(transreminder);
        }

        // POST: /TransReminder/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            ////Karena masuk Ke dalam menu personal jadi ga usah 
            //lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            //if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
            //    return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            transReminder transreminder = db.transReminders.Find(id);
            db.transReminders.Remove(transreminder);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
