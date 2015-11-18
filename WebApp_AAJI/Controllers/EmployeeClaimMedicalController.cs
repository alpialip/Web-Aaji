using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;
using PagedList;
using System.Data.SqlClient;

namespace WebApp_AAJI.Controllers
{
    public class EmployeeClaimMedicalController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        private AccountController acm = new AccountController();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private ConnectionController cd = new ConnectionController();
        private CommonController ccm = new CommonController();
        public string uploadDir = "~/uploads";
        int maxRows = 10;

        [HttpGet]
        public async Task<ActionResult> LoadDetailFamilySearch(string employeeID)
        {
            #region newProcess
            getListFamily(employeeID);
            #endregion

            return PartialView("_PartialPagePopUpFamilySearchSub");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDetailClaimMedical(int employeeID, int claimID)
        {
            ViewBag.typeClaim = ccm.typeClaimEmployee(string.Empty);
            string sql = "SELECT B.klaimID, seqID, claimType, relationStatus, happeningDate, relationSeqID, B.amount, B.remarks ";
                  sql += "FROM [dbo].[EmployeeClaimMedicals] A WITH (NOLOCK) ";
                  sql += "INNER JOIN [dbo].[EmployeeClaimMedicalDetails] B WITH (NOLOCK) ON B.klaimID = A.klaimID ";
                  sql += "WHERE A.klaimID = " + claimID + " AND A.employeeID = " + employeeID + "";

            DataTable dtFamilyList = cd.executeReader(sql);

            var model = new employeeClaimMedical();
            foreach (DataRow dr in dtFamilyList.Rows)
            {
                var editor = new employeeClaimMedical.employeeClaimMedicalDetail()
                {
                    amount = decimal.Parse(dr["amount"].ToString()),
                    claimType = dr["claimType"].ToString(),
                    happeningDate = DateTime.Parse(dr["happeningDate"].ToString()),
                    klaimID = int.Parse(dr[""].ToString()),
                    relationSeqID = int.Parse(dr["relationSeqID"].ToString()),
                    relationStatus = dr["relationStatus"].ToString(),
                    remarks = dr["remarks"].ToString(),
                    seqID = int.Parse(dr["seqID"].ToString())
                };
                model.claimMedicalDetail.Add(editor);
            }
            ViewBag.detailClaimMedical = model.claimMedicalDetail.ToList();

            getListFamily(employeeID.ToString());

            return PartialView("_PartialPageClaimDetail");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDetailClaimMedical_Client(string dataDetail, string act, int employeeID)
        {
            string[] data = dataDetail.Split('|');

            var model = new employeeClaimMedical();
            for (int i = 0; i < data.Count(); i++)
            {
                string[] value = data[i].Split(';');

                if (value[1].ToString() == "")
                    break;

                int txtSeqID_Val = int.Parse(value[1].ToString());
                string txtFamilyName_Val = value[2].ToString();
                DateTime klaimDateDetail_Val = DateTime.Parse(value[3].ToString());
                string typeClaim_Val = value[4].ToString();
                decimal amountClaims_Val = decimal.Parse(value[5].ToString());
                string descriptionClaim_Val = value[6].ToString();
                string relationStatus_Val = value[7].ToString();
                string jenisKelamin_Val = value[8].ToString();

                var editor = new employeeClaimMedical.employeeClaimMedicalDetail()
                {
                    amount = amountClaims_Val,
                    claimType = typeClaim_Val,
                    happeningDate = klaimDateDetail_Val,
                    klaimID = 0,
                    relationSeqID = txtSeqID_Val,
                    relationStatus = relationStatus_Val,
                    remarks = descriptionClaim_Val,
                    seqID = txtSeqID_Val
                };
                model.claimMedicalDetail.Add(editor);
            }
            ViewBag.detailClaimMedical = model.claimMedicalDetail.ToList();

            ViewBag.typeClaim = ccm.typeClaimEmployee(string.Empty);
            getListFamily(employeeID.ToString());
            return PartialView("_PartialPageClaimDetail");
        }
        
        [HttpGet]
        public async Task<ActionResult> ApprovalProcess(string act, string id)
        {
            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            int klaimID = int.Parse(id);
            employeeClaimMedical model = db.employeeClaimMedicals.Find(klaimID);
            model.approvedStatus = act == "approved" ? true : false;
            model.approvedDate = DateTime.Now;
            model.approvedBy = lvm.userID;
            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: /EmployeeClaimMedical/
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

            var data = from s in db.employeeClaimMedicals select s;
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
                data = data.Where(s => s.klaimDate >= sDate && s.klaimDate <= eDate);
            }
            //else if (!String.IsNullOrEmpty(searchString))
            //{
            //    data = data.Where(s => s.klaimID.Contains(searchString));
            //}


            if (lvm.isAdmin == false && ViewBag.ApprovalAuth == false)
                data = data.Where(x => x.createdUser == lvm.userID);

            switch (sortOrder)
            {
                case "date_":
                    data = data.OrderByDescending(s => s.klaimDate);
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
                    data = data.OrderByDescending(s => s.createdDate);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(data.ToPagedList(pageNumber, pageSize));
            #endregion

            #region outout Index
            //if (lvm.isAdmin == false && ViewBag.ApprovalAuth == false)
            //    return View(db.employeeClaimMedicals.Where(x => x.createdUser == lvm.userID).ToList());
            //else if (lvm.isAdmin == false && ViewBag.ApprovalAuth == true)
            //    return View(db.employeeClaimMedicals.ToList());
            //else
            //    return View(db.employeeClaimMedicals.ToList());
            #endregion
        }

        // GET: /EmployeeClaimMedical/Details/5
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
            employeeClaimMedical employeeclaimmedical = db.employeeClaimMedicals.Find(id);
            if (employeeclaimmedical == null)
            {
                return HttpNotFound();
            }

            loadDefault(employeeclaimmedical.klaimID.ToString());
            return View(employeeclaimmedical);
        }

        // GET: /EmployeeClaimMedical/Create
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

        // POST: /EmployeeClaimMedical/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(employeeClaimMedical employeeclaimmedical)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            var countChk = 0;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("txtSeqID_"))
                    countChk++;
            }

