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
using PagedList;

namespace WebApp_AAJI.Controllers
{
    public class EmployeeLoanController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private ConnectionController cd = new ConnectionController();
        private CommonController cm = new CommonController();
        
        [HttpGet]
        public async Task<ActionResult> ApprovalProcess(string act, string id)
        {
            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            int i_id = int.Parse(id);
            employeeLoan model = db.employeeLoans.Find(i_id);
            model.approvedStatus = act == "approved" ? true : false;
            model.approvedDate = DateTime.Now;
            model.approvedBy = lvm.userID;
            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: /employeeLoans/
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, string startDateFilter, string startDate, string endDateFilter, string endDate, int? page)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.ApprovalAuth = false;
            foreach (string b in lvm.listAuthorize as List<string>)
            {
                string[] auth = b.Split('_');
                if (auth[0].ToString() == "A1")
                    if (b.Contains((Url.Action("")).Replace("/", "")))
                    {
                        ViewBag.ApprovalAuth = true;
                    }
            }
            if (ViewBag.ApprovalAuth == false && lvm.isAdmin == true)
                ViewBag.ApprovalAuth = true;

            loadDefault(string.Empty);

            #region paging
            ViewBag.CurrentSort = sortOrder;
            ViewBag.VoucherSortParm = String.IsNullOrEmpty(sortOrder) ? "date_" : "";
            ViewBag.DateSortParm = sortOrder == "voucherNo" ? "voucherNo_" : "voucherNo";

            if (searchString != null || (startDate != null && endDate != null))
                page = 1;
            else
            {
                searchString = currentFilter;
                startDate = startDateFilter;
                endDate = endDateFilter;
            }
            ViewBag.CurrentFilter = searchString;
            ViewBag.startDateFilter = startDate;
            ViewBag.endDateFilter = endDate;

