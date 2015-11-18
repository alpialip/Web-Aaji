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
    public class SetupEmailController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();

        // GET: /SetupEmail/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            var dbId = db.setupEmails.ToList();
            int id = 0;
            if(dbId.Count > 0)
                id = dbId[0].id;
            
            setupEmail setupemail = db.setupEmails.Find(id);

            //if (setupemail == null)
            //{
            //    return HttpNotFound();
            //}
            return View(setupemail);
            //return View(db.setupEmails.ToList());
        }
        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index([Bind(Include = "id,host,port,emailName,emailAddress,modifiedDate,modifiedUser")] setupEmail setupemail)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                if(setupemail.id==0)
                    for (int i = 0; i < Request.Form.Count; i++)
                    {
                        if (Request.Form.AllKeys.ToList()[i].Contains("id"))
                        {
                            setupemail.id = Convert.ToInt32(Request.Form["id"].ToString());
                            break;
                        }
                    }

                setupemail.modifiedDate = DateTime.Now;
                setupemail.modifiedUser = "";
                var isAdded = db.setupEmails.AsNoTracking().Where(x => x.id == setupemail.id).ToList();

                if (isAdded.Count > 0)
                {
                    db.Entry(setupemail).State = EntityState.Modified;
                }
                else
                {
                    db.setupEmails.Add(setupemail);
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(setupemail);
        }

        // GET: /SetupEmail/Details/5
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
            setupEmail setupemail = db.setupEmails.Find(id);
            if (setupemail == null)
            {
                return HttpNotFound();
            }
            return View(setupemail);
        }

        // GET: /SetupEmail/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            return View();
        }

        // POST: /SetupEmail/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="host,port,emailName,emailAddress,modifiedDate,modifiedUser")] setupEmail setupemail)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                setupemail.modifiedDate = DateTime.Now;
                setupemail.modifiedUser = lvm.userID;
                db.setupEmails.Add(setupemail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(setupemail);
        }

        // GET: /SetupEmail/Edit/5
        public ActionResult Edit(string id)
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
            setupEmail setupemail = db.setupEmails.Find(id);
            if (setupemail == null)
            {
                return HttpNotFound();
            }
            return View(setupemail);
        }

        // POST: /SetupEmail/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="host,port,emailName,emailAddress,modifiedDate,modifiedUser")] setupEmail setupemail)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                setupemail.modifiedUser = lvm.userID;
                setupemail.modifiedDate = DateTime.Now;
                db.Entry(setupemail).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(setupemail);
        }

        // GET: /SetupEmail/Delete/5
        public ActionResult Delete(string id)
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
            setupEmail setupemail = db.setupEmails.Find(id);
            if (setupemail == null)
            {
                return HttpNotFound();
            }
            return View(setupemail);
        }

        // POST: /SetupEmail/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            setupEmail setupemail = db.setupEmails.Find(id);
            db.setupEmails.Remove(setupemail);
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