            var model = new employeeClaimMedical();
            for (int i = 0; i < countChk; i++)
            {
                int txtSeqID_Val = int.Parse(Request.Form["txtSeqID_"+(i+1)].ToString());
                string txtFamilyName_Val = Request.Form["txtFamilyName_" + (i + 1)].ToString();
                DateTime klaimDateDetail_Val = DateTime.Parse(Request.Form["klaimDateDetail_" + (i + 1)].ToString());
                string typeClaim_Val = Request.Form["typeClaim_" + (i + 1)].ToString();
                decimal amountClaims_Val = decimal.Parse(Request.Form["amountClaims_" + (i + 1)].ToString());
                string descriptionClaim_Val = Request.Form["descriptionClaim_" + (i + 1)].ToString();
                string relationStatus_Val = Request.Form["relationStatus_" + (i + 1)].ToString();
                string jenisKelamin_Val = Request.Form["jenisKelamin_" + (i + 1)].ToString();

                var editor = new employeeClaimMedical.employeeClaimMedicalDetail()
                {
                    amount = amountClaims_Val,
                    claimType = typeClaim_Val,
                    happeningDate = klaimDateDetail_Val,
                    klaimID = 0,
                    relationSeqID = txtSeqID_Val,
                    relationStatus = relationStatus_Val,
                    remarks = descriptionClaim_Val,
                    seqID = txtSeqID_Val
                };
                model.claimMedicalDetail.Add(editor);
            }
            ViewBag.detailClaimMedical = model.claimMedicalDetail.ToList();

            ViewBag.typeClaim = ccm.typeClaimEmployee(string.Empty);
            getListFamily(employeeclaimmedical.employeeID.ToString());
            
