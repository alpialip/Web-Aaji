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
    public class DivisiController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        private AccountController acm = new AccountController();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();

        // GET: /Divisi/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewData["Dept"] = db.departments.ToList();
            return View(db.divisis.OrderBy(x => new { x.deptID, x.divisiName }).ToList());
        }

        // GET: /Divisi/Details/5
        public ActionResult Details(int? id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            divisi divisi = db.divisis.Find(id);
            if (divisi == null)
            {
                return HttpNotFound();
            }

            ViewBag.Dept = new SelectList(db.departments.Select(x => new { x.deptID, x.deptName }).OrderBy(x => x.deptName), "deptID", "deptName", divisi.divisiID);
            return View(divisi);
        }

        // GET: /Divisi/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.Dept = new SelectList(db.departments.Select(x=> new {x.deptID, x.deptName}).OrderBy(x=>x.deptName),"deptID","deptName");
            return View();
        }

        // POST: /Divisi/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="divisiID,deptID,divisiName,createdUser,createdDate,modifiedUser,modifiedDate")] divisi divisi)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.Dept = new SelectList(db.departments.Select(x => new { x.deptID, x.deptName }).OrderBy(x => x.deptName), "deptID", "deptName",divisi.divisiID);
            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                divisi.createdUser = lvm.userID;
                divisi.createdDate = DateTime.Now;
                db.divisis.Add(divisi);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(divisi);
        }

        // GET: /Divisi/Edit/5
        public ActionResult Edit(int? id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            divisi divisi = db.divisis.Find(id);
            if (divisi == null)
            {
                return HttpNotFound();
            }

            ViewBag.Dept = new SelectList(db.departments.Select(x => new { x.deptID, x.deptName }).OrderBy(x => x.deptName), "deptID", "deptName", divisi.divisiID);
            return View(divisi);
        }

        // POST: /Divisi/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="divisiID,deptID,divisiName,createdUser,createdDate,modifiedUser,modifiedDate")] divisi divisi)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.Dept = new SelectList(db.departments.Select(x => new { x.deptID, x.deptName }).OrderBy(x => x.deptName), "deptID", "deptName", divisi.divisiID);
            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                divisi.modifiedUser = lvm.userID;
                divisi.modifiedDate = DateTime.Now;
                db.Entry(divisi).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(divisi);
        }

        // GET: /Divisi/Delete/5
        public ActionResult Delete(int? id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            divisi divisi = db.divisis.Find(id);
            if (divisi == null)
            {
                return HttpNotFound();
            }
            return View(divisi);
        }

        // POST: /Divisi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            divisi divisi = db.divisis.Find(id);
            db.divisis.Remove(divisi);
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
