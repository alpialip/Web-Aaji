using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;
using PagedList;

namespace WebApp_AAJI.Controllers
{
    public class AdvancePaymentVoucherController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private CommonController ccm = new CommonController();
        string prefix = "APV";

        [HttpGet]
        public async Task<ActionResult> LoadDetailAdvancePaymentVoucher(string dataDetail, string act, string voucherNo)
        {
            string[] data = dataDetail.Split('|');

            var model = new advancePaymentVoucher();
            for (int i = 0; i < data.Count(); i++)
            {
                string[] value = data[i].Split(';');

                if (value[1].ToString() == "")
                    break;

                string idxRows = value[0].ToString();
                string t_accountNo = value[1].ToString();
                //string t_accountName = value[2].ToString();
                decimal t_amount = value[2].ToString() != "" ? Convert.ToDecimal(value[2].ToString()) : 0;

                var editor = new advancePaymentVoucher.advancePaymentVoucherDetail()
                {
                    voucherNo = voucherNo,
                    accountNo = t_accountNo,
                    amount = t_amount
                };
                model.detailAdvancePaymentVoucher.Add(editor);
            }
            ViewData["dataDetail"] = model.detailAdvancePaymentVoucher.ToList();
            ViewBag.COA = ccm.ddlAccountCOA(string.Empty);

            return PartialView("_PartialPageAPV1");
        }

        [HttpGet]
        public async Task<ActionResult> ApprovalProcess(string act, string id)
        {
            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            advancePaymentVoucher apv = db.advancePaymentVouchers.Find(id);
            apv.approvedStatus = act == "approved" ? true : false;
            apv.approvedDate = DateTime.Now;
            apv.approvedBy = lvm.userID; 
            db.Entry(apv).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: /AdvancePaymentVoucher/
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, string startDateFilter, string startDate, string endDateFilter, string endDate, int? page)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.ApprovalAuth = false;
            foreach(string b in lvm.listAuthorize as List<string>)
            {
                string[] auth = b.Split('_');
                if (auth[0].ToString() == "A1")
                    if(b.Contains((Url.Action("")).Replace("/","")))
                    {
                        ViewBag.ApprovalAuth = true;
                    }
            }
            if (ViewBag.ApprovalAuth == false && lvm.isAdmin == true)
                ViewBag.ApprovalAuth = true;

            ViewBag.Bank = db.banks.ToList();
            ViewBag.User= db.Users.ToList();

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