            if (ModelState.IsValid)
            {
                bool isProposed = false;
                for (int i = 0; i < Request.Form.Count; i++)
                {
                    if (Request.Form.AllKeys.ToList()[i].Contains("hdnTypeSubmit"))
                        if (Request.Form["hdnTypeSubmit"].ToString() == "requested")
                            isProposed = true;
                }

                var file = Request.Files["linkFileData"];
                if (file != null && file.ContentLength > 0)
                {
                    var imagePath = Path.Combine(Server.MapPath(uploadDir), file.FileName);
                    var imageUrl = Path.Combine(uploadDir, file.FileName);
                    file.SaveAs(imagePath);
                    employeeclaimmedical.linkFileData = imageUrl;
                }

                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                employeeclaimmedical.createdUser = lvm.userID;
                employeeclaimmedical.createdDate = DateTime.Now;
                employeeclaimmedical.proposedStatus = isProposed;
                employeeclaimmedical.proposedDate = DateTime.Now;
                employeeclaimmedical.proposedBy = lvm.userID;
                db.employeeClaimMedicals.Add(employeeclaimmedical);
                db.SaveChanges();

                if (employeeclaimmedical.klaimID > 0)
                { 
                    #region insertDetail
                    for (int i = 0; i < countChk; i++)
                    {
                        int txtSeqID_Val = int.Parse(Request.Form["txtSeqID_" + (i + 1)].ToString());
                        string txtFamilyName_Val = Request.Form["txtFamilyName_" + (i + 1)].ToString();
                        DateTime klaimDateDetail_Val = DateTime.Parse(Request.Form["klaimDateDetail_" + (i + 1)].ToString());
                        string typeClaim_Val = Request.Form["typeClaim_" + (i + 1)].ToString();
                        decimal amountClaims_Val = decimal.Parse(Request.Form["amountClaims_" + (i + 1)].ToString());
                        string descriptionClaim_Val = Request.Form["descriptionClaim_" + (i + 1)].ToString();
                        string relationStatus_Val = Request.Form["relationStatus_" + (i + 1)].ToString();
                        string jenisKelamin_Val = Request.Form["jenisKelamin_" + (i + 1)].ToString();

                        var editor = new employeeClaimMedical.employeeClaimMedicalDetail()
                        {
                            amount = amountClaims_Val,
                            claimType = typeClaim_Val,
                            happeningDate = klaimDateDetail_Val,
                            klaimID = 0,
                            relationSeqID = txtSeqID_Val,
                            relationStatus = relationStatus_Val,
                            remarks = descriptionClaim_Val,
                            seqID = txtSeqID_Val
                        };
                        db.employeeClaimMedicalDetails.Add(editor);
                    }
                    #endregion

                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            return View(employeeclaimmedical);
        }

        // GET: /EmployeeClaimMedical/Edit/5
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
            employeeClaimMedical employeeclaimmedical = db.employeeClaimMedicals.Find(id);
            if (employeeclaimmedical == null)
            {
                return HttpNotFound();
            }

            loadDefault(employeeclaimmedical.klaimID.ToString());
            return View(employeeclaimmedical);
        }

        // POST: /EmployeeClaimMedical/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(employeeClaimMedical employeeclaimmedical)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            if (ModelState.IsValid)
            {
                bool isProposed = false;
                for (int i = 0; i < Request.Form.Count; i++)
                {
                    if (Request.Form.AllKeys.ToList()[i].Contains("hdnTypeSubmit"))
                        if (Request.Form["hdnTypeSubmit"].ToString() == "requested")
                            isProposed = true;
                }

                var file = Request.Files["linkFileData"];
                if (file != null && file.ContentLength > 0)
                {
                    var imagePath = Path.Combine(Server.MapPath(uploadDir), file.FileName);
                    var imageUrl = Path.Combine(uploadDir, file.FileName);
                    file.SaveAs(imagePath);
                    employeeclaimmedical.linkFileData = imageUrl;
                }
                else
                {
                    var xfile = db.employeeClaimMedicals.AsNoTracking().Where(x => x.klaimID == employeeclaimmedical.klaimID).ToList();
                    employeeclaimmedical.linkFileData = xfile[0].linkFileData;
                }

                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                employeeclaimmedical.modifiedUser = lvm.userID;
                employeeclaimmedical.modifiedDate = DateTime.Now;
                employeeclaimmedical.proposedStatus = isProposed;
                employeeclaimmedical.proposedDate = DateTime.Now;
                employeeclaimmedical.proposedBy = lvm.userID;
                db.Entry(employeeclaimmedical).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(employeeclaimmedical);
        }

        // GET: /EmployeeClaimMedical/Delete/5
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
            employeeClaimMedical employeeclaimmedical = db.employeeClaimMedicals.Find(id);
            if (employeeclaimmedical == null)
            {
                return HttpNotFound();
            }

