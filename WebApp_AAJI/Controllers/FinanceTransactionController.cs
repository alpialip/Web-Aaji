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

namespace WebApp_AAJI.Controllers
{
    public class FinanceTransactionController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        string prefix = "FTR";

        [HttpGet]
        public async Task<ActionResult> LoadDetailFinanceTransaction(string dataDetail, string act)
        {
            string[] data = dataDetail.Split('|');

            var model = new financeTransactionHeader();
            for (int i = 0; i < data.Count(); i++)
            {
                string[] value = data[i].Split(';');

                if (value[1].ToString() == "")
                    break;

                string t_invoiceNo = value[1].ToString();
                DateTime t_invoiceDate = Convert.ToDateTime(value[2].ToString());
                string t_supplierName = value[3].ToString();
                decimal t_debt = value[4].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(value[4].ToString().Replace('.', ','))) : 0;
                decimal t_remains = value[5].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(value[5].ToString().Replace('.', ','))) : 0;
                decimal t_totalPay = value[6].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(value[6].ToString().Replace('.', ','))) : 0;

                var editor = new financeTransactionHeader.collectTransactionDetail()
                {
                     invoiceNo = t_invoiceNo,
                     invoiceDate = t_invoiceDate, 
                     supplierName = t_supplierName,
                     debt = t_debt,
                     remains = t_remains,
                     amount = t_totalPay
                };
                model.detailFinanceTransaction.Add(editor);
            }
            ViewData["dataDetail"] = model.detailFinanceTransaction.ToList();

            return PartialView("_PartialPageFT1");
        }

        // GET: /FinanceTransaction/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            return View(db.financeTransactionHeaders.ToList());
        }

        // GET: /FinanceTransaction/Details/5
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
            financeTransactionHeader financetransactionheader = db.financeTransactionHeaders.Find(id);
            if (financetransactionheader == null)
            {
                return HttpNotFound();
            }

            var tempFinancetransactionType = db.financeTransactionHeaders.AsNoTracking().Where(x => x.voucherNo == id)
                                        .Join(db.typeFinanceTransactions, a => a.tftID, b => b.id, (a, b) => new { a, b })
                                        .Select(c => new { tftID = c.a.tftID, tftName = c.b.tftName, transType = c.a.transactionType }).ToList();

            foreach(var a in tempFinancetransactionType)
            {
                financetransactionheader.transactionType = a.transType.Replace('_', ' ');
                ViewBag.tftName = a.tftName;
            }

            var tempFinancebillingType = db.financeTransactionHeaders.AsNoTracking().Where(x => x.voucherNo == id)
                                           .Join(db.chartOfAccounts.Where(y => y.levelID == 3), a => a.billingNo, b => b.id, (a, b) => new { a, b })
                                           .Select(c => new { c.a.billingType, accountName = "[" + c.b.accountNo + "] " + c.b.accountName }).ToList();
            foreach(var b in tempFinancebillingType)
            {
                financetransactionheader.billingType = b.billingType;
                ViewBag.billingNo = b.accountName;
            }

            #region detail
            var model = new financeTransactionHeader();
            var tempDetail = db.financeTransactionDetails.Where(x => x.voucherNo == id).ToList();
            foreach (var a in tempDetail)
            {
                string t_invoiceNo = a.invoiceNo.ToString();
                //DateTime t_invoiceDate = Convert.ToDateTime(.ToString());
                //string t_supplierName = value[3].ToString();
                //decimal t_debt = value[4].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(value[4].ToString().Replace('.', ','))) : 0;
                //decimal t_remains = value[5].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(value[5].ToString().Replace('.', ','))) : 0;
                decimal t_totalPay = a.amount.ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(a.amount.ToString().Replace('.', ','))) : 0;

                var editor = new financeTransactionHeader.collectTransactionDetail()
                {
                    invoiceNo = t_invoiceNo,
                    //invoiceDate = t_invoiceDate,
                    //supplierName = t_supplierName,
                    //debt = t_debt,
                    //remains = t_remains,
                    amount = t_totalPay
                };
                model.detailFinanceTransaction.Add(editor);
            }
            ViewData["dataDetail"] = model.detailFinanceTransaction.ToList();
            #endregion
            return View(financetransactionheader);
        }

        // GET: /FinanceTransaction/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.ddlFt = TypeFinanceTransactionController.DDLTypeFinanceTransaction(""); 
            ViewBag.ddlBt = DDLTypeBilling("");
            ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 )
                                    .Join(db.chartOfAccounts.Where(y => y.levelID == 2 && y.accountNo == "1001"), x => x.parentCOAId, y => y.id, (x, y) => x)
                                    .Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");

            ViewBag.TftCI = new SelectList(db.typeFinanceTransactions.Where(x => x.tftID.Contains("Cash_In")).Select(x => new { x.id, x.tftName }), "id", "tftName");
            ViewBag.TftCO = new SelectList(db.typeFinanceTransactions.Where(x => x.tftID.Contains("Cash_Out")).Select(x => new { x.id, x.tftName }), "id", "tftName");
            ViewBag.TftAR = new SelectList(db.typeFinanceTransactions.Where(x => x.tftID.Contains("AR")).Select(x => new { x.id, x.tftName }), "id", "tftName");
            ViewBag.TftAP = new SelectList(db.typeFinanceTransactions.Where(x => x.tftID.Contains("AP")).Select(x => new { x.id, x.tftName }), "id", "tftName");

            return View();
        }

        // POST: /FinanceTransaction/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,voucherNo,voucherDate,transactionType,tftID,billingType,billingNo,clearingDate,amount,remarks,createdUser,createdDate,modifiedUser,modifiedDate")] financeTransactionHeader financetransactionheader, string dataDetail)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            #region collect detail finance trans
            var countChk = 0;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("txtInvoiceNo_"))
                    countChk++;
            }

            var model = new financeTransactionHeader();
            for (int i = 1; i <= countChk; i++)
            {
                var colVal = Request.Form["txtInvoiceNo_" + i];
                if (colVal == "")
                    break;

                string t_invoiceNo = Request.Form["txtInvoiceNo_" + i].ToString();
                DateTime t_invoiceDate = Convert.ToDateTime(Request.Form["txtInvoiceDate_" + i].ToString());
                string t_supplierName = Request.Form["supplierName_" + i].ToString();
                decimal t_debt = Request.Form["txtDebt_" + i].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(Request.Form["txtDebt_" + i].ToString().Replace('.', ','))) : 0;
                decimal t_remains = Request.Form["txtRemains_" + i].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(Request.Form["txtRemains_" + i].ToString().Replace('.', ','))) : 0;
                decimal t_totalPay = Request.Form["txtPay_" + i].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(Request.Form["txtPay_" + i].ToString().Replace('.', ','))) : 0;

                var editor = new financeTransactionHeader.collectTransactionDetail()
                {
                    invoiceNo = t_invoiceNo,
                    invoiceDate = t_invoiceDate,
                    supplierName = t_supplierName,
                    debt = t_debt,
                    remains = t_remains,
                    amount = t_totalPay
                };
                model.detailFinanceTransaction.Add(editor);
            }
            ViewData["dataDetail"] = model.detailFinanceTransaction.ToList();
            #endregion 

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;

                try
                {
                    using(TransactionScope ts = new TransactionScope())
                    {
                        string vouchNo = prefix + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("d2");
                        var voucherNoCreated = db.financeTransactionHeaders.Where(x => x.voucherNo.Contains(vouchNo)).OrderByDescending(x => x.id).Select(x => x.voucherNo).ToList();
                        if (voucherNoCreated.Count == 0)
                        {
                            financetransactionheader.voucherNo = prefix + DateTime.Now.Year + DateTime.Now.Month.ToString("d2") + "0001";
                        }
                        else
                        {
                            financetransactionheader.voucherNo = prefix + DateTime.Now.Year + DateTime.Now.Month.ToString("d2") + (Convert.ToInt32(voucherNoCreated[0].Substring((voucherNoCreated[0].Length - 4))) + 1).ToString().PadLeft(4, '0');
                        }

                        financetransactionheader.createdDate = DateTime.Now;
                        financetransactionheader.createdUser = lvm.userID;
                        db.financeTransactionHeaders.Add(financetransactionheader);
                        db.SaveChanges();

                        if (financetransactionheader.id > 0)
                        {
                            #region insertDetail
                            for (int i = 1; i <= countChk; i++)
                            {
                                var colVal = Request.Form["txtInvoiceNo_" + i];
                                if (colVal == "")
                                    break;

                                string t_invoiceNo = Request.Form["txtInvoiceNo_" + i].ToString();
                                decimal t_totalPay = Request.Form["txtPay_" + i].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(Request.Form["txtPay_" + i].ToString().Replace('.', ','))) : 0;

                                db.financeTransactionDetails.Add(new financeTransactionHeader.financeTransactionDetail()
                                {
                                    voucherNo = financetransactionheader.voucherNo,
                                    invoiceNo = t_invoiceNo,
                                    amount = t_totalPay
                                });
                            }
                            #endregion

                            db.SaveChanges();
                        }
                        ts.Complete();
                    }
                    return RedirectToAction("Index");
                }
                catch (Exception exc)
                {
                    string a = exc.Message;
                }
            }

            return View(financetransactionheader);
        }

        // GET: /FinanceTransaction/Edit/5
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
            financeTransactionHeader financetransactionheader = db.financeTransactionHeaders.Find(id);
            if (financetransactionheader == null)
            {
                return HttpNotFound();
            }
            return View(financetransactionheader);
        }

        // POST: /FinanceTransaction/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="voucherNo,id,voucherDate,tftID,billingType,billingNo,clearingDate,amount,remarks,createdUser,createdDate,modifiedUser,modifiedDate")] financeTransactionHeader financetransactionheader)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;

                db.Entry(financetransactionheader).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(financetransactionheader);
        }

        // GET: /FinanceTransaction/Delete/5
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
            financeTransactionHeader financetransactionheader = db.financeTransactionHeaders.Find(id);
            if (financetransactionheader == null)
            {
                return HttpNotFound();
            }

            var tempFinancetransactionType = db.financeTransactionHeaders.AsNoTracking().Where(x => x.voucherNo == id)
                                        .Join(db.typeFinanceTransactions, a => a.tftID, b => b.id, (a, b) => new { a, b })
                                        .Select(c => new { tftID = c.a.tftID, tftName = c.b.tftName, transType = c.a.transactionType }).ToList();

            foreach (var a in tempFinancetransactionType)
            {
                financetransactionheader.transactionType = a.transType.Replace('_', ' ');
                ViewBag.tftName = a.tftName;
            }

            var tempFinancebillingType = db.financeTransactionHeaders.AsNoTracking().Where(x => x.voucherNo == id)
                                           .Join(db.chartOfAccounts.Where(y => y.levelID == 3), a => a.billingNo, b => b.id, (a, b) => new { a, b })
                                           .Select(c => new { c.a.billingType, accountName = "[" + c.b.accountNo + "] " + c.b.accountName }).ToList();
            foreach (var b in tempFinancebillingType)
            {
                financetransactionheader.billingType = b.billingType;
                ViewBag.billingNo = b.accountName;
            }

            #region detail
            var model = new financeTransactionHeader();
            var tempDetail = db.financeTransactionDetails.Where(x => x.voucherNo == id).ToList();
            foreach (var a in tempDetail)
            {
                string t_invoiceNo = a.invoiceNo.ToString();
                //DateTime t_invoiceDate = Convert.ToDateTime(.ToString());
                //string t_supplierName = value[3].ToString();
                //decimal t_debt = value[4].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(value[4].ToString().Replace('.', ','))) : 0;
                //decimal t_remains = value[5].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(value[5].ToString().Replace('.', ','))) : 0;
                decimal t_totalPay = a.amount.ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(a.amount.ToString().Replace('.', ','))) : 0;

                var editor = new financeTransactionHeader.collectTransactionDetail()
                {
                    invoiceNo = t_invoiceNo,
                    //invoiceDate = t_invoiceDate,
                    //supplierName = t_supplierName,
                    //debt = t_debt,
                    //remains = t_remains,
                    amount = t_totalPay
                };
                model.detailFinanceTransaction.Add(editor);
            }
            ViewData["dataDetail"] = model.detailFinanceTransaction.ToList();
            #endregion

            return View(financetransactionheader);
        }

        // POST: /FinanceTransaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            financeTransactionHeader financetransactionheader = db.financeTransactionHeaders.Find(id);
            db.financeTransactionHeaders.Remove(financetransactionheader);
            db.financeTransactionDetails.RemoveRange(db.financeTransactionDetails.Where(x => x.voucherNo == financetransactionheader.voucherNo));
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

        public static SelectList DDLTypeBilling(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "Cash", Text = "Cash" });
            list.Add(new SelectListItem { Value = "Bank", Text = "Bank" });
            list.Add(new SelectListItem { Value = "Giro", Text = "Giro" });


            return new SelectList(list, "Value", "Text", currSelection);
        }
    }
}
