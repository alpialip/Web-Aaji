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
using WebApp_AAJI.Controllers;
using WebApp_AAJI.Models;
using PagedList;

namespace WebApplicationTest.Controllers
{
    public class AccountPayableController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private CommonController ccm = new CommonController();
        private ConnectionController con = new ConnectionController();
        string prefix = "AP";

        [HttpGet]
        public async Task<ActionResult> LoadDetail(string dataDetail, string act)
        {
            string[] data = dataDetail.Split('|');

            if (data[0].Length > 0)
            { 
                var model = new accountPayableHeader();
                for (int i = 0; i < data.Count(); i++)
                {
                    string[] value = data[i].Split(';');

                    if (value[1].ToString() == "")
                        break;

                    string t_invoiceNo = value[1].ToString();
                    decimal t_amountDebt = Decimal.Parse(value[2].ToString());
                    decimal t_amountPayment = Decimal.Parse(value[3].ToString());

                    var editor = new accountPayableHeader.accountPayableDetailView()
                    {
                        invoiceNo = t_invoiceNo,
                        amountDebt = t_amountDebt,
                        amountPayment = t_amountPayment
                    };
                    model.viewDetailAccountPayable.Add(editor);
                }
                ViewData["dataDetail"] = model.viewDetailAccountPayable.ToList();

            }
            else
            {
                ViewData["dataDetail"] = null;
            }
            return PartialView("_PartialPageAP1");
        }

        [HttpGet]
        public async Task<ActionResult> popUpSearchVendor(string keywordId, string keywordSearch, int maxRows, int pageStart)
        {
            string sql = "WITH cte AS ( ";
            sql += "SELECT ROW_NUMBER() OVER(ORDER BY id) AS rn, ";
            sql += "[id],[vendorID],[vendorName],[npwp],[address1],[address2],[telp],[fax],[contactPerson],[ppn],[top],[createdUser],[createdDate],[modifiedUser],[modifiedDate] ";
            sql += "FROM [dbo].[Vendors] a WITH(NOLOCK) ";
            if (keywordId != string.Empty && keywordSearch != string.Empty)
            {
                sql += "WHERE " + keywordId + " LIKE  '%" + keywordSearch + "%' ";
            }
            sql += ") ";
            sql += "SELECT * FROM cte ";
            string sqlAll = sql;
            sql += "WHERE rn BETWEEN " + ((pageStart * maxRows) - (maxRows - 1)) + " AND " + (pageStart * maxRows);

            DataTable dataAll = con.executeReader(sqlAll);
            ViewBag.totalRows = dataAll.Rows.Count;
            ViewBag.currentPage = pageStart;

            DataTable popUp = con.executeReader(sql);
            List<vendor> model = new List<vendor>();
            if (popUp != null)
            {
                foreach (DataRow dr in popUp.Rows)
                {
                    DateTime? modified = dr["modifiedDate"].ToString() == string.Empty ? DateTime.Now : Convert.ToDateTime(dr["modifiedDate"].ToString());
                    var editor = new vendor()
                    {
                        id = int.Parse(dr["id"].ToString()),
                        vendorID = dr["vendorID"].ToString(),
                        vendorName = dr["vendorName"].ToString(),
                        npwp = dr["npwp"].ToString(),
                        address1 = dr["address1"].ToString(),
                        address2 = dr["address2"].ToString(),
                        telp = dr["telp"].ToString(),
                        fax = dr["fax"].ToString(),
                        contactPerson = dr["contactPerson"].ToString(),
                        ppn = bool.Parse(dr["ppn"].ToString()),
                        top = int.Parse(dr["top"].ToString()),
                        createdUser = dr["createdUser"].ToString(),
                        createdDate = DateTime.Parse(dr["createdDate"].ToString()),
                        modifiedDate = modified,
                        modifiedUser = dr["modifiedUser"].ToString()
                    };
                    model.Add(editor);
                }
                ViewBag.VendorPopUp = model.ToList();
            }

            return PartialView("_PartialPagePopUpVendorSearchSub1");
        }

        [HttpGet]
        public async Task<ActionResult> popUpSearch(string keywordId, string keywordSearch, int maxRows, int pageStart)
        {
            string sql = "WITH cte AS ( ";
            sql += "SELECT ROW_NUMBER() OVER(ORDER BY purchaseInvoiceNo) AS rn, ";
            sql += "a.purchaseInvoiceNo, a.invoiceDate, a.dueDate, a.paymentInfo, a.remarks, a.total ,c.vendorId ";
            sql += "FROM [dbo].[PurchaseInvoices] a WITH(NOLOCK) ";
            sql += "INNER JOIN [dbo].[PurchaseReceives] b ON b.receiveNo = a.receiveNo AND b.[receivedStatus] = 1 ";
            sql += "INNER JOIN [dbo].[PurchaseOrderHeaders] c ON c.poId = b.poId AND c.proposedStatus = 1 AND c.approvedStatus = 1 ";
            sql += "WHERE a.purchaseInvoiceNo NOT IN (SELECT invoiceNo FROM [dbo].[AccountPayableDetails]) ";
            //sql += "AND c.vendorId = (SELECT vendorId FROM dbo.Vendors WHERE id = " + vendorId + ") ";
            if (keywordId != string.Empty && keywordSearch != string.Empty && keywordId.Contains("a.purchaseInvoiceNo"))
            {
                sql += "AND " + keywordId + " LIKE  '%" + keywordSearch + "%' ";
            }
            else
            {
                sql += "AND " + keywordId + keywordSearch;
            }
            sql += ") ";
            sql += "SELECT * FROM cte ";
            string sqlAll = sql;
            sql += "WHERE rn BETWEEN " + ((pageStart * maxRows) - (maxRows - 1)) + " AND " + (pageStart * maxRows);

            DataTable dataAll = con.executeReader(sqlAll);
            ViewBag.totalRows = dataAll.Rows.Count;
            ViewBag.currentPage = pageStart;

            DataTable popUp = con.executeReader(sql);
            ViewBag.InvoicePopUp = popUp;
            ViewBag.totalRows = popUp.Rows.Count;

            return PartialView("_PartialPagePopUpInvoiceSearchSub");
        }

        [HttpGet]
        public async Task<ActionResult> refreshInvoiceByVendorId(string vendorId)
        {
            string sql = "WITH cte AS ( ";
            sql += "SELECT ROW_NUMBER() OVER(ORDER BY purchaseInvoiceNo) AS rn, ";
            sql += "a.purchaseInvoiceNo, a.invoiceDate, a.dueDate, a.paymentInfo, a.remarks, a.total ,c.vendorId ";
            sql += "FROM [dbo].[PurchaseInvoices] a WITH(NOLOCK) ";
            sql += "INNER JOIN [dbo].[PurchaseReceives] b ON b.receiveNo = a.receiveNo AND b.[receivedStatus] = 1 ";
            sql += "INNER JOIN [dbo].[PurchaseOrderHeaders] c ON c.poId = b.poId AND c.proposedStatus = 1 AND c.approvedStatus = 1 ";
            sql += "WHERE a.isCheckDocument = 1 AND a.purchaseInvoiceNo NOT IN (SELECT invoiceNo FROM [dbo].[AccountPayableDetails]) ";
            //sql += "AND c.vendorId = (SELECT vendorId FROM dbo.Vendors WHERE id = " + vendorId + ") ";
            sql += "AND c.vendorId = '" + vendorId + "' ";
            sql += ") ";
            sql += "SELECT * FROM cte ";
            string sqlAll = sql;

            DataTable dataAll = con.executeReader(sqlAll);
            ViewBag.totalRows = dataAll.Rows.Count;
            ViewBag.currentPage = 1;

            DataTable popUp = con.executeReader(sql);
            ViewBag.InvoicePopUp = popUp;
            ViewBag.totalRows = popUp.Rows.Count;

            return PartialView("_PartialPagePopUpInvoiceSearch");
        }