            loadDefault(employeeclaimmedical.klaimID.ToString());
            return View(employeeclaimmedical);
        }

        // POST: /EmployeeClaimMedical/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            employeeClaimMedical employeeclaimmedical = db.employeeClaimMedicals.Find(id);
            db.employeeClaimMedicals.Remove(employeeclaimmedical);
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

        public void loadDefault(string claimId)
        {
            Session["employeeID"] = "";
            ViewBag.typeClaim = ccm.typeClaimEmployee(string.Empty);
            ViewBag.employeeList = db.employees.AsNoTracking().ToList();

            string sql = string.Empty;
            #region oldProcess
            //  sql = "SELECT ";
            //    sql += "a.employeeID, a.employeeNIK, a.employeeName, ";
            //    sql += "d.deptID, d.deptName ";
            //    sql += "FROM [dbo].[Employees] a ";
            //    sql += "INNER JOIN ";
            //    sql += "(SELECT a.employeeID, b.positionId ";
            //    sql += "FROM [dbo].[EmployeeResigns] a ";
            //    sql += "INNER JOIN [dbo].[EmployeePositions] b ON (b.employeeID = a.employeeID AND b.positionDate > a.resignDate) ";
            //    sql += "UNION ";
            //    sql += "SELECT employeeID, ";
            //    sql += "(SELECT positionId FROM [dbo].[EmployeePositions] WHERE employeeID = a.employeeID)positionId ";
            //    sql += "FROM [dbo].[Employees] a ";
            //    sql += "WHERE a.employeeID NOT IN (SELECT employeeID ";
            //    sql += "							FROM [dbo].[EmployeeResigns]) ";
            //    sql += ") b ON b.employeeID = a.employeeID ";
            //    sql += "LEFT JOIN [dbo].[EmployeePositions] c ON c.employeeID = b.employeeID AND c.positionId = b.positionId ";
            //    sql += "LEFT JOIN [dbo].[Departments] d ON d.deptID = c.deptID ";
            #endregion
            #region newProcess
                sql += "SELECT * ";
                sql += "FROM [dbo].[v_employee] b ";
                sql += "LEFT JOIN [dbo].[EmployeePositions] c ON c.employeeID = b.employeeID AND c.positionId = b.positionId  ";
                sql += "LEFT JOIN [dbo].[Departments] d ON d.deptID = c.deptID ";
                sql += "WHERE b.resignDate IS NULL ";
            #endregion
            DataTable dtEmployee = cd.executeReader(sql);

            var model = new employeeLoan();
            foreach(DataRow dr in dtEmployee.Rows)
            {
                var editor = new employeeLoan.employeePOPUp()
                {
                    employeeID = int.Parse(dr["employeeID"].ToString()),
                    employeeNIK = dr["employeeNIK"].ToString(),
                    employeeName = dr["employeeName"].ToString(),
                    deptID = int.Parse(dr["deptID"].ToString()),
                    deptName = dr["deptName"].ToString()
                };
                model.popUpEmployee.Add(editor);
            }
            ViewBag.EmpPopUp = model.popUpEmployee.ToList();


            if (claimId != string.Empty)
            {
                var id = int.Parse(claimId);
                var dataSaved = db.employeeClaimMedicals.AsNoTracking().Where(x => x.klaimID == id)
                            .Join(db.employees, q => q.employeeID, r => r.employeeID, (q, r) => new { q, r })
                            .Join(db.employeePositions, s => s.q.employeeID, t => t.employeeID, (s, t) => new { s, t })
                            .Join(db.departments, u => u.t.deptID, v => v.deptID, (u, v) => new { u, v })
                            .Select(c => new { c.u.s.r.employeeName, c.u.s.r.employeeNIK, c.u.t.deptID, c.v.deptName, c.u.s.q.proposedBy }).ToList();

                ViewBag.empName = dataSaved[0].employeeName;
                ViewBag.empDeptName = dataSaved[0].deptName;

                //lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                //var a = db.departments.Where(x => x.deptID == lvm.deptID).Select(x => new { x.deptName }).ToList();
                //ViewBag.Dept = a[0].deptName;

                var userID = dataSaved[0].proposedBy;
                var b = db.Users.Where(x => x.userID == userID).Select(x => new { x.userName }).ToList();
                ViewBag.UserName = b[0].userName;
                ViewBag.UserID = userID;
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
            int klaimID = int.Parse(id);
            employeeClaimMedical employeeclaimmedical = db.employeeClaimMedicals.Find(klaimID);
            if (employeeclaimmedical == null)
            {
                return HttpNotFound();
            }

            loadDefault(employeeclaimmedical.klaimID.ToString());
            return View(employeeclaimmedical);
        }


        public void getListFamily(string employeeID)
        {
            string sql = "SELECT [employeeID],[sId],[name],[birthDate],[birthPlace],[education],[statusRelation],[gender] ";
                    sql += "FROM [dbo].[v_family] ";
                    sql += "WHERE [employeeID] = " + employeeID + " ORDER BY number ASC";

            DataTable dtFamilyList = cd.executeReader(sql);

            var model = new employeeClaimMedical();
            foreach (DataRow dr in dtFamilyList.Rows)
            {
                var editor = new employeeClaimMedical.familyList()
                {
                    employeeID = int.Parse(dr["employeeID"].ToString()),
                    sId = int.Parse(dr["sId"].ToString()),
                    name = dr["name"].ToString(),
                    gender = dr["gender"].ToString(),
                    birthdate = DateTime.Parse(dr["birthdate"].ToString()),
                    birthplace = dr["birthplace"].ToString(),
                    education = dr["education"].ToString(),
                    statusRelation = dr["statusRelation"].ToString(),
                };
                model.listFamily.Add(editor);
            }
            ViewBag.FamilyPopUp = model.listFamily.ToList();
        }
    }
}
