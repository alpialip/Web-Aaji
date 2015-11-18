using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class EmployeeLoanPersonalController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private ConnectionController cd = new ConnectionController();
        private CommonController cm = new CommonController();

        // GET: /employeeLoans/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            loadDefault(string.Empty);
            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            return View(db.employeeLoans.Where(x=>x.employeeID == lvm.employeeId).ToList());
        }

        // GET: /employeeLoans/Details/5
        public ActionResult Details(int? id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            employeeLoan employeeLoans = db.employeeLoans.Find(id);
            if (employeeLoans == null)
            {
                return HttpNotFound();
            }

            loadDefault(employeeLoans.pinjamanID.ToString());
            return View(employeeLoans);
        }

        // GET: /employeeLoans/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            loadDefault(string.Empty);
            return View();
        }

        // POST: /employeeLoans/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(employeeLoan employeeLoans)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            loadDefault(string.Empty);
            if (ModelState.IsValid)
            {
                bool isProposed = false;
                for (int i = 0; i < Request.Form.Count; i++)
                {
                    if (Request.Form.AllKeys.ToList()[i].Contains("hdnTypeSubmit"))
                        if (Request.Form["hdnTypeSubmit"].ToString() == "requested")
                            isProposed = true;
                }

                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                employeeLoans.createdUser = lvm.userID;
                employeeLoans.createdDate = DateTime.Now;
                employeeLoans.proposedBy = lvm.userID;
                employeeLoans.proposedDate = DateTime.Now;
                employeeLoans.proposedStatus = isProposed;
                db.employeeLoans.Add(employeeLoans);
                db.SaveChanges();
                return RedirectToAction("Index", "EmployeeLoanPersonal");
            }
            return View(employeeLoans);
        }

        // GET: /employeeLoans/Edit/5
        public ActionResult Edit(int? id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            employeeLoan employeeLoans = db.employeeLoans.Find(id);
            if (employeeLoans == null)
            {
                return HttpNotFound();
            }

            loadDefault(employeeLoans.pinjamanID.ToString());
            return View(employeeLoans);
        }

        // POST: /employeeLoans/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(employeeLoan employeeLoans)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            loadDefault(employeeLoans.pinjamanID.ToString());
            if (ModelState.IsValid)
            {
                bool isProposed = false;
                for (int i = 0; i < Request.Form.Count; i++)
                {
                    if (Request.Form.AllKeys.ToList()[i].Contains("hdnTypeSubmit"))
                        if (Request.Form["hdnTypeSubmit"].ToString() == "requested")
                            isProposed = true;
                }

                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                employeeLoans.modifiedUser = lvm.userID;
                employeeLoans.modifiedDate = DateTime.Now;
                employeeLoans.proposedBy = lvm.userID;
                employeeLoans.proposedDate = DateTime.Now;
                employeeLoans.proposedStatus = isProposed;
                db.Entry(employeeLoans).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "EmployeeLoanPersonal");
            }
            return View(employeeLoans);
        }

        // GET: /employeeLoans/Delete/5
        public ActionResult Delete(int? id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            employeeLoan employeeLoans = db.employeeLoans.Find(id);
            if (employeeLoans == null)
            {
                return HttpNotFound();
            }

            loadDefault(employeeLoans.pinjamanID.ToString());
            return View(employeeLoans);
        }

        // POST: /employeeLoans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            employeeLoan employeeLoans = db.employeeLoans.Find(id);
            db.employeeLoans.Remove(employeeLoans);
            db.SaveChanges();
            return RedirectToAction("Index", "EmployeeLoanPersonal");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public void loadDefault(string pinjamanID)
        {
            ViewBag.employeeList = db.employees.AsNoTracking().ToList();
            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            ViewBag.empName = lvm.employeeName;
            ViewBag.empId = lvm.employeeId;

            #region old proses ~salah divisiID dengan deptID
            //var dept = db.departments.AsNoTracking().Where(x=>x.deptID == lvm.deptID).ToList();
            //ViewBag.empDeptName = lvm.isAdmin == true ? "" : dept[0].deptName;

            //Perbaikannya
            var dept = db.departments.AsNoTracking().Where(x => x.deptID == lvm.divID).ToList();
            ViewBag.empDeptName = lvm.isAdmin == true ? "" : dept[0].deptName;
            #endregion
            var emp = db.employees.AsNoTracking().Where(x=>x.employeeID == lvm.employeeId).ToList();
            ViewBag.empGapok = lvm.isAdmin == true ? "" : emp[0].gapok.ToString();
            ViewBag.LoanCategory = cm.ddlLoanCategory(string.Empty);

            if (pinjamanID != string.Empty)
            {
                var id = int.Parse(pinjamanID);
                var dataSaved = db.employeeLoans.AsNoTracking().Where(x => x.pinjamanID == id && x.employeeID == lvm.employeeId)
                            .Join(db.employees, q => q.employeeID, r => r.employeeID, (q, r) => new { q, r })
                            .Join(db.employeePositions, s => s.q.employeeID, t => t.employeeID, (s, t) => new { s, t })
                            .Join(db.departments, u => u.t.deptID, v => v.deptID, (u, v) => new { u, v })
                            .Select(c => new { c.u.s.r.employeeName, c.u.s.r.employeeNIK, c.u.t.deptID, c.v.deptName, c.u.s.q.proposedBy, c.u.s.q.loanCategory, c.u.s.r.gapok }).ToList();

                ViewBag.empName = dataSaved[0].employeeName;
                ViewBag.empDeptName = dataSaved[0].deptName;
                ViewBag.empGapok = dataSaved[0].gapok;

                //lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                //var a = db.departments.Where(x => x.deptID == lvm.deptID).Select(x => new { x.deptName }).ToList();
                //ViewBag.Dept = a[0].deptName;

                var userID = dataSaved[0].proposedBy;
                var b = db.Users.Where(x => x.userID == userID).Select(x => new { x.userName }).ToList();
                ViewBag.UserName = b[0].userName;
                ViewBag.UserID = userID;

                ViewBag.LoanCategory = cm.ddlLoanCategory(dataSaved[0].loanCategory);
            }
        }

	}
}