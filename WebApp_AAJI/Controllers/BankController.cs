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
    public class BankController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        private AccountController acm = new AccountController();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();

        // GET: /Bank/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            ViewData["coa"] = db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountName.Contains("bank")).ToList();
            return View(db.banks.ToList());
        }

        // GET: /Bank/Details/5
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
            bank bank = db.banks.Find(id);
            if (bank == null)
            {
                return HttpNotFound();
            }

            var a = db.chartOfAccounts.Where(x => x.id == bank.coaID).Select(x => new{x.accountName}).ToList();
            ViewData["coa"] = a[0].accountName.ToString();
            return View(bank);
        }

        // GET: /Bank/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.coa = new SelectList((db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountName.Contains("bank")).Select(x => new { x.id, x.accountName}).OrderBy(x => x.accountName)), "id", "accountName");
            return View();
        }

        // POST: /Bank/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(bank bank)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            ViewBag.coa = new SelectList((db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountName.Contains("bank")).Select(x => new { x.id, x.accountName }).OrderBy(x => x.accountName)), "id", "accountName", bank.coaID);
            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                bank.createdUser = lvm.userID;
                bank.createdDate = DateTime.Now;
                db.banks.Add(bank);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(bank);
        }

        // GET: /Bank/Edit/5
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
            bank bank = db.banks.Find(id);
            if (bank == null)
            {
                return HttpNotFound();
            }

            ViewBag.coa = new SelectList((db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountName.Contains("bank")).Select(x => new { x.id, x.accountName }).OrderBy(x => x.accountName)), "id", "accountName", bank.coaID);
            return View(bank);
        }

        // POST: /Bank/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="bankID,bankName,accountNo,coaID,createdUser,createdDate,modifiedUser,modifiedDate")] bank bank)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                bank.modifiedUser = lvm.userID;
                bank.modifiedDate = DateTime.Now;
                db.Entry(bank).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.coa = new SelectList((db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountName.Contains("bank")).Select(x => new { x.id, x.accountName }).OrderBy(x => x.accountName)), "id", "accountName", bank.coaID);
            return View(bank);
        }

        // GET: /Bank/Delete/5
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
            bank bank = db.banks.Find(id);
            if (bank == null)
            {
                return HttpNotFound();
            }

            var a = db.chartOfAccounts.Where(x => x.id == bank.coaID).Select(x => new { x.accountName }).ToList();
            ViewData["coa"] = a[0].accountName.ToString();
            return View(bank);
        }

        // POST: /Bank/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            bank bank = db.banks.Find(id);
            db.banks.Remove(bank);
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
