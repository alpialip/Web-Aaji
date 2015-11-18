using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class TypeFinanceTransactionController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();

        // GET: /TypeFinanceTransaction/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            //ViewData["accountName"] = db.chartOfAccounts.Select(x => new { x.accountNo, x.id }).ToList();
             ViewBag.accountName = new SelectList(db.chartOfAccounts.Select(x => new { x.accountNo, x.id }),"id","accountNo");
            return View(db.typeFinanceTransactions.ToList());
        }

        // GET: /TypeFinanceTransaction/Details/5
        public ActionResult Details(string id)
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
            typeFinanceTransaction typefinancetransaction = db.typeFinanceTransactions.Find(id);
            if (typefinancetransaction == null)
            {
                return HttpNotFound();
            }
            return View(typefinancetransaction);
        }

        // GET: /TypeFinanceTransaction/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.ddlTft = DDLTypeFinanceTransaction("");
            ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");
            return View();
        }

        // POST: /TypeFinanceTransaction/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="tftID,tftName,accountNo,createdDate,createdUser,modifiedDate,modifiedUser")] typeFinanceTransaction typefinancetransaction)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.ddlTft = DDLTypeFinanceTransaction("");
            ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                try
                {                    
                    using (TransactionScope ts = new TransactionScope())
                    {
                        typefinancetransaction.createdDate = DateTime.Now;
                        typefinancetransaction.createdUser = lvm.userID;
                        db.typeFinanceTransactions.Add(typefinancetransaction);
                        db.SaveChanges();
                        ts.Complete();
                    }
                    return RedirectToAction("Index");
                }
                catch(Exception exc)
                {
                    string a = exc.Message;
                }
            }

            return View(typefinancetransaction);
        }

        // GET: /TypeFinanceTransaction/Edit/5
        public ActionResult Edit(int id)
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
            typeFinanceTransaction typefinancetransaction = db.typeFinanceTransactions.Find(id);
            if (typefinancetransaction == null)
            {
                return HttpNotFound();
            }
            ViewBag.sddlTft = DDLTypeFinanceTransaction(typefinancetransaction.tftID);
            ViewBag.sddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName", typefinancetransaction.accountNo);

            return View(typefinancetransaction);
        }

        // POST: /TypeFinanceTransaction/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,tftID,tftName,accountNo,createdDate,createdUser,modifiedDate,modifiedUser")] typeFinanceTransaction typefinancetransaction)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.ddlTft = DDLTypeFinanceTransaction(typefinancetransaction.tftID);
            ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName", typefinancetransaction.accountNo);
            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                try
                {
                    using(TransactionScope ts = new TransactionScope())
                    {
                        typefinancetransaction.modifiedDate = DateTime.Now;
                        typefinancetransaction.modifiedUser = lvm.userID;
                        db.Entry(typefinancetransaction).State = EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    return RedirectToAction("Index");
                }
                catch(Exception exc)
                {
                    string a = exc.Message;
                }
            }
            return View(typefinancetransaction);
        }

        // GET: /TypeFinanceTransaction/Delete/5
        public ActionResult Delete(int id)
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
            typeFinanceTransaction typefinancetransaction = db.typeFinanceTransactions.Find(id);
            if (typefinancetransaction == null)
            {
                return HttpNotFound();
            }
            int accountNo = Convert.ToInt32(typefinancetransaction.accountNo);
            var a = db.chartOfAccounts.Where(x => x.id == accountNo).Select(x => new { x.accountNo }).ToList();
            ViewData["accountNo"] = a[0].accountNo.ToString();
            return View(typefinancetransaction);
        }

        // POST: /TypeFinanceTransaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            typeFinanceTransaction typefinancetransaction = db.typeFinanceTransactions.Find(id);
            db.typeFinanceTransactions.Remove(typefinancetransaction);
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

        public static SelectList DDLTypeFinanceTransaction(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "Cash_In", Text = "Cash In" });
            list.Add(new SelectListItem { Value = "Cash_Out", Text = "Cash Out" });
            list.Add(new SelectListItem { Value = "AR", Text = "Account Receive" });
            list.Add(new SelectListItem { Value = "AP", Text = "Account Payable" });


            return new SelectList(list, "Value", "Text", currSelection);
        }
    }
}