            var data = from s in db.advancePaymentVouchers select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                data = data.Where(s => s.voucherNo.Contains(searchString));
            }
            else if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.voucherDate >= sDate && s.voucherDate <= eDate);
            }
            else if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate) && !String.IsNullOrEmpty(searchString))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.voucherDate >= sDate && s.voucherDate <= eDate && s.voucherNo.Contains(searchString));
            }

            if (lvm.isAdmin == false && ViewBag.ApprovalAuth == false)
                data = data.Where(x => x.createdUser == lvm.userID);

            switch (sortOrder)
            {
                case "date_":
                    data = data.OrderByDescending(s => s.voucherDate);
                    break;
                case "voucherNo":
                    data = data.OrderBy(s => new { s.voucherNo });
                    break;
                case "voucherNo_":
                    data = data.OrderByDescending(s => s.voucherNo);
                    break;
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
            //    return View(db.advancePaymentVouchers.Where(x => x.createdUser == lvm.userID).ToList());
            //else if (lvm.isAdmin == false && ViewBag.ApprovalAuth == true)
            //    return View(db.advancePaymentVouchers.ToList());
            //else
            //    return View(db.advancePaymentVouchers.ToList());
            #endregion
        }

        // GET: /AdvancePaymentVoucher/Details/5
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
            advancePaymentVoucher advancepaymentvoucher = db.advancePaymentVouchers.Find(id);
            if (advancepaymentvoucher == null)
            {
                return HttpNotFound();
            }

            loadDetail(id);
            ViewBag.Bank = ccm.ddlBank(advancepaymentvoucher.bankID.ToString());
            ViewBag.COA = ccm.ddlAccountCOA(string.Empty);
            ViewBag.User = db.Users.ToList();
            return View(advancepaymentvoucher);
        }

        // GET: /AdvancePaymentVoucher/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.Bank = ccm.ddlBank(string.Empty);
            ViewBag.COA = ccm.ddlAccountCOA(string.Empty);
            return View();
        }

        // POST: /AdvancePaymentVoucher/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(advancePaymentVoucher advancepaymentvoucher)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            #region collect detail purchase request
            var countChk = 0;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("txtAccountNo_"))
                    countChk++;
            }

            var model = new advancePaymentVoucher();
            for (int i = 1; i <= countChk; i++)
            {
                var colVal = Request.Form["txtAccountNo_" + i];
                if (colVal == "")
                    break;

                string t_accountNo = Request.Form["txtAccountNo_" + i].ToString();
                decimal t_amount = Request.Form["txtAmount_" + i].ToString() != "" ? Convert.ToDecimal(Request.Form["txtAmount_" + i].ToString().Replace('.', ',')) : 0;

                var editor = new advancePaymentVoucher.advancePaymentVoucherDetail()
                {
                    voucherNo = "",
                    accountNo = t_accountNo,
                    amount = t_amount
                };
                model.detailAdvancePaymentVoucher.Add(editor);
            }
            ViewData["dataDetail"] = model.detailAdvancePaymentVoucher.ToList();
            #endregion 

            ViewBag.Bank = ccm.ddlBank(advancepaymentvoucher.bankID.ToString());
            ViewBag.COA = ccm.ddlAccountCOA(string.Empty);
            if (ModelState.IsValid)
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        string generateID = prefix + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("d2");
                        var apvCreated = db.advancePaymentVouchers.Where(x => x.voucherNo.Contains(generateID)).OrderByDescending(x => x.createdDate).Select(x => x.voucherNo).ToList();
                        if (apvCreated.Count == 0)
                        {
                            generateID = generateID + "0001";
                        }
                        else
                        {
                            generateID = generateID + (Convert.ToInt32(apvCreated[0].Substring((apvCreated[0].Length - 4))) + 1).ToString().PadLeft(4, '0');
                        }

                        advancepaymentvoucher.voucherNo = generateID;
                        lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                        advancepaymentvoucher.createdUser = lvm.userID;
                        advancepaymentvoucher.createdDate = DateTime.Now;
                        advancepaymentvoucher.preparedBy = lvm.userID;
                        advancepaymentvoucher.preparedDate = DateTime.Now;
                        advancepaymentvoucher.preparedStatus = true;
                        db.advancePaymentVouchers.Add(advancepaymentvoucher);

                        #region insertDetail
                        for (int i = 1; i <= countChk; i++)
                        {
                            var colVal = Request.Form["txtAccountNo_" + i];
                            if (colVal == "")
                                break;

                            string t_accountNo = Request.Form["txtAccountNo_" + i].ToString();
                            decimal t_amount = Request.Form["txtAmount_" + i].ToString() != "" ? Convert.ToDecimal(Request.Form["txtAmount_" + i].ToString().Replace('.', ',')) : 0;

                            var editor = new advancePaymentVoucher.advancePaymentVoucherDetail()
                            {
                                voucherNo = advancepaymentvoucher.voucherNo,
                                accountNo = t_accountNo,
                                amount = t_amount
                            };
                            db.advancePaymentVoucherDetails.Add(editor);
                        }
                        #endregion

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

            return View(advancepaymentvoucher);
        }

        // GET: /AdvancePaymentVoucher/Edit/5
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
            advancePaymentVoucher advancepaymentvoucher = db.advancePaymentVouchers.Find(id);
            if (advancepaymentvoucher == null)
            {
                return HttpNotFound();
            }

            loadDetail(id);
            ViewBag.Bank = ccm.ddlBank(advancepaymentvoucher.bankID.ToString());
            ViewBag.COA = ccm.ddlAccountCOA(string.Empty);
            return View(advancepaymentvoucher);
        }

        // POST: /AdvancePaymentVoucher/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(advancePaymentVoucher advancepaymentvoucher)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            #region collect detail purchase request
            var countChk = 0;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("txtAccountNo_"))
                    countChk++;
            }

            var model = new advancePaymentVoucher();
            for (int i = 1; i <= countChk; i++)
            {
                var colVal = Request.Form["txtAccountNo_" + i];
                if (colVal == "")
                    break;

                string t_accountNo = Request.Form["txtAccountNo_" + i].ToString();
                decimal t_amount = Request.Form["txtAmount_" + i].ToString() != "" ? Convert.ToDecimal(Request.Form["txtAmount_" + i].ToString().Replace('.', ',')) : 0;

                var editor = new advancePaymentVoucher.advancePaymentVoucherDetail()
                {
                    voucherNo = "",
                    accountNo = t_accountNo,
                    amount = t_amount
                };
                model.detailAdvancePaymentVoucher.Add(editor);
            }
            ViewData["dataDetail"] = model.detailAdvancePaymentVoucher.ToList();
            #endregion

            ViewBag.Bank = ccm.ddlBank(advancepaymentvoucher.bankID.ToString());
            ViewBag.COA = ccm.ddlAccountCOA(string.Empty);
            if (ModelState.IsValid)
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                        advancepaymentvoucher.modifiedUser = lvm.userID;
                        advancepaymentvoucher.modifiedDate = DateTime.Now;
                        db.Entry(advancepaymentvoucher).State = EntityState.Modified;

                        #region insertDetail
                        bool runFirst = true;
                        for (int i = 1; i <= countChk; i++)
                        {
                            if (runFirst == true)
                            {
                                db.advancePaymentVoucherDetails.RemoveRange(db.advancePaymentVoucherDetails.Where(x => x.voucherNo == advancepaymentvoucher.voucherNo));
                                runFirst = false;
                            }

                            var colVal = Request.Form["txtAccountNo_" + i];
                            if (colVal == "")
                                break;

                            string t_accountNo = Request.Form["txtAccountNo_" + i].ToString();
                            decimal t_amount = Request.Form["txtAmount_" + i].ToString() != "" ? Convert.ToDecimal(Request.Form["txtAmount_" + i].ToString().Replace('.', ',')) : 0;

                            var editor = new advancePaymentVoucher.advancePaymentVoucherDetail()
                            {
                                voucherNo = advancepaymentvoucher.voucherNo,
                                accountNo = t_accountNo,
                                amount = t_amount
                            };
                            db.advancePaymentVoucherDetails.Add(editor);
                        }
                        #endregion

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

            return View(advancepaymentvoucher);
        }

        // GET: /AdvancePaymentVoucher/Delete/5
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
            advancePaymentVoucher advancepaymentvoucher = db.advancePaymentVouchers.Find(id);
            if (advancepaymentvoucher == null)
            {
                return HttpNotFound();
            }

            loadDetail(id);
            ViewBag.Bank = ccm.ddlBank(advancepaymentvoucher.bankID.ToString());
            ViewBag.COA = ccm.ddlAccountCOA(string.Empty);
            ViewBag.User = db.Users.ToList();
            return View(advancepaymentvoucher);
        }

        // POST: /AdvancePaymentVoucher/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            advancePaymentVoucher advancepaymentvoucher = db.advancePaymentVouchers.Find(id);
            db.advancePaymentVouchers.Remove(advancepaymentvoucher);
            db.advancePaymentVoucherDetails.RemoveRange(db.advancePaymentVoucherDetails.Where(x=>x.voucherNo==advancepaymentvoucher.voucherNo));
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
        protected void loadDetail(string id)
        {
            var model = new advancePaymentVoucher();
            var tempDetail = db.advancePaymentVoucherDetails.Where(x => x.voucherNo == id).ToList();
            foreach (var a in tempDetail)
            {
                string t_accountNo = a.accountNo;
                decimal t_amount = Convert.ToDecimal(a.amount);

                var editor = new advancePaymentVoucher.advancePaymentVoucherDetail()
                {
                    accountNo = t_accountNo,
                    amount = t_amount
                };
                model.detailAdvancePaymentVoucher.Add(editor);
            }
            ViewData["dataDetail"] = model.detailAdvancePaymentVoucher.ToList();
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
            advancePaymentVoucher advancepaymentvoucher = db.advancePaymentVouchers.Find(id);
            if (advancepaymentvoucher == null)
            {
                return HttpNotFound();
            }

            loadDetail(id);
            ViewBag.Bank = ccm.ddlBank(advancepaymentvoucher.bankID.ToString());
            ViewBag.COA = ccm.ddlAccountCOA(string.Empty);
            ViewBag.User = db.Users.ToList();
            return View(advancepaymentvoucher);
        }
    }
}
