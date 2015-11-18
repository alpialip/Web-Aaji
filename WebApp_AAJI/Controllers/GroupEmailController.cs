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
    public class GroupEmailController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();

        // GET: /GroupEmail/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.email = db.emails.OrderBy(a=>a.emailName) as IEnumerable<email>;
            return View(db.groupEmails.ToList());
        }

        // GET: /GroupEmail/Details/5
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
            groupEmail groupemail = db.groupEmails.Find(id);
            if (groupemail == null)
            {
                return HttpNotFound();
            }
            ViewBag.email = db.emails.OrderBy(a => a.emailName) as IEnumerable<email>;
            return View(groupemail);
        }

        // GET: /GroupEmail/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            string[] a = new string[]{};
            displayEmailDistributed(a);
            return View();
        }

        // POST: /GroupEmail/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="groupEmailID,groupEmailName,emailAddress,modifiedDate,modifiedUser")] groupEmail groupemail)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            string[] a = new string[] { };
            displayEmailDistributed(a);
            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;

                #region collect email distributed
                var countChk = 0;
                for (int i = 0; i < Request.Form.Count; i++)
                {
                    if (Request.Form.AllKeys.ToList()[i].Contains(".Selected"))
                        countChk++;
                }

                string checkedList = string.Empty;
                for (int i = 0; i < countChk; i++)
                {
                    var checkboxValue = Request.Form["[" + i + "].Selected"].Split(',');

                    if (checkboxValue[0].ToString().ToLower() != "false")
                    {
                        checkedList += checkboxValue[0].ToString() + ",";
                    }
                }
                checkedList = checkedList.Substring(0, checkedList.Length - 1);
                groupemail.emailAddress = checkedList;
                #endregion 

                groupemail.modifiedDate = DateTime.Now;
                groupemail.modifiedUser = lvm.userID;
                db.groupEmails.Add(groupemail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(groupemail);
        }

        // GET: /GroupEmail/Edit/5
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
            groupEmail groupemail = db.groupEmails.Find(id);
            if (groupemail == null)
            {
                return HttpNotFound();
            }

            var emailDistributedSaved = db.groupEmails.Where(g => g.groupEmailID == groupemail.groupEmailID).Select(a => a.emailAddress).ToList();
            string[] ax = emailDistributedSaved[0].ToString().Split(',');
            displayEmailDistributed(ax);

            return View(groupemail);
        }

        // POST: /GroupEmail/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="groupEmailID,groupEmailName,emailAddress,modifiedDate,modifiedUser")] groupEmail groupemail)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            var emailDistributedSaved = db.groupEmails.Where(g => g.groupEmailID == groupemail.groupEmailID).Select(a => a.emailAddress).ToList();
            string[] ax = emailDistributedSaved[0].ToString().Split(',');
            displayEmailDistributed(ax);

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;

                #region collect email distributed
                var countChk = 0;
                for (int i = 0; i < Request.Form.Count; i++)
                {
                    if (Request.Form.AllKeys.ToList()[i].Contains(".Selected"))
                        countChk++;
                }

                string checkedList = string.Empty;
                for (int i = 0; i < countChk; i++)
                {
                    var checkboxValue = Request.Form["[" + i + "].Selected"].Split(',');

                    if (checkboxValue[0].ToString().ToLower() != "false")
                    {
                        checkedList += checkboxValue[0].ToString() + ",";
                    }
                }
                checkedList = checkedList.Substring(0, checkedList.Length - 1);
                groupemail.emailAddress = checkedList;
                #endregion 

                groupemail.modifiedDate = DateTime.Now;
                groupemail.modifiedUser = lvm.userID;

                db.Entry(groupemail).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(groupemail);
        }

        // GET: /GroupEmail/Delete/5
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
            groupEmail groupemail = db.groupEmails.Find(id);
            if (groupemail == null)
            {
                return HttpNotFound();
            }
            return View(groupemail);
        }

        // POST: /GroupEmail/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            groupEmail groupemail = db.groupEmails.Find(id);
            db.groupEmails.Remove(groupemail);
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

        protected void displayEmailDistributed(string[] loadSaved)
        {
            var model = new groupEmail();
            IEnumerable<email> emailColect = db.emails;

            if (loadSaved.Length == 0)
            {
                foreach (email a in emailColect)
                {
                    var listEmail = new groupEmail.SelectEmailDistributed()
                    {
                        Id = a.emailID.ToString(),
                        Name = a.emailName + " (" + a.emailAddress + ")",
                        Selected = false
                    };
                    model.emailDistributed.Add(listEmail);
                }
            }
            else
            {
                foreach (email a in emailColect)
                {
                    var listEmail = new groupEmail.SelectEmailDistributed()
                    {
                        Id = a.emailID.ToString(),
                        Name = a.emailName + " (" + a.emailAddress + ")",
                        Selected = loadSaved.Contains(a.emailID.ToString()) ? true : false
                    };
                    model.emailDistributed.Add(listEmail);
                }
            }
            ViewData["emailDistributed"] = model.emailDistributed.ToList();
        }
    }
}
