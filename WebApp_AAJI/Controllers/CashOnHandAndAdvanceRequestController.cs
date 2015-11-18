using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
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
    public class CashOnHandAndAdvanceRequestController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private CommonController ccm = new CommonController();
        string prefix = "REQ";

        [HttpGet]
        public async Task<ActionResult> ApprovalProcess(string act, string id)
        {
            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            cashOnHandAndAdvanceRequest apv = db.cashOnHandAndAdvanceRequests.Find(id);
            apv.approvedStatus = act == "approved" ? true : false;
            apv.approvedDate = DateTime.Now;
            apv.approvedBy = lvm.userID;
            db.Entry(apv).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: /CashOnHandAndAdvanceRequest/
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

            ViewBag.user = db.Users.ToList();

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

            var data = from s in db.cashOnHandAndAdvanceRequests where s.reqIsCashOnHand == true select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                data = data.Where(s => s.reqNo.Contains(searchString));
            }
            else if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.date >= sDate && s.date <= eDate);
            }
            else if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate) && !String.IsNullOrEmpty(searchString))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.date >= sDate && s.date <= eDate && s.reqNo.Contains(searchString));
            }
            
            switch (sortOrder)
            {
                case "date_":
                    data = data.OrderByDescending(s => s.date);
                    break;
                case "voucherNo":
                    data = data.OrderBy(s => new { s.reqNo });
                    break;
                case "voucherNo_":
                    data = data.OrderByDescending(s => s.reqNo);
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
                    data = data.OrderByDescending(s => s.createdDate);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(data.ToPagedList(pageNumber, pageSize));
            #endregion
            //return View(db.cashOnHandAndAdvanceRequests.ToList());
        }

        // GET: /CashOnHandAndAdvanceRequest/Details/5
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
            cashOnHandAndAdvanceRequest cashonhandandadvancerequest = db.cashOnHandAndAdvanceRequests.Find(id);
            if (cashonhandandadvancerequest == null)
            {
                return HttpNotFound();
            }

            loadDetail(cashonhandandadvancerequest.reqNo);
            return View(cashonhandandadvancerequest);
        }

        // GET: /CashOnHandAndAdvanceRequest/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.isAdmin = lvm.isAdmin;
            if (lvm.isAdmin == true)
                ViewBag.User = ccm.ddlUserAll(string.Empty);
            else
                ViewBag.User = ccm.ddlUserAll(lvm.employeeNIK);

            ViewBag.Activity = ccm.ddlActivityType(string.Empty);
            return View();
        }

        // POST: /CashOnHandAndAdvanceRequest/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(cashOnHandAndAdvanceRequest cashonhandandadvancerequest)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.User = ccm.ddlUserAll(cashonhandandadvancerequest.reqFrom);
            ViewBag.Activity = ccm.ddlActivityType(cashonhandandadvancerequest.activity);

            #region collect detail request
            var countChk = 0;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("txtDesc_"))
                    countChk++;
            }
            #endregion 

            if (ModelState.IsValid)
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        string generateID = prefix + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("d2");
                        var apvCreated = db.cashOnHandAndAdvanceRequests.Where(x => x.reqNo.Contains(generateID)).OrderByDescending(x => x.createdDate).Select(x => x.reqNo).ToList();
                        if (apvCreated.Count == 0)
                        {
                            generateID = generateID + "0001";
                        }
                        else
                        {
                            generateID = generateID + (Convert.ToInt32(apvCreated[0].Substring((apvCreated[0].Length - 4))) + 1).ToString().PadLeft(4, '0');
                        }

                        cashonhandandadvancerequest.coaActivity = ccm.COA_ActivityType(cashonhandandadvancerequest.activity);
                        cashonhandandadvancerequest.reqNo = generateID;
                        cashonhandandadvancerequest.createdDate = DateTime.Now;
                        cashonhandandadvancerequest.createdUser = lvm.userID;
                        db.cashOnHandAndAdvanceRequests.Add(cashonhandandadvancerequest);

                        #region insertDetail
                            for (int i = 0; i < countChk; i++)
                            {
                                var colVal = Request.Form["txtDesc_" + i];
                                if (colVal == "")
                                    break;

                                string t_actDesc = Request.Form["txtDesc_" + i].ToString();
                                int t_currency = Request.Form["txtCur_" + i].ToString() != "" ? int.Parse(Request.Form["txtCur_" + i].ToString()) : 0;
                                decimal t_amount = Request.Form["txtAmount_" + i].ToString() != "" ? Convert.ToDecimal(Request.Form["txtAmount_" + i].ToString().Replace('.', ',')) : 0;

                                string t_imageUrl = null;
                                if(cashonhandandadvancerequest.isReimbursement == true)
                                {
                                    var file = Request.Files["linkFileData_" + i];
                                    if (file != null && file.ContentLength > 0)
                                    {
                                        var imagePath = Path.Combine(Server.MapPath(ccm.uploadDir), file.FileName);
                                        t_imageUrl = Path.Combine(ccm.uploadDir, file.FileName);
                                        file.SaveAs(imagePath);
                                    }
                                }

                                var editor = new cashOnHandAndAdvanceRequest.cashOnHandAndAdvanceRequestDetail()
                                {
                                    seqId = (i+1),
                                    reqNo = cashonhandandadvancerequest.reqNo,
                                    activityDescription = t_actDesc,
                                    currency = t_currency,
                                    amount = t_amount,
                                    fileReimbursement = t_imageUrl
                                };
                                db.cashOnHandAndAdvanceRequestDetails.Add(editor);
                            }
                        #endregion
                    
                        db.SaveChanges();
                        ts.Complete();
                    }

                    if (cashonhandandadvancerequest.reqIsCashOnHand == true)
                        return RedirectToAction("Index");
                    else
                        return RedirectToAction("Index","AdvanceRequest");
                }
                catch(Exception exc)
                {
                    string msg = exc.Message;
                }
            }

            return View(cashonhandandadvancerequest);
        }

        // GET: /CashOnHandAndAdvanceRequest/Edit/5
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
            cashOnHandAndAdvanceRequest cashonhandandadvancerequest = db.cashOnHandAndAdvanceRequests.Find(id);
            if (cashonhandandadvancerequest == null)
            {
                return HttpNotFound();
            }

            loadDetail(cashonhandandadvancerequest.reqNo);
            return View(cashonhandandadvancerequest);
        }

        // POST: /CashOnHandAndAdvanceRequest/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(cashOnHandAndAdvanceRequest cashonhandandadvancerequest)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            ViewBag.User = ccm.ddlUserAll(cashonhandandadvancerequest.reqFrom);
            ViewBag.Activity = ccm.ddlActivityType(cashonhandandadvancerequest.activity);

            #region collect detail request
            var countChk = 0;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("txtDesc_"))
                    countChk++;
            }
            #endregion 

            if (ModelState.IsValid)
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        cashonhandandadvancerequest.coaActivity = ccm.COA_ActivityType(cashonhandandadvancerequest.activity);
                        cashonhandandadvancerequest.modifiedDate = DateTime.Now;
                        cashonhandandadvancerequest.modifiedUser = lvm.userID;
                        db.Entry(cashonhandandadvancerequest).State = EntityState.Modified;

                        #region insertDetail
                        bool runFirst = true;
                        for (int i = 0; i < countChk; i++)
                        {
                            if (runFirst == true)
                            {
                                db.cashOnHandAndAdvanceRequestDetails.RemoveRange(db.cashOnHandAndAdvanceRequestDetails.Where(x => x.reqNo == cashonhandandadvancerequest.reqNo));
                                runFirst = false;
                            }

                            var colVal = Request.Form["txtDesc_" + i];
                            if (colVal == "")
                                break;

                            int t_seqId = int.Parse(Request.Form["seqId_" + i].ToString());
                            string t_actDesc = Request.Form["txtDesc_" + i].ToString();
                            int t_currency = Request.Form["txtCur_" + i].ToString() != "" ? int.Parse(Request.Form["txtCur_" + i].ToString()) : 0;
                            decimal t_amount = Request.Form["txtAmount_" + i].ToString() != "" ? Convert.ToDecimal(Request.Form["txtAmount_" + i].ToString().Replace('.', ',')) : 0;

                            var dtDetail = db.cashOnHandAndAdvanceRequestDetails.AsNoTracking().Where(x => x.reqNo == cashonhandandadvancerequest.reqNo && x.seqId == t_seqId).ToList();

                            string t_imageUrl = null;
                            if (dtDetail.Count > 0)
                                if(dtDetail[0].fileReimbursement != null)
                                    if (dtDetail[0].fileReimbursement.ToString() != string.Empty)
                                    {
                                        t_imageUrl = dtDetail[0].fileReimbursement.ToString();
                                    }

                            if (cashonhandandadvancerequest.isReimbursement == true)
                            {
                                var file = Request.Files["linkFileData_" + i];
                                if (file != null && file.ContentLength > 0)
                                {
                                    var imagePath = Path.Combine(Server.MapPath(ccm.uploadDir), file.FileName);
                                    t_imageUrl = Path.Combine(ccm.uploadDir, file.FileName);
                                    file.SaveAs(imagePath);
                                }
                            }

                            var editor = new cashOnHandAndAdvanceRequest.cashOnHandAndAdvanceRequestDetail()
                            {
                                seqId = (i + 1),
                                reqNo = cashonhandandadvancerequest.reqNo,
                                activityDescription = t_actDesc,
                                currency = t_currency,
                                amount = t_amount,
                                fileReimbursement = t_imageUrl
                            };
                            db.cashOnHandAndAdvanceRequestDetails.Add(editor);
                        }
                        #endregion

                        db.SaveChanges();
                        ts.Complete();
                    }

                    if (cashonhandandadvancerequest.reqIsCashOnHand == true)
                        return RedirectToAction("Index");
                    else
                        return RedirectToAction("Index", "AdvanceRequest");
                }
                catch (Exception exc)
                {
                    string msg = exc.Message;
                }
            }
            return View(cashonhandandadvancerequest);
        }

        // GET: /CashOnHandAndAdvanceRequest/Delete/5
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
            cashOnHandAndAdvanceRequest cashonhandandadvancerequest = db.cashOnHandAndAdvanceRequests.Find(id);
            if (cashonhandandadvancerequest == null)
            {
                return HttpNotFound();
            }

            loadDetail(cashonhandandadvancerequest.reqNo);
            return View(cashonhandandadvancerequest);
        }

        // POST: /CashOnHandAndAdvanceRequest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            cashOnHandAndAdvanceRequest cashonhandandadvancerequest = db.cashOnHandAndAdvanceRequests.Find(id);
            db.cashOnHandAndAdvanceRequests.Remove(cashonhandandadvancerequest);
            db.cashOnHandAndAdvanceRequestDetails.RemoveRange(db.cashOnHandAndAdvanceRequestDetails.Where(x => x.reqNo == cashonhandandadvancerequest.reqNo));
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

        protected void loadDetail(string reqNo)
        {
            if(reqNo != string.Empty)
            {
                var dtResult = db.cashOnHandAndAdvanceRequests.AsNoTracking().Where(x => x.reqNo == reqNo).ToList();
                string reqFrom = dtResult[0].reqFrom;
                string activity = dtResult[0].activity;
                bool reqType = dtResult[0].reqIsCashOnHand;
                    ViewBag.User = ccm.ddlUserAll(reqFrom);
                    ViewBag.Activity = ccm.ddlActivityType(activity);
                    ViewBag.ReqType = reqType;

                var detailResult = db.cashOnHandAndAdvanceRequestDetails.AsNoTracking().Where(x => x.reqNo == reqNo).ToList();
                var model = new cashOnHandAndAdvanceRequest();
                foreach (var a in detailResult)
                {
                    var editor = new cashOnHandAndAdvanceRequest.cashOnHandAndAdvanceRequestDetail()
                    {
                        seqId = a.seqId,
                        reqNo = a.reqNo,
                        activityDescription = a.activityDescription,
                        currency = a.currency,
                        amount = a.amount,
                        fileReimbursement = a.fileReimbursement
                    };
                    model.detailCashOnHandAndAdvanceRequest.Add(editor);
                }
                ViewBag.dataDetail = model.detailCashOnHandAndAdvanceRequest.ToList();
            }
        }


        public ActionResult Approval(string id)
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
            cashOnHandAndAdvanceRequest cashonhandandadvancerequest = db.cashOnHandAndAdvanceRequests.Find(id);
            if (cashonhandandadvancerequest == null)
            {
                return HttpNotFound();
            }

            loadDetail(cashonhandandadvancerequest.reqNo);

            return View(cashonhandandadvancerequest);
        }
    }
}
