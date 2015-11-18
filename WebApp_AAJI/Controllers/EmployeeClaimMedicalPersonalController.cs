using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class EmployeeClaimMedicalPersonalController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        private AccountController acm = new AccountController();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private ConnectionController cd = new ConnectionController();

        // GET: /EmployeeClaimMedical/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            loadDefault(string.Empty);
            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            return View(db.employeeClaimMedicals.Where(x=>x.employeeID==lvm.employeeId).ToList());
        }

        // GET: /EmployeeClaimMedical/Details/5
        public ActionResult Details(int? id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");
            
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
                    var uploadDir = "~/uploads";
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
                return RedirectToAction("Index","EmployeeClaimMedicalPersonal");
            }

            return View(employeeclaimmedical);
        }

        // GET: /EmployeeClaimMedical/Edit/5
        public ActionResult Edit(int? id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");
            
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
                    var uploadDir = "~/uploads";
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
                return RedirectToAction("Index", "EmployeeClaimMedicalPersonal");
            }
            return View(employeeclaimmedical);
        }

        // GET: /EmployeeClaimMedical/Delete/5
        public ActionResult Delete(int? id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");
            
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
            
            employeeClaimMedical employeeclaimmedical = db.employeeClaimMedicals.Find(id);
            db.employeeClaimMedicals.Remove(employeeclaimmedical);
            db.SaveChanges();
            return RedirectToAction("Index", "EmployeeClaimMedicalPersonal");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public FileResult Download(string file)
        {
            string[] filename = file.Split('\\');
            string filePath = Server.MapPath(file).ToString();
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            var response = new FileContentResult(fileBytes, "application/octet-stream");
            response.FileDownloadName = filename[1].ToString();
            return response;
        }

        public void loadDefault(string claimId)
        {
            ViewBag.employeeList = db.employees.AsNoTracking().ToList();

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            ViewBag.empName = lvm.employeeName;
            ViewBag.empId = lvm.employeeId;

            #region old proses ~salah divisiID dengan deptID
            //var dept = db.departments.Where(x=>x.deptID == lvm.deptID).ToList();
            //ViewBag.empDeptName = lvm.isAdmin == true ? "" : dept[0].deptName;

            ////Perbaikannya
            var dept = db.departments.Where(x => x.deptID == lvm.divID).ToList();
            ViewBag.empDeptName = lvm.isAdmin == true ? "" : dept[0].deptName;
            #endregion

            if (claimId != string.Empty)
            {
                var id = int.Parse(claimId);
                var dataSaved = db.employeeClaimMedicals.AsNoTracking().Where(x => x.klaimID == id && x.employeeID == lvm.employeeId)
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
    }
}