using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;
using PagedList;
using System.Threading.Tasks;

namespace WebApp_AAJI.Controllers
{
    public class PurchaseInvoiceController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        public string prefix = "INV";

        [HttpGet]
        public async Task<ActionResult> ApprovalCheckDocument(string act, string id)
        {
            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            
            purchaseInvoice pInv = db.purchaseInvoices.Find(id);
            pInv.isCheckDocument = act.ToLower() == "approved" ? true : false;
            pInv.isCheckDocumentDate = DateTime.Now;
            pInv.isCheckDocumentUser = lvm.userID;
            db.Entry(pInv).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: /PurchaseInvoice/
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, string startDateFilter, string startDate, string endDateFilter, string endDate, int? page)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            var invoiceFromPR = db.purchaseInvoices
                                    .Join(db.purchaseReceives.Where(x=>x.receivedStatus == true), a => a.receiveNo, b => b.receiveNo, (a, b) => new { a,b }).ToList();
            List<string> listPR = new List<string>();
            for (int i = 0; i < invoiceFromPR.Count; i++ )
            {
                listPR.Add(invoiceFromPR[i].a.receiveNo);
            }
            ViewBag.invoiceFromPR = listPR.ToList();
            
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

            var data = from s in db.purchaseInvoices select s;
            if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate) && !String.IsNullOrEmpty(searchString))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.invoiceDate >= sDate && s.invoiceDate <= eDate && s.purchaseInvoiceNo.Contains(searchString));
            }
            else if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.invoiceDate >= sDate && s.invoiceDate <= eDate);
            }
            else if (!String.IsNullOrEmpty(searchString))
            {
                data = data.Where(s => s.purchaseInvoiceNo.Contains(searchString));
            }


            if (lvm.isAdmin == false && ViewBag.ApprovalAuth == false)
                data = data.Where(x => x.createdUser == lvm.userID);

            switch (sortOrder)
            {
                case "date_":
                    data = data.OrderByDescending(s => s.invoiceDate);
                    break;
                case "voucherNo":
                    data = data.OrderBy(s => new { s.purchaseInvoiceNo });
                    break;
                case "voucherNo_":
                    data = data.OrderByDescending(s => s.purchaseInvoiceNo);
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
            //    return View(db.purchaseInvoices.Where(x => x.createdUser == lvm.userID).ToList());
            //else if (lvm.isAdmin == false && ViewBag.ApprovalAuth == true)
            //    return View(db.purchaseInvoices.ToList());
            //else
            //    return View(db.purchaseInvoices.ToList());
            #endregion
        }

        // GET: /PurchaseInvoice/Details/5
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
            purchaseInvoice purchaseinvoice = db.purchaseInvoices.Find(id);
            if (purchaseinvoice == null)
            {
                return HttpNotFound();
            }
            return View(purchaseinvoice);
        }

        // GET: /PurchaseInvoice/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            return View();
        }

        // POST: /PurchaseInvoice/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="purchaseInvoiceNo,invoiceDate,dueDate,receiveNo,paymentInfo,remarks,total,createdUser,createdDate,modifiedUser,modifiedDate")] purchaseInvoice purchaseinvoice)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                string generateID = prefix + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("d2");
                var prIDCreated = db.purchaseInvoices.Where(x => x.purchaseInvoiceNo.Contains(generateID)).OrderByDescending(x => x.createdDate).Select(x => x.purchaseInvoiceNo).ToList();
                if (prIDCreated.Count == 0)
                {
                    generateID = generateID + "0001";
                }
                else
                {
                    generateID = generateID + (Convert.ToInt32(prIDCreated[0].Substring((prIDCreated[0].Length - 4))) + 1).ToString().PadLeft(4, '0');
                }

                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                purchaseinvoice.purchaseInvoiceNo = generateID;
                purchaseinvoice.createdUser = lvm.userID;
                purchaseinvoice.createdDate = DateTime.Now;
                db.purchaseInvoices.Add(purchaseinvoice);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(purchaseinvoice);
        }

        // GET: /PurchaseInvoice/Edit/5
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
            purchaseInvoice purchaseinvoice = db.purchaseInvoices.Find(id);
            if (purchaseinvoice == null)
            {
                return HttpNotFound();
            }
            var isReceiveFromTransPurchase = db.purchaseReceives.Where(x => x.receiveNo == purchaseinvoice.receiveNo).ToList();
            if (isReceiveFromTransPurchase.Count > 0)
                ViewBag.isReceiveFromTransPurchase = true;
            else
                ViewBag.isReceiveFromTransPurchase = false;

            return View(purchaseinvoice);
        }

        // POST: /PurchaseInvoice/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="purchaseInvoiceNo,invoiceDate,dueDate,receiveNo,paymentInfo,remarks,total,createdUser,createdDate,modifiedUser,modifiedDate")] purchaseInvoice purchaseinvoice)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                purchaseinvoice.createdUser = lvm.userID;
                purchaseinvoice.createdDate = DateTime.Now;
                db.Entry(purchaseinvoice).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(purchaseinvoice);
        }

        // GET: /PurchaseInvoice/Delete/5
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

            purchaseInvoice purchaseinvoice = db.purchaseInvoices.Find(id);
            if (purchaseinvoice == null)
            {
                return HttpNotFound();
            }
            return View(purchaseinvoice);
        }

        // POST: /PurchaseInvoice/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            purchaseInvoice purchaseinvoice = db.purchaseInvoices.Find(id);
            db.purchaseInvoices.Remove(purchaseinvoice);
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

            purchaseInvoice purchaseinvoice = db.purchaseInvoices.Find(id);
            if (purchaseinvoice == null)
            {
                return HttpNotFound();
            }
            return View(purchaseinvoice);
        }
    }
}