        // GET: /AccountPayable/
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, string startDateFilter, string startDate, string endDateFilter, string endDate, int? page)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.Bank = db.banks.ToList();
            ViewBag.Vendor = db.vendors.ToList();
            ViewBag.User = db.Users.ToList();

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

            var data = from s in db.accountPayableHeaders select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                data = data.Where(s => s.voucherNo.Contains(searchString));
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
                data = data.Where(s => s.date >= sDate && s.date <= eDate && s.voucherNo.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "date_":
                    data = data.OrderByDescending(s => s.date);
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
            //return View(db.accountPayableHeaders.ToList());
        }

        // GET: /AccountPayable/Details/5
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
            accountPayableHeader accountpayable = db.accountPayableHeaders.Find(id);
            if (accountpayable == null)
            {
                return HttpNotFound();
            }

            loadDetailView(accountpayable);
            return View(accountpayable);
        }

        // GET: /AccountPayable/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.Bank = ccm.ddlBank(string.Empty);
            ViewBag.DDLpaymentType = ccm.ddlPaymentType(string.Empty);
            ViewBag.VendorPopUp = db.vendors.ToList();
            ViewBag.totalRowsVendor = db.vendors.Count();

            return View();
        }

        // POST: /AccountPayable/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(accountPayableHeader accountpayable)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            #region collect detail account payable
            var countChk = 0;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("txtInvoiceNo_"))
                    countChk++;
            }

            var model = new accountPayableHeader();
            for (int i = 1; i <= countChk; i++)
            {
                var colVal = Request.Form["txtInvoiceNo_" + i];
                if (colVal == "")
                    break;

                string t_invoiceNo = Request.Form["txtInvoiceNo_" + i].ToString();
                decimal t_amountDebt = decimal.Parse(Request.Form["txtAmount_" + i].ToString());
                decimal t_amountPayment = decimal.Parse(Request.Form["txtPaymentAmount_" + i].ToString());

                var editor = new accountPayableHeader.accountPayableDetailView()
                {
                    invoiceNo = t_invoiceNo,
                    amountDebt = t_amountDebt,
                    amountPayment = t_amountPayment
                };
                model.viewDetailAccountPayable.Add(editor);
            }
            ViewData["dataDetail"] = model.viewDetailAccountPayable.ToList();
            #endregion 
                        
            ViewBag.Bank = ccm.ddlBank(accountpayable.bankId.ToString());
            ViewBag.DDLpaymentType = ccm.ddlPaymentType(accountpayable.paymentType);
            ViewBag.VendorPopUp = db.vendors.ToList();
            ViewBag.totalRowsVendor = db.vendors.Count();
            if (ModelState.IsValid)
            {
                try
                {
                    using(TransactionScope ts = new TransactionScope())
                    {
                        string generateID = prefix + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("d2");
                        var prIDCreated = db.accountPayableHeaders.Where(x => x.voucherNo.Contains(generateID)).OrderByDescending(x => x.createdDate).Select(x => x.voucherNo).ToList();
                        if (prIDCreated.Count == 0)
                        {
                            generateID = generateID + "0001";
                        }
                        else
                        {
                            generateID = generateID + (Convert.ToInt32(prIDCreated[0].Substring((prIDCreated[0].Length - 4))) + 1).ToString().PadLeft(4, '0');
                        }

                        accountpayable.voucherNo = generateID;
                        accountpayable.createdUser = lvm.userID;
                        accountpayable.createdDate = DateTime.Now;
                        db.accountPayableHeaders.Add(accountpayable);

                        #region insertDetail
                        for (int i = 1; i <= countChk; i++)
                        {
                            var colVal = Request.Form["txtInvoiceNo_" + i];
                            if (colVal == "")
                                break;

                            string t_invoiceNo = Request.Form["txtInvoiceNo_" + i].ToString();
                            decimal t_amountDebt = decimal.Parse(Request.Form["txtAmount_" + i].ToString());
                            decimal t_amountPayment = decimal.Parse(Request.Form["txtPaymentAmount_" + i].ToString());

                            var editor = new accountPayableHeader.accountPayableDetail()
                            {
                                voucherNo = accountpayable.voucherNo,
                                invoiceNo = t_invoiceNo,
                                amountPayment = t_amountPayment
                            };
                            db.accountPayableDetails.Add(editor);
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

            return View(accountpayable);
        }

        // GET: /AccountPayable/Edit/5
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
            accountPayableHeader accountpayable = db.accountPayableHeaders.Find(id);
            if (accountpayable == null)
            {
                return HttpNotFound();
            }

            loadDetailView(accountpayable);
            return View(accountpayable);
        }

        // POST: /AccountPayable/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(accountPayableHeader accountpayable)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            #region collect detail account payable
            var countChk = 0;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("txtInvoiceNo_"))
                    countChk++;
            }

            var model = new accountPayableHeader();
            for (int i = 1; i <= countChk; i++)
            {
                var colVal = Request.Form["txtInvoiceNo_" + i];
                if (colVal == "")
                    break;

                string t_invoiceNo = Request.Form["txtInvoiceNo_" + i].ToString();
                decimal t_amountDebt = decimal.Parse(Request.Form["txtAmount_" + i].ToString());
                decimal t_amountPayment = decimal.Parse(Request.Form["txtPaymentAmount_" + i].ToString());

                var editor = new accountPayableHeader.accountPayableDetailView()
                {
                    invoiceNo = t_invoiceNo,
                    amountDebt = t_amountDebt,
                    amountPayment = t_amountPayment
                };
                model.viewDetailAccountPayable.Add(editor);
            }
            ViewData["dataDetail"] = model.viewDetailAccountPayable.ToList();
            #endregion

            ViewBag.Bank = ccm.ddlBank(accountpayable.bankId.ToString());
            ViewBag.DDLpaymentType = ccm.ddlPaymentType(accountpayable.paymentType);
            ViewBag.VendorPopUp = db.vendors.ToList();
            ViewBag.totalRowsVendor = db.vendors.Count();
            if (ModelState.IsValid)
            {
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(accountpayable).State = EntityState.Modified;

                        #region insertDetail
                        bool runFirst = true;
                        for (int i = 1; i <= countChk; i++)
                        {
                            if (runFirst == true)
                            {
                                db.accountPayableDetails.RemoveRange(db.accountPayableDetails.Where(x => x.voucherNo == accountpayable.voucherNo));
                                runFirst = false;
                            }

                            var colVal = Request.Form["txtInvoiceNo_" + i];
                            if (colVal == "")
                                break;

                            string t_invoiceNo = Request.Form["txtInvoiceNo_" + i].ToString();
                            decimal t_amountDebt = decimal.Parse(Request.Form["txtAmount_" + i].ToString());
                            decimal t_amountPayment = decimal.Parse(Request.Form["txtPaymentAmount_" + i].ToString());

                            var editor = new accountPayableHeader.accountPayableDetail()
                            {
                                voucherNo = accountpayable.voucherNo,
                                invoiceNo = t_invoiceNo,
                                amountPayment = t_amountPayment
                            };
                            db.accountPayableDetails.Add(editor);
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

            if (ModelState.IsValid)
            {
                
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(accountpayable);
        }

        // GET: /AccountPayable/Delete/5
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
            accountPayableHeader accountpayable = db.accountPayableHeaders.Find(id);
            if (accountpayable == null)
            {
                return HttpNotFound();
            }

            loadDetailView(accountpayable);
            return View(accountpayable);
        }

        // POST: /AccountPayable/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            accountPayableHeader accountpayable = db.accountPayableHeaders.Find(id);
            db.accountPayableHeaders.Remove(accountpayable);
            db.accountPayableDetails.RemoveRange(db.accountPayableDetails.Where(x => x.voucherNo == id));
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

        public void loadDetailView(accountPayableHeader accountpayable)
        {
            ViewBag.Bank = ccm.ddlBank(accountpayable.bankId.ToString());
            ViewBag.DDLpaymentType = ccm.ddlPaymentType(accountpayable.paymentType);
            ViewBag.VendorPopUp = db.vendors.ToList();
            ViewBag.totalRowsVendor = db.vendors.Count();

            var apd = db.accountPayableDetails.Where(x => x.voucherNo == accountpayable.voucherNo).ToList();

            var model = new accountPayableHeader();
            foreach(var a in apd as IEnumerable<accountPayableHeader.accountPayableDetail>)
            {
                string sql = "SELECT a.invoiceNo, c.total, a.amountPayment, b.voucherNo ";
                sql += "FROM [dbo].[AccountPayableDetails] a ";
                sql += "INNER JOIN [dbo].[AccountPayableHeaders] b ON b.voucherNo = a.voucherNo ";
                sql += "INNER JOIN [dbo].[PurchaseInvoices] c ON c.purchaseInvoiceNo = a.invoiceNo ";
                sql += "WHERE a.invoiceNo = '" + a.invoiceNo + "'";

                DataTable dataResult = con.executeReader(sql);
                decimal amountDebt = 0;
                foreach(DataRow dr in dataResult.Rows)
                {
                    amountDebt = decimal.Parse(dr["total"].ToString());
                }      
                var editor = new accountPayableHeader.accountPayableDetailView()
                    {
                        invoiceNo = a.invoiceNo,
                        amountDebt = amountDebt,
                        amountPayment = a.amountPayment
                    };
                model.viewDetailAccountPayable.Add(editor);
            }
            ViewData["dataDetail"] = model.viewDetailAccountPayable.ToList();
        }
    }
}
