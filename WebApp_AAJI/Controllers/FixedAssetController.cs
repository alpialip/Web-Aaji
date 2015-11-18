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
using PagedList;

namespace WebApp_AAJI.Controllers
{
    public class FixedAssetController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private CommonController ccm = new CommonController();
        string prefix = "GA";

        // GET: /FixedAsset/
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, string startDateFilter, string startDate, string endDateFilter, string endDate, int? page)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

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

            var data = from s in db.fixedAssets select s;
            if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate) && !String.IsNullOrEmpty(searchString))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.purchaseDate >= sDate && s.purchaseDate <= eDate && (s.fixedAssetName.Contains(searchString) || s.fixedAssetCode.Contains(searchString)));
            }
            else if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.purchaseDate >= sDate && s.purchaseDate <= eDate);
            }
            else if (!String.IsNullOrEmpty(searchString))
            {
                data = data.Where(s => s.fixedAssetName.Contains(searchString) || s.fixedAssetCode.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "date_":
                    data = data.OrderByDescending(s => s.purchaseDate);
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

            //return View(db.fixedAssets.ToList());
        }

        // GET: /FixedAsset/Details/5
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
            fixedAsset fixedasset = db.fixedAssets.Find(id);
            if (fixedasset == null)
            {
                return HttpNotFound();
            }

            loadDefaultData(fixedasset.fixedAssetID.ToString());
            return View(fixedasset);
        }

        // GET: /FixedAsset/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            loadDefaultData(string.Empty);
	     return View(new fixedAsset
	     {
		     purchaseDate = DateTime.Now
	     });
        }

        // POST: /FixedAsset/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(fixedAsset fixedasset)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            loadDefaultData(string.Empty);

            #region collect code n detail
            string userID = string.Empty;
            string typeCreated = string.Empty;
            int countDetail = 0;
            string rbTypeCode = string.Empty;
            DateTime maintDate = DateTime.Now;
            string maintRemark = string.Empty;
            int maintAmount = 0;
            string codeManual = string.Empty;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("rbTypeCode"))
                {
                    rbTypeCode = Request.Form["rbTypeCode"].ToString();
                    ViewBag.typeCode = Request.Form["rbTypeCode"].ToString();
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("txtCode"))
                {
                    if(rbTypeCode == "Manual")
                    { 
                        fixedasset.fixedAssetCode = Request.Form["txtCode"].ToString();
                        ViewBag.codeManual = Request.Form["txtCode"].ToString();
                    }
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("persStart_"))
                {
                    countDetail++;
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("maintDate_"))
                {
                    maintDate = Convert.ToDateTime(Request.Form["maintDate_0"].ToString());
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("maintRemark_"))
                {
                    maintRemark = Request.Form["maintRemark_0"].ToString();
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("maintAmount_"))
                {
                    maintAmount = int.Parse(Request.Form["maintAmount_0"].ToString());
                }
            }
            #endregion

            if (ModelState.IsValid)
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        if (rbTypeCode == "Generated")
                        {
                            string generateID = prefix + DateTime.Now.Year.ToString().Substring(2) + DateTime.Now.Month.ToString("d2");
                            var codeGenerated = db.fixedAssets.Where(x => x.fixedAssetCode.Contains(generateID)).OrderByDescending(x => x.createdDate).Select(x => x.fixedAssetCode).ToList();
                            if (codeGenerated.Count == 0)
                            {
                                generateID = generateID + "0001";
                            }
                            else
                            {
                                generateID = generateID + (Convert.ToInt32(codeGenerated[0].Substring((codeGenerated[0].Length - 4))) + 1).ToString().PadLeft(4, '0');
                            }
                            fixedasset.fixedAssetCode = generateID;
                        }

                        lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                        fixedasset.createdUser = lvm.userID;
                        fixedasset.createdDate = DateTime.Now;
                        db.fixedAssets.Add(fixedasset);
                        db.SaveChanges();

                        if(fixedasset.fixedAssetID > 0)
                        {
                            #region insertDetailPerson
                            for (int i = 0; i < countDetail; i++)
                            {
                                var colVal = Request.Form["persStart_" + i];
                                if (colVal == "")
                                    break;

                                DateTime t_persStart = Convert.ToDateTime(Request.Form["persStart_" + i].ToString());
                                DateTime t_persEnd = Convert.ToDateTime(Request.Form["persEnd_" + i].ToString());
                                int t_persEmpId = int.Parse(Request.Form["persEmpId_" + i].ToString());
                                string t_persEmpName = Request.Form["persEmpName_" + i].ToString();
                                string t_persRemark = Request.Form["persRemark_" + i].ToString();

                                var editorx = new fixedAsset.fixedAssetPerson()
                                {
                                    fixedAssetID = fixedasset.fixedAssetID,
                                    startDate = t_persStart,
                                    endDate = t_persEnd,
                                    employeeID = t_persEmpId,
                                    remarks = t_persRemark
                                };
                                db.fixedAssetPersons.Add(editorx);
                            }
                            #endregion

                            fixedAsset.fixedAssetMaintenance assetMaint = new fixedAsset.fixedAssetMaintenance();
                            assetMaint.fixedAssetID = fixedasset.fixedAssetID;
                            assetMaint.maintenanceDate = maintDate;
                            assetMaint.remarks = maintRemark;
                            assetMaint.amount = maintAmount;
                            db.fixedAssetMaintenances.Add(assetMaint);
                        }

                        db.SaveChanges();
                        ts.Complete();
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception exc)
                {
                    string a = exc.Message;
                }
            }

            return View(fixedasset);
        }

        // GET: /FixedAsset/Edit/5
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
            fixedAsset fixedasset = db.fixedAssets.Find(id);
            if (fixedasset == null)
            {
                return HttpNotFound();
            }

            decimal depreciationValue = ccm.depreciationAsset(fixedasset.depreciationPeriod, fixedasset.depreciationProcentage, fixedasset.depreciationValPeriod, fixedasset.purchaseDate, fixedasset.amount);
            loadDefaultData(fixedasset.fixedAssetID.ToString());
            return View(fixedasset);
        }

        // POST: /FixedAsset/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( fixedAsset fixedasset)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            loadDefaultData(string.Empty);

            #region collect code n detail
            string userID = string.Empty;
            string typeCreated = string.Empty;
            int countDetail = 0;
            string rbTypeCode = string.Empty;
            DateTime maintDate = DateTime.Now;
            string maintRemark = string.Empty;
            int maintAmount = 0;
            string codeManual = string.Empty;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("rbTypeCode"))
                {
                    rbTypeCode = Request.Form["rbTypeCode"].ToString();
                    ViewBag.typeCode = Request.Form["rbTypeCode"].ToString();
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("txtCode"))
                {
                    if (rbTypeCode == "Manual")
                    {
                        fixedasset.fixedAssetCode = Request.Form["txtCode"].ToString();
                        ViewBag.codeManual = Request.Form["txtCode"].ToString();
                    }
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("persStart_"))
                {
                    countDetail++;
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("maintDate_"))
                {
                    maintDate = Convert.ToDateTime(Request.Form["maintDate_0"].ToString());
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("maintRemark_"))
                {
                    maintRemark = Request.Form["maintRemark_0"].ToString();
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("maintAmount_"))
                {
                    maintAmount = int.Parse(Request.Form["maintAmount_0"].ToString());
                }
            }
            #endregion

            if (ModelState.IsValid)
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                        fixedasset.modifiedUser = lvm.userID;
                        fixedasset.modifiedDate = DateTime.Now;
                        db.Entry(fixedasset).State = EntityState.Modified;

                        db.fixedAssetPersons.RemoveRange(db.fixedAssetPersons.Where(x => x.fixedAssetID == fixedasset.fixedAssetID));
                        db.fixedAssetMaintenances.RemoveRange(db.fixedAssetMaintenances.Where(x => x.fixedAssetID == fixedasset.fixedAssetID));

                            #region insertDetailPerson
                            for (int i = 0; i < countDetail; i++)
                            {
                                var colVal = Request.Form["persStart_" + i];
                                if (colVal == "")
                                    break;

                                DateTime t_persStart = Convert.ToDateTime(Request.Form["persStart_" + i].ToString());
                                DateTime t_persEnd = Convert.ToDateTime(Request.Form["persEnd_" + i].ToString());
                                int t_persEmpId = int.Parse(Request.Form["persEmpId_" + i].ToString());
                                string t_persEmpName = Request.Form["persEmpName_" + i].ToString();
                                string t_persRemark = Request.Form["persRemark_" + i].ToString();

                                var editorx = new fixedAsset.fixedAssetPerson()
                                {
                                    fixedAssetID = fixedasset.fixedAssetID,
                                    startDate = t_persStart,
                                    endDate = t_persEnd,
                                    employeeID = t_persEmpId,
                                    remarks = t_persRemark
                                };
                                db.fixedAssetPersons.Add(editorx);
                            }
                            #endregion

                            fixedAsset.fixedAssetMaintenance assetMaint = new fixedAsset.fixedAssetMaintenance();
                            assetMaint.fixedAssetID = fixedasset.fixedAssetID;
                            assetMaint.maintenanceDate = maintDate;
                            assetMaint.remarks = maintRemark;
                            assetMaint.amount = maintAmount;
                            db.fixedAssetMaintenances.Add(assetMaint);

                        db.SaveChanges();
                        ts.Complete();
                    }

                    return RedirectToAction("Index");
                }
                catch (Exception exc)
                {
                    string a = exc.Message;
                }
            }
            return View(fixedasset);
        }

        // GET: /FixedAsset/Delete/5
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
            fixedAsset fixedasset = db.fixedAssets.Find(id);
            if (fixedasset == null)
            {
                return HttpNotFound();
            }

            loadDefaultData(fixedasset.fixedAssetID.ToString());
            return View(fixedasset);
        }

        // POST: /FixedAsset/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            fixedAsset fixedasset = db.fixedAssets.Find(id);
            db.fixedAssets.Remove(fixedasset);
            db.fixedAssetPersons.RemoveRange(db.fixedAssetPersons.Where(x => x.fixedAssetID == fixedasset.fixedAssetID));
            db.fixedAssetMaintenances.RemoveRange(db.fixedAssetMaintenances.Where(x => x.fixedAssetID == fixedasset.fixedAssetID));
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

        public void loadDefaultData(string fixAssetId)
        {
            ViewBag.typeCode = "Generated";
            ViewBag.codeManual = "";
            ViewBag.employeeList = db.employees.AsNoTracking().ToList();

            var popUp = db.employees.AsNoTracking()
                        .Join(db.employeePositions, x => x.employeeID, y => y.employeeID, (x, y) => new { x, y })
                        .Join(db.departments, q => q.y.deptID, r => r.deptID, (q, r) => new { q, r })
                        .Select(c => new { c.q.x.employeeID, c.q.x.employeeNIK, c.q.x.employeeName, c.q.y.deptID, c.r.deptName }).ToList();

            var model = new employeeLoan();
            for (int i = 0; i < popUp.Count; i++)
            {
                var editor = new employeeLoan.employeePOPUp()
                {
                    employeeID = popUp[i].employeeID,
                    employeeNIK = popUp[i].employeeNIK,
                    employeeName = popUp[i].employeeName,
                    deptID = popUp[i].deptID,
                    deptName = popUp[i].deptName
                };
                model.popUpEmployee.Add(editor);
            }
            ViewBag.EmpPopUp = model.popUpEmployee.ToList();
            ViewBag.StatusfixedAsset = ccm.ddlStatusFixedAsset(string.Empty);
            ViewBag.ddlPeriodDepreciation = ccm.ddlPeriodDepreciation(string.Empty);


            if (fixAssetId != string.Empty)
            {
                var id = int.Parse(fixAssetId);
                var dataDetailSaved = db.fixedAssetPersons.AsNoTracking().Where(x => x.fixedAssetID == id)
                                        .Join(db.employees, c => c.employeeID, d => d.employeeID, (c, d) => new { c, d })
                                        .Join(db.employeePositions, f => f.d.employeeID, g => g.employeeID, (f, g) => new { f, g })
                                        .Join(db.departments, e => e.g.deptID, h => h.deptID, (e, h) => new { e, h })
                                        .Select(z => new { z.e.f.c.startDate, z.e.f.c.endDate, z.e.f.c.employeeID, z.e.f.d.employeeName, z.h.deptName, z.e.f.c.remarks }).ToList();
                var headerFix = new fixedAsset();
                for (int i = 0; i < dataDetailSaved.Count; i++ )
                {
                    var editor = new fixedAsset.detailSavedFixedAssetPerson()
                    {
                        startDate = dataDetailSaved[i].startDate,
                        endDate = dataDetailSaved[i].endDate,
                        employeeID = dataDetailSaved[i].employeeID,
                        employeeName = dataDetailSaved[i].employeeName,
                        deptName = dataDetailSaved[i].deptName,
                        remarks = dataDetailSaved[i].remarks
                    };
                    headerFix.fixAssetPersonDetail.Add(editor);
                }
                ViewBag.dataDetail = headerFix.fixAssetPersonDetail.ToList();

                ViewBag.dataDetailMaint = db.fixedAssetMaintenances.AsNoTracking().Where(x => x.fixedAssetID == id).ToList();

                var headerSaved = db.fixedAssets.AsNoTracking().Where(x => x.fixedAssetID == id).ToList();
                var userID = headerSaved[0].createdUser;
                var s = db.Users.Where(x => x.userID == userID).Select(x => new { x.userName }).ToList();
                if (s.Count > 0)
                    ViewBag.UserName = s[0].userName;
                else
                {
                    var modifuserID = headerSaved[0].modifiedUser;
                    var mod = db.Users.Where(x => x.userID == modifuserID).Select(x => new { x.userName }).ToList();
                    if (mod.Count > 0)
                        ViewBag.UserName = mod[0].userName;
                    else
                        ViewBag.UserName = "";
                }
                ViewBag.UserID = userID;
                ViewBag.StatusfixedAsset = ccm.ddlStatusFixedAsset(headerSaved[0].fixedAssetStatus);
                ViewBag.ddlPeriodDepreciation = ccm.ddlPeriodDepreciation(headerSaved[0].depreciationPeriod);
            }
        }
    }
}