            var data = from s in db.employeeLoans select s;
            //if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate) && !String.IsNullOrEmpty(searchString))
            //{
            //    DateTime sDate = DateTime.Parse(startDate);
            //    DateTime eDate = DateTime.Parse(endDate);
            //    data = data.Where(s => s.klaimDate >= sDate && s.klaimDate <= eDate && s.klaimID.Contains(searchString));
            //}
            //else 
            if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.proposedDate >= sDate && s.proposedDate <= eDate);
            }
            //else 
            //if (!String.IsNullOrEmpty(searchString))
            //{
            //    data = data.Where(s => s.employeeName.Contains(searchString) || s.employeeNIK.Contains(searchString));
            //}


            if (lvm.isAdmin == false && ViewBag.ApprovalAuth == false)
                data = data.Where(x => x.createdUser == lvm.userID);

            switch (sortOrder)
            {
                case "date_":
                    data = data.OrderByDescending(s => s.proposedDate);
                    break;
                //case "voucherNo":
                //    data = data.OrderBy(s => new { s.purchaseInvoiceNo });
                //    break;
                //case "voucherNo_":
                //    data = data.OrderByDescending(s => s.purchaseInvoiceNo);
                //    break;
                //case "isParent":
                //    data = data.OrderBy(s => new { s.menuIsParent, s.menuName });
                //    break;
                //case "isParent_":
                //    data = data.OrderByDescending(s => s.menuIsParent);
                //    break;
                //case "isActive":
                //    data = data.OrderBy(s => new { s.menuIsActive, s.menuName });
                //    break;
                //case "isActive_":
                //    data = data.OrderByDescending(s => s.menuIsActive);
                //    break;
                default:
                    data = data.OrderBy(s => s.createdDate);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(data.ToPagedList(pageNumber, pageSize));
            #endregion

            #region output Index
            //if (lvm.isAdmin == false && ViewBag.ApprovalAuth == false)
            //    return View(db.employeeLoans.Where(x => x.createdUser == lvm.userID).ToList());
            //else if (lvm.isAdmin == false && ViewBag.ApprovalAuth == true)
            //    return View(db.employeeLoans.ToList());
            //else
            //    return View(db.employeeLoans.ToList());
            #endregion
        }

        // GET: /employeeLoans/Details/5
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

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

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

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

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
                return RedirectToAction("Index");
            }
            return View(employeeLoans);
        }

        // GET: /employeeLoans/Edit/5
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

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

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
                return RedirectToAction("Index");
            }
            return View(employeeLoans);
        }

        // GET: /employeeLoans/Delete/5
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

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            employeeLoan employeeLoans = db.employeeLoans.Find(id);
            db.employeeLoans.Remove(employeeLoans);
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

        public void loadDefault(string pinjamanID)
        {
            ViewBag.employeeList = db.employees.AsNoTracking().ToList();

            string sql = string.Empty;
            #region oldProcess
            //sql = "SELECT ";
            //sql += "a.employeeID, a.employeeNIK, a.employeeName, ";
            //sql += "d.deptID, d.deptName ";
            //sql += "FROM [dbo].[Employees] a ";
            //sql += "INNER JOIN ";
            //sql += "(SELECT a.employeeID, b.positionId ";
            //sql += "FROM [dbo].[EmployeeResigns] a ";
            //sql += "INNER JOIN [dbo].[EmployeePositions] b ON (b.employeeID = a.employeeID AND b.positionDate > a.resignDate) ";
            //sql += "UNION ";
            //sql += "SELECT employeeID, ";
            //sql += "(SELECT positionId FROM [dbo].[EmployeePositions] WHERE employeeID = a.employeeID)positionId ";
            //sql += "FROM [dbo].[Employees] a ";
            //sql += "WHERE a.employeeID NOT IN (SELECT employeeID ";
            //sql += "							FROM [dbo].[EmployeeResigns]) ";
            //sql += ") b ON b.employeeID = a.employeeID ";
            //sql += "LEFT JOIN [dbo].[EmployeePositions] c ON c.employeeID = b.employeeID AND c.positionId = b.positionId ";
            //sql += "LEFT JOIN [dbo].[Departments] d ON d.deptID = c.deptID ";
            #endregion
            #region newProcess
            ////output:
            ////employeeID, employeeName, employeeNIK, email, npwp, ktp, address, city, bankName, 
            ////rekening, tempatLahir, tanggalLahir, linkFilePhoto, positionDate, positionId, resignDate, gapok
            sql += "SELECT * ";
            sql += "FROM [dbo].[v_employee] b ";
            sql += "LEFT JOIN [dbo].[EmployeePositions] c ON c.employeeID = b.employeeID AND c.positionId = b.positionId  ";
            sql += "LEFT JOIN [dbo].[Departments] d ON d.deptID = c.deptID ";
            sql += "WHERE b.resignDate IS NULL ";
            #endregion
            DataTable dtEmployee = cd.executeReader(sql);

            var model = new employeeLoan();
            foreach (DataRow dr in dtEmployee.Rows)
            {
                var editor = new employeeLoan.employeePOPUp()
                {
                    employeeID = int.Parse(dr["employeeID"].ToString()),
                    employeeNIK = dr["employeeNIK"].ToString(),
                    employeeName = dr["employeeName"].ToString(),
                    deptID = int.Parse(dr["deptID"].ToString()),
                    deptName = dr["deptName"].ToString(),
                    gapok = decimal.Parse(dr["gapok"].ToString())
                };
                model.popUpEmployee.Add(editor);
            }
            ViewBag.EmpPopUp = model.popUpEmployee.ToList();
            ViewBag.LoanCategory = cm.ddlLoanCategory(string.Empty);
            


            if(pinjamanID != string.Empty)
            {
                var id = int.Parse(pinjamanID);
                var dataSaved = db.employeeLoans.AsNoTracking().Where(x => x.pinjamanID == id)
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

        public ActionResult Approval(string id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;

            ViewBag.ApprovalAuth = false;
            foreach (string b in lvm.listAuthorize as List<string>)
            {
                string[] auth = b.Split('_');
                if (auth[0].ToString() == "A1")
                    if (b.Contains((Url.Action("")).Replace("/", "")))
                    {
                        ViewBag.ApprovalAuth = true;
                    }
            }
            if (ViewBag.ApprovalAuth == false && lvm.isAdmin == true)
                ViewBag.ApprovalAuth = true;

            if (ViewBag.ApprovalAuth == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int i_id = int.Parse(id);
            employeeLoan employeeloan = db.employeeLoans.Find(i_id);
            if (employeeloan == null)
            {
                return HttpNotFound();
            }

            loadDefault(i_id.ToString());
            return View(employeeloan);
        }
    }
}
