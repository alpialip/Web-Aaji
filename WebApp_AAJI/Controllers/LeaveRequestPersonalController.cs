using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class LeaveRequestPersonalController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        private AccountController acm = new AccountController();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private ConnectionController con = new ConnectionController();
        int maxRows = 10;

        [HttpGet]
        public async Task<ActionResult> LoadDetailEmployeeSearch(string keywordId, string keywordSearch /*, int maxRows*/, int pageStart)
        {
            loadPopUpResult(keywordId, keywordSearch, pageStart);

            return PartialView("_PartialPageEmployeeSearchSub");
        }

        public void loadPopUpResult(string keywordId, string keywordSearch, int pageStart)
        {
            ////[employeeID],[employeeName],[employeeNIK],[email],[npwp],[ktp],[address],[city],[bankName],[rekening],[tempatLahir],[tanggalLahir]
            ////,[linkFilePhoto],[positionDate],[positionId],[resignDate],[divisiID],[deptName],[deptID],[divisiName],[gapok],[levelID],[employeeStatus]
            string sql = "WITH cte AS ( ";
            sql += "SELECT ROW_NUMBER() OVER(ORDER BY employeeID) AS rn, ";
            sql += "[employeeID], [employeeNIK], [employeeName], [deptName], [employeeStatus] ";
            sql += "FROM [dbo].[v_employee] b  WITH(NOLOCK) ";
            sql += "WHERE b.resignDate IS NULL ";

            if (!string.IsNullOrEmpty(keywordId) && !string.IsNullOrEmpty(keywordSearch))
            {
                if (keywordId == "employeeStatus")
                    sql += "AND " + keywordId + " = " + keywordSearch;
                else
                    sql += "AND " + keywordId + " LIKE '%" + keywordSearch + "%'";
            }
            sql += ") ";
            sql += "SELECT * FROM cte ";
            string sqlAll = sql;
            sql += "WHERE rn BETWEEN " + ((pageStart * maxRows) - (maxRows - 1)) + " AND " + (pageStart * maxRows);

            DataTable dataAll = con.executeReader(sqlAll);
            ViewBag.totalRows = dataAll.Rows.Count;
            ViewBag.currentPage = pageStart;

            DataTable popUp = con.executeReader(sql);
            ViewBag.resultPopUp = popUp;
        }

        // GET: /LeaveRequest/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            return View(db.transLeaveRequests.ToList());
        }

        // GET: /LeaveRequest/Details/5
        public ActionResult Details(int? id, int empId)
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
            var tlq = db.transLeaveRequests.Where(x => x.leaveRequestID == id && x.employeeID == empId).ToList();
            if (tlq == null)
            {
                return HttpNotFound();
            }

            transLeaveRequest transleaverequest = new transLeaveRequest();
            foreach (var a in tlq as IEnumerable<transLeaveRequest>)
            {
                transleaverequest.acknowledgeBy = a.acknowledgeBy;
                transleaverequest.acknowledgeDate = a.acknowledgeDate;
                transleaverequest.acknowledgeStatus = a.acknowledgeStatus;
                transleaverequest.approvedBy = a.approvedBy;
                transleaverequest.approvedDate = a.approvedDate;
                transleaverequest.approvedStatus = a.approvedStatus;
                transleaverequest.createdDate = a.createdDate;
                transleaverequest.createdUser = a.createdUser;
                transleaverequest.employeeID = a.employeeID;
                transleaverequest.leaveDescription = a.leaveDescription;
                transleaverequest.leaveEndDate = a.leaveEndDate;
                transleaverequest.leaveRequestID = a.leaveRequestID;
                transleaverequest.leaveStartDate = a.leaveStartDate;
                transleaverequest.modifiedDate = a.modifiedDate;
                transleaverequest.modifiedUser = a.modifiedUser;
            }
            return View(transleaverequest);
        }

        // GET: /LeaveRequest/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            loadPopUpResult(string.Empty, string.Empty, 1);
            return View();
        }

        // POST: /LeaveRequest/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(transLeaveRequest transleaverequest)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                transleaverequest.createdDate = DateTime.Now;
                transleaverequest.createdUser = lvm.userID;
                db.transLeaveRequests.Add(transleaverequest);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(transleaverequest);
        }

        // GET: /LeaveRequest/Edit/5
        public ActionResult Edit(int? id, int empId)
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
            var tlq = db.transLeaveRequests.Where(x => x.leaveRequestID == id && x.employeeID == empId).ToList();
            if (tlq == null)
            {
                return HttpNotFound();
            }

            transLeaveRequest transleaverequest = new transLeaveRequest();
            foreach (var a in tlq as IEnumerable<transLeaveRequest>)
            {
                transleaverequest.acknowledgeBy = a.acknowledgeBy;
                transleaverequest.acknowledgeDate = a.acknowledgeDate;
                transleaverequest.acknowledgeStatus = a.acknowledgeStatus;
                transleaverequest.approvedBy = a.approvedBy;
                transleaverequest.approvedDate = a.approvedDate;
                transleaverequest.approvedStatus = a.approvedStatus;
                transleaverequest.createdDate = a.createdDate;
                transleaverequest.createdUser = a.createdUser;
                transleaverequest.employeeID = a.employeeID;
                transleaverequest.leaveDescription = a.leaveDescription;
                transleaverequest.leaveEndDate = a.leaveEndDate;
                transleaverequest.leaveRequestID = a.leaveRequestID;
                transleaverequest.leaveStartDate = a.leaveStartDate;
                transleaverequest.modifiedDate = a.modifiedDate;
                transleaverequest.modifiedUser = a.modifiedUser;

                var employeeName = db.employees.Where(x => x.employeeID == a.employeeID).ToList();
                ViewBag.employeeName = employeeName[0].employeeName;
            }
            return View(transleaverequest);
        }

        // POST: /LeaveRequest/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(transLeaveRequest transleaverequest)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                transleaverequest.modifiedDate = DateTime.Now;
                transleaverequest.modifiedUser = lvm.userID;
                db.Entry(transleaverequest).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(transleaverequest);
        }

        // GET: /LeaveRequest/Delete/5
        public ActionResult Delete(int? id, int empId)
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

            var tlq = db.transLeaveRequests.Where(x => x.leaveRequestID == id && x.employeeID == empId).ToList();
            if (tlq == null)
            {
                return HttpNotFound();
            }

            transLeaveRequest transleaverequest = new transLeaveRequest();
            foreach (var a in tlq as IEnumerable<transLeaveRequest>)
            {
                transleaverequest.acknowledgeBy = a.acknowledgeBy;
                transleaverequest.acknowledgeDate = a.acknowledgeDate;
                transleaverequest.acknowledgeStatus = a.acknowledgeStatus;
                transleaverequest.approvedBy = a.approvedBy;
                transleaverequest.approvedDate = a.approvedDate;
                transleaverequest.approvedStatus = a.approvedStatus;
                transleaverequest.createdDate = a.createdDate;
                transleaverequest.createdUser = a.createdUser;
                transleaverequest.employeeID = a.employeeID;
                transleaverequest.leaveDescription = a.leaveDescription;
                transleaverequest.leaveEndDate = a.leaveEndDate;
                transleaverequest.leaveRequestID = a.leaveRequestID;
                transleaverequest.leaveStartDate = a.leaveStartDate;
                transleaverequest.modifiedDate = a.modifiedDate;
                transleaverequest.modifiedUser = a.modifiedUser;
            }
            return View(transleaverequest);
        }

        // POST: /LeaveRequest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, int empId)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            db.transLeaveRequests.RemoveRange(db.transLeaveRequests.Where(x => x.employeeID == empId && x.leaveRequestID == id));
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