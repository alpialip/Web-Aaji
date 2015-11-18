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
    public class JournalAdjustmentController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();

        [HttpGet]
        public async Task<ActionResult> LoadDetailCategoryProducts(string dataDetail, string act)
        {
            string[] data = dataDetail.Split('|');
            
            var model = new journalHeader();
            for (int i = 0; i < data.Count(); i++ )
            {
                string[] value = data[i].Split(';');

                if (value[1].ToString() == "")
                    break;

                int idCoa = Convert.ToInt32(value[1].ToString());
                string othrAccountName = value[2].ToString();
                string remak = value[3].ToString();
                decimal debt = value[4].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(value[4].ToString().Replace('.', ','))) : 0;
                decimal credt = value[5].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(value[5].ToString().Replace('.', ','))) : 0;

                var editor = new journalHeader.journalDetail()
                {
                    coaId = idCoa,
                    accountNameOther = othrAccountName,
                    remarks = remak,
                    debit = debt,
                    credit = credt
                };
                model.detailJournal.Add(editor);

                ViewData["ddlAccount" + (i + 1)] = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { x.id, accountName = x.accountName + " [" + x.accountNo + "]" }), "id", "accountName", idCoa);
            }
            ViewData["dataDetail"] = model.detailJournal.ToList();

            //ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { x.id, accountName = /*"[" + */x.accountNo/* + "] " +  x.accountName */ }), "id", "accountName");
            ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { id = x.id + "-" + x.accountName, accountName = x.accountName + " [" + x.accountNo + "]" }), "id", "accountName");
            //var lookupId = int.Parse(categoryId);
            ////var model = await this.GetFullAndPartialViewModel(lookupId);
            return PartialView("_PartialPage1");
        }

        // GET: /JournalAdjustment/
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

            var data = from s in db.journalHeaders select s;
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
            //return View(db.journalHeaders.ToList());
        }

        // GET: /JournalAdjustment/Details/5
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
            journalHeader journalheader = db.journalHeaders.Find(id);
            if (journalheader == null)
            {
                return HttpNotFound();
            }
            
            #region getDataDetail
            var data = db.journalDetails.Where(x => x.voucherNo == id).ToList();
            var model = new journalHeader();
            for (int i = 0; i < data.Count(); i++)
            {
                int idCoa = Convert.ToInt32(data[i].coaId.ToString());
                string othrAccountName = data[i].accountNameOther.ToString();
                string remak = data[i].remarks.ToString();
                decimal debt = Convert.ToDecimal(data[i].debit.ToString());
                decimal credt = Convert.ToDecimal(data[i].credit.ToString());

                var coaName = db.chartOfAccounts.Where(x => x.id == idCoa).ToList();
                var editor = new journalHeader.journalDetail()
                {
                    coaId = idCoa,
                    accountNameOther = coaName[0].accountName/*othrAccountName*/,
                    remarks = remak,
                    debit = debt,
                    credit = credt
                };
                model.detailJournal.Add(editor);

                //ViewData["ddlAccount" + (i + 1)] = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { x.id, accountName = /*"[" + */x.accountNo/* + "] " +  x.accountName */ }), "id", "accountName", idCoa);
                ViewData["ddlAccount" + (i + 1)] = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { id = x.id + "-" + x.accountName, accountName = x.accountNo }), "id", "accountName", (idCoa + "-" + coaName[0].accountName));
            }
            ViewData["dataDetail"] = model.detailJournal.ToList();

            //ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { x.id, accountName = /*"[" + */x.accountNo/* + "] " +  x.accountName */ }), "id", "accountName");
            ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { id = x.id + "-" + x.accountName, accountName = x.accountNo }), "id", "accountName");
            #endregion

            return View(journalheader);
        }

        // GET: /JournalAdjustment/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            journalHeader jh = new journalHeader();
            jh.voucherDate = DateTime.Now;

            ViewBag.dataDetail = null;
            //ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { x.id, accountName = x.accountName + " [" + x.accountNo + "]" }), "id", "accountName");
            ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { id = x.id+"-"+x.accountName , accountName = x.accountName + " [" + x.accountNo + "]" }), "id", "accountName");
            return View(jh);
        }

        // POST: /JournalAdjustment/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "voucherNo,voucherDate,journalType,remark,createdDate,createdUser,modifiedDate,modifiedUser")] journalHeader journalheader, string hdnTotalRows)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            //ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { x.id, accountName = x.accountName + " [" + x.accountNo + "]" }), "id", "accountName");
            ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { id = x.id + "-" + x.accountName, accountName = x.accountName + " [" + x.accountNo + "]" }), "id", "accountName");

            #region collect detail journal
            var countChk = 0;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("ddlAccount_"))
                    countChk++;
            }

            var model = new journalHeader();
            for (int i = 1; i <= countChk; i++)
            {
                var ddlValue = Request.Form["ddlAccount_"+i];
                if (ddlValue == "")
                    break;

                var acctName = Request.Form["txtAccountName_" + i];
                var remark = Request.Form["txtRemark_" + i];
                var debit = Request.Form["txtDebit_" + i].Trim() == "" ? "0" : Request.Form["txtDebit_" + i];
                var credit = Request.Form["txtCredit_" + i].Trim() == "" ? "0" : Request.Form["txtCredit_" + i];

                string[] ddlVal = ddlValue.ToString().Split('-');
                var editor = new journalHeader.journalDetail()
                {
                    coaId = Convert.ToInt32(ddlVal[0].ToString()),
                    accountNameOther = acctName.ToString(),
                    remarks = remark.ToString(),
                    debit = Convert.ToDecimal(Convert.ToDouble(debit.ToString().Replace('.', ','))),
                    credit = Convert.ToDecimal(Convert.ToDouble(credit.ToString().Replace('.', ',')))
                };
                model.detailJournal.Add(editor);

                ViewData["ddlAccount_" + i] = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { x.id, accountName = /*"[" + */x.accountNo/* + "] " +  x.accountName */ }), "id", "accountName", ddlValue);
            }
            ViewData["dataDetail"] = model.detailJournal.ToList();
            #endregion 

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;

                try
                {
                    using(TransactionScope ts = new TransactionScope())
                    {
                        string vouchNo = "ADJ" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("d2");
                        //var voucherNoCreated = db.journalHeaders.Where(x => x.voucherNo.Contains("ADJ")).OrderByDescending(x => x.id).Select(x => x.id).ToList();
                        var voucherNoCreated = db.journalHeaders.Where(x => x.voucherNo.Contains(vouchNo)).OrderByDescending(x => x.id).Select(x => x.voucherNo).ToList();
                        if (voucherNoCreated.Count == 0)
                        {
                            journalheader.voucherNo = "ADJ" + DateTime.Now.Year + DateTime.Now.Month.ToString("d2") + "0001";
                        }
                        else
                        {
                            journalheader.voucherNo = "ADJ" + DateTime.Now.Year + DateTime.Now.Month.ToString("d2") + (Convert.ToInt32(voucherNoCreated[0].Substring((voucherNoCreated[0].Length - 4))) + 1).ToString().PadLeft(4, '0');
                        }

                        journalheader.createdDate = DateTime.Now;
                        journalheader.createdUser = "";
                        db.journalHeaders.Add(journalheader);
                        db.SaveChanges();

                        if (journalheader.id > 0)//var groupIdH = db.UserGroupHs.OrderByDescending(x => x.id).Select(x => x.id).First(); <--cara lain
                        {
                            #region insertDetail
                            for (int i = 1; i <= countChk; i++)
                            {
                                var ddlValue = Request.Form["ddlAccount_" + i];
                                if (ddlValue == "" || ddlValue == null)
                                    break;

                                var acctName = Request.Form["txtAccountName_" + i];
                                var remark = Request.Form["txtRemark_" + i];
                                var debit = Request.Form["txtDebit_" + i].Trim() == "" ? "0" : Request.Form["txtDebit_" + i];
                                var credit = Request.Form["txtCredit_" + i].Trim() == "" ? "0" : Request.Form["txtCredit_" + i];

                                
                                string[] ddlVal = ddlValue.ToString().Split('-');
                                db.journalDetails.Add(new journalHeader.journalDetail()
                                {
                                    voucherNo = journalheader.voucherNo,
                                    //coaId = Convert.ToInt32(ddlValue.ToString()),
                                    coaId = Convert.ToInt32(ddlVal[0].ToString()),
                                    accountNameOther = acctName.ToString(),
                                    remarks = remark.ToString(),
                                    debit = Convert.ToDecimal(Convert.ToDouble(debit.ToString().Replace('.', ','))),
                                    credit = Convert.ToDecimal(Convert.ToDouble(credit.ToString().Replace('.', ',')))//Convert.ToInt32(credit.ToString())
                                });
                            }
                            db.SaveChanges();
                            #endregion
                        }

                        ts.Complete();
                    }
                    return RedirectToAction("Index");
                }
                catch(Exception exc)
                {
                    string a = exc.Message;
                }
            }

            return View(journalheader);
        }

        // GET: /JournalAdjustment/Edit/5
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
            journalHeader journalheader = db.journalHeaders.Find(id);
            if (journalheader == null)
            {
                return HttpNotFound();
            }

            #region getDataDetail
            var data = db.journalDetails.Where(x => x.voucherNo == id).ToList();
            var model = new journalHeader();
            for (int i = 0; i < data.Count(); i++)
            {
                int idCoa = Convert.ToInt32(data[i].coaId.ToString());
                string othrAccountName = data[i].accountNameOther.ToString();
                string remak = data[i].remarks.ToString();
                decimal debt = Convert.ToDecimal(data[i].debit.ToString());
                decimal credt = Convert.ToDecimal(data[i].credit.ToString());
                var coaName = db.chartOfAccounts.Where(x => x.id == idCoa).ToList();

                var editor = new journalHeader.journalDetail()
                {
                    coaId = idCoa,
                    accountNameOther = coaName[0].accountName/*othrAccountName*/,
                    remarks = remak,
                    debit = debt,
                    credit = credt
                };
                model.detailJournal.Add(editor);

                //ViewData["ddlAccount" + (i + 1)] = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { x.id, accountName = /*"[" + */x.accountNo/* + "] " +  x.accountName */ }), "id", "accountName", idCoa);
                ViewData["ddlAccount" + (i + 1)] = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { id = x.id + "-" + x.accountName, accountName = x.accountNo }), "id", "accountName", (idCoa+"-"+coaName[0].accountName));
            }
            ViewData["dataDetail"] = model.detailJournal.ToList();

            //ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { x.id, accountName = x.accountName + " [" + x.accountNo + "]" }), "id", "accountName");
            ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { id = x.id + "-" + x.accountName, accountName = x.accountName + " [" + x.accountNo + "]" }), "id", "accountName");
            #endregion

            return View(journalheader);
        }

        // POST: /JournalAdjustment/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "voucherNo,voucherDate,journalType,remark,createdDate,createdUser,modifiedDate,modifiedUser")] journalHeader journalheader, string hdnTotalRows)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            #region collect detail journal
            var countChk = 0;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("ddlAccount_"))
                    countChk++;
            }

            var model = new journalHeader();
            for (int i = 1; i <= countChk; i++)
            {
                var ddlValue = Request.Form["ddlAccount_" + i];
                if (ddlValue == "")
                    break;

                var acctName = Request.Form["txtAccountName_" + i];
                var remark = Request.Form["txtRemark_" + i];
                var debit = Request.Form["txtDebit_" + i].Trim() == "" ? "0" : Request.Form["txtDebit_" + i];
                var credit = Request.Form["txtCredit_" + i].Trim() == "" ? "0" : Request.Form["txtCredit_" + i];

                string[] ddlVal = ddlValue.ToString().Split('-');
                var editor = new journalHeader.journalDetail()
                {
                    coaId = Convert.ToInt32(ddlVal[0].ToString()),
                    accountNameOther = acctName.ToString(),
                    remarks = remark.ToString(),
                    debit = Convert.ToDecimal(Convert.ToDouble(debit.ToString().Replace('.', ','))),
                    credit = Convert.ToDecimal(Convert.ToDouble(credit.ToString().Replace('.', ',')))
                };
                model.detailJournal.Add(editor);

                ViewData["ddlAccount_" + i] = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { x.id, accountName = /*"[" + */x.accountNo/* + "] " +  x.accountName */ }), "id", "accountName", ddlValue);
            }
            ViewData["dataDetail"] = model.detailJournal.ToList();
            #endregion

            ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { id = x.id + "-" + x.accountName, accountName = x.accountName + " [" + x.accountNo + "]" }), "id", "accountName");
            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        var ids = db.journalHeaders.AsNoTracking().Where(x => x.voucherNo == journalheader.voucherNo).ToList();
                        long id = ids[0].id;

                        journalheader.id = id;
                        journalheader.modifiedDate = DateTime.Now;
                        journalheader.modifiedUser = lvm.userID;
                        db.Entry(journalheader).State = EntityState.Modified;
                        //db.SaveChanges();

                        #region insertDetail
                        bool runFirst = true;
                        for (int i = 1; i <= countChk; i++)
                        {
                            if (runFirst == true)
                            {                                
                                db.journalDetails.RemoveRange(db.journalDetails.Where(x => x.voucherNo == journalheader.voucherNo));
                                //db.SaveChanges();
                                runFirst = false;
                            }

                            var ddlValue = Request.Form["ddlAccount_" + i];
                            if (ddlValue == "" || ddlValue == null)
                                break;

                            var acctName = Request.Form["txtAccountName_" + i];
                            var remark = Request.Form["txtRemark_" + i];
                            var debit = Request.Form["txtDebit_" + i].Trim() == "" ? "0" : Request.Form["txtDebit_" + i];
                            var credit = Request.Form["txtCredit_" + i].Trim() == "" ? "0" : Request.Form["txtCredit_" + i];
                            
                            string[] ddlVal = ddlValue.ToString().Split('-');
                            db.journalDetails.Add(new journalHeader.journalDetail()
                            {
                                voucherNo = journalheader.voucherNo,
                                //coaId = Convert.ToInt32(ddlValue.ToString()),
                                coaId = Convert.ToInt32(ddlVal[0].ToString()),
                                accountNameOther = acctName.ToString(),
                                remarks = remark.ToString(),
                                debit = Convert.ToDecimal(Convert.ToDouble(debit.ToString().Replace('.', ','))),
                                credit = Convert.ToDecimal(Convert.ToDouble(credit.ToString().Replace('.', ',')))
                            });
                        }
                        #endregion

                        db.SaveChanges();
                        ts.Complete();
                    }
                    return RedirectToAction("Index");
                }
                catch(Exception exc)
                {

                }
            }
            return View(journalheader);
        }

        // GET: /JournalAdjustment/Delete/5
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
            journalHeader journalheader = db.journalHeaders.Find(id);
            if (journalheader == null)
            {
                return HttpNotFound();
            }

            #region getDataDetail
            var data = db.journalDetails.Where(x => x.voucherNo == id).ToList();
            var model = new journalHeader();
            for (int i = 0; i < data.Count(); i++)
            {
                int idCoa = Convert.ToInt32(data[i].coaId.ToString());
                string othrAccountName = data[i].accountNameOther.ToString();
                string remak = data[i].remarks.ToString();
                decimal debt = Convert.ToDecimal(data[i].debit.ToString());
                decimal credt = Convert.ToDecimal(data[i].credit.ToString());

                var coaName = db.chartOfAccounts.Where(x => x.id == idCoa).ToList();
                var editor = new journalHeader.journalDetail()
                {
                    coaId = idCoa,
                    accountNameOther = coaName[0].accountName/*othrAccountName*/,
                    remarks = remak,
                    debit = debt,
                    credit = credt
                };
                model.detailJournal.Add(editor);

                //ViewData["ddlAccount" + (i + 1)] = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { x.id, accountName = /*"[" + */x.accountNo/* + "] " +  x.accountName */ }), "id", "accountName", idCoa);
                ViewData["ddlAccount" + (i + 1)] = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { id = x.id + "-" + x.accountName, accountName = x.accountNo }), "id", "accountName", (idCoa + "-" + coaName[0].accountName));
            }
            ViewData["dataDetail"] = model.detailJournal.ToList();

            //ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { x.id, accountName = /*"[" + */x.accountNo/* + "] " +  x.accountName */ }), "id", "accountName");
            ViewBag.ddlAccount = new SelectList(db.chartOfAccounts.Where(x => x.levelID == 3 && x.accountNo != "").Select(x => new { id = x.id + "-" + x.accountName, accountName = x.accountNo }), "id", "accountName");
            #endregion

            return View(journalheader);
        }

        // POST: /JournalAdjustment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            journalHeader journalheader = db.journalHeaders.Find(id);
            db.journalHeaders.Remove(journalheader);
            db.journalDetails.RemoveRange(db.journalDetails.Where(x => x.voucherNo == journalheader.voucherNo));
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
