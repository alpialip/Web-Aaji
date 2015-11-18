using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class ChartOfAccountController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        private ConnectionController cd = new ConnectionController();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();

        // GET: /ChartOfAccount/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            var level1 = db.chartOfAccounts.Where(l1 =>l1.levelID == 1 && l1.parentCOAId == 0).Select(x=>new{x.id, x.accountNo, x.accountName}).ToList();
            var level2 = db.chartOfAccounts.Where(l2 => l2.levelID == 2).Select(x => new { x.id, x.accountNo, x.accountName, x.parentCOAId }).ToList();
            var level3 = db.chartOfAccounts.Where(l3 => l3.levelID == 3).Select(x => new { x.id, x.accountNo, x.accountName, x.parentCOAId }).ToList();
            ViewBag.treeCOA = string.Empty;
            string ltrTree = string.Empty;

            ltrTree = "<script type='text/javascript'>d = new dTree('d');d.add(0,-1,'Chart Of Account');";
            foreach(var l1 in level1.ToList())
            {
                ltrTree += "d.add(" + l1.id + ",0,'[" + l1.accountNo + "] " + l1.accountName + "','');";
                foreach(var l2 in level2.ToList())
                {
                    if(l2.parentCOAId == l1.id)
                    { 
                        ltrTree += "d.add("+l2.id+","+l1.id+",'["+l2.accountNo+"] "+l2.accountName+"','');";
                        foreach(var l3 in level3.ToList())
                        {
                            if(l3.parentCOAId == l2.id)
                            {
                                ltrTree += "d.add(" + l3.id + "," + l2.id + ",'[" + l3.accountNo + "] " + l3.accountName + "','');";
                            }
                        }
                    }
                }
            }
            ltrTree += "document.write(d);</script>";
            ViewBag.treeCOA = ltrTree;

            ViewBag.Checked = 0;
            return View(db.chartOfAccounts.ToList());
        }

        // GET: /ChartOfAccount/Details/5
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
            chartOfAccount chartofaccount = db.chartOfAccounts.Find(id);
            if (chartofaccount == null)
            {
                return HttpNotFound();
            }
            return View(chartofaccount);
        }

        // GET: /ChartOfAccount/Create
        public ActionResult Create(int? idLevel)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.parentCOAId = null;// new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 0), "id", "accountName");
            if (idLevel == 2)
                ViewBag.parentCOAId = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 1), "id", "accountName");
            else if (idLevel == 3)
                ViewBag.parentCOAId = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 2), "id", "accountName");

            return View();
        }

        // POST: /ChartOfAccount/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(/*[Bind(Include="accountNo,accountName,createdDate,createdUser,modifiedDate,modifiedUser")]*/ chartOfAccount chartofaccount, int? idLevel)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.parentCOAId = null;
            if (idLevel == 2)
                ViewBag.parentCOAId = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 1).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");
            else if (idLevel == 3)
                ViewBag.parentCOAId = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 2).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;

                for (int i = 0; i < Request.Form.Count; i++)
                {
                    if (Request.Form.AllKeys.ToList()[i].Contains("parentCOAId"))
                    {
                        chartofaccount.parentCOAId = Convert.ToInt32(Request.Form["parentCOAId"].ToString());
                        break;
                    }
                }

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        string sql = string.Empty;
                        if (idLevel == 1)
                        {
                            sql = "SELECT RIGHT('0'+CAST(ISNULL(MAX(accountNo),0)+1 AS VARCHAR(1)),1) ";
                            sql += "FROM [dbo].[ChartOfAccounts] WHERE levelID = " + idLevel + " AND parentCOAId = 0";
                        }
                        else if (idLevel == 2)
                        {
                            sql = "SELECT CAST((SELECT accountno FROM [dbo].[ChartOfAccounts] WHERE levelID = 1 and id = " + chartofaccount.parentCOAId + ") AS VARCHAR) ";
                            sql += "+RIGHT('000',3-LEN(postfix))+CAST(postfix AS VARCHAR) ";
                            sql += "FROM(SELECT CAST(COUNT(*) AS VARCHAR(3)) AS postfix ";
                            sql += "FROM [dbo].[ChartOfAccounts] WHERE levelID = " + idLevel + " and parentCOAId = " + chartofaccount.parentCOAId + " )Z ";
                        }
                        else if (idLevel == 3)
                        {
                            sql = "SELECT CAST((SELECT accountno FROM [dbo].[ChartOfAccounts] WHERE levelID = 2 and id = " + chartofaccount.parentCOAId + ") AS VARCHAR) ";
                            sql += "+RIGHT('0000',4-LEN(postfix))+CAST(postfix AS VARCHAR) ";
                            sql += "FROM(SELECT CAST(COUNT(*) AS VARCHAR(4)) AS postfix ";
                            sql += "FROM [dbo].[ChartOfAccounts] WHERE levelID = " + idLevel + " AND parentCOAId = " + chartofaccount.parentCOAId + " )Z ";
                        }

                        string coaNo = cd.executeScalar(sql).ToString();
                        chartofaccount.levelID = Convert.ToInt32(idLevel);
                        chartofaccount.accountNo = coaNo;
                        chartofaccount.createdDate = DateTime.Now;
                        chartofaccount.createdUser = lvm.userID;

                        db.chartOfAccounts.Add(chartofaccount);
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

            return View(chartofaccount);
        }

        // GET: /ChartOfAccount/Edit/5
        public ActionResult Edit(/*int? id*/)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.parentCOAId0 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 0).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");
            ViewBag.parentCOAId1 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 1).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");
            ViewBag.parentCOAId2 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 2).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");
            ViewBag.parentCOAId3 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 3).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");

            return View();
        }

        // POST: /ChartOfAccount/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(/*[Bind(Include="accountNo,accountName,createdDate,createdUser,modifiedDate,modifiedUser")]*/ chartOfAccount chartofaccount)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            #region unused
            //if (chartofaccount.levelID == 1)
            //{
            //    ViewBag.parentCOAId = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 1).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName", chartofaccount.parentCOAId == null ? 0 : chartofaccount.parentCOAId);
            //    ViewBag.parentCOAId1 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 1).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");
            //}
            //else if (chartofaccount.levelID == 2)
            //{
            //    ViewBag.parentCOAId = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 2).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName", chartofaccount.parentCOAId == null ? 0 : chartofaccount.parentCOAId);
            //    ViewBag.parentCOAId2 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 2).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");
            //}
            //else if (chartofaccount.levelID == 3)
            //{ 
            //    ViewBag.parentCOAId = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 3).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName", chartofaccount.parentCOAId == null ? 0 : chartofaccount.parentCOAId);
            //    ViewBag.parentCOAId3 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 3).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");
            //}
            //if(chartofaccount.parentCOAId != null)
            //{ 
            //    var accountName = db.chartOfAccounts.Where(x => x.id == (int)chartofaccount.parentCOAId).Select(x => x.accountName).ToList();
            //    ViewBag.accountName = accountName[0].ToString();
            //    chartofaccount.accountName = accountName[0].ToString();
            //}
            #endregion

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            ViewBag.isEdit = true;
            ViewBag.parentCOAId0 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 0).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");
            ViewBag.parentCOAId1 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 1).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName", chartofaccount.parentCOAId);
            ViewBag.parentCOAId2 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 2).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName", chartofaccount.parentCOAId);
            ViewBag.parentCOAId3 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 3).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName", chartofaccount.parentCOAId);

            var accountNo = db.chartOfAccounts.Where(x=>x.id==chartofaccount.parentCOAId).Select(x=>x.accountNo).ToList();
            List<chartOfAccount> coat = db.chartOfAccounts.AsNoTracking().Where(x=>x.levelID == chartofaccount.levelID && x.id==chartofaccount.parentCOAId).ToList();
            foreach(chartOfAccount a in coat)
            { 
                chartofaccount.accountName = chartofaccount.accountName;
                chartofaccount.accountNo = a.accountNo;
                chartofaccount.createdDate = a.createdDate;
                chartofaccount.createdUser = a.createdUser;
                chartofaccount.id = a.id;
                chartofaccount.levelID = a.levelID;
                chartofaccount.modifiedDate = DateTime.Now;
                chartofaccount.modifiedUser = lvm.userID;
                chartofaccount.parentCOAId = a.parentCOAId;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    using(TransactionScope ts = new TransactionScope())
                    {
                        db.Entry(chartofaccount).State = EntityState.Modified;
                        db.SaveChanges();
                        ts.Complete();
                    }
                    return RedirectToAction("Index");

                }
                catch(Exception exc)
                {
                    string a = exc.Message;
                }
            }
            return View(chartofaccount);
        }

        // GET: /ChartOfAccount/Delete/5
        public ActionResult Delete(/*string id*/)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.parentCOAId0 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 0).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");
            ViewBag.parentCOAId1 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 1).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");
            ViewBag.parentCOAId2 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 2).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");
            ViewBag.parentCOAId3 = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 3).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");

            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            //chartOfAccount chartofaccount = db.chartOfAccounts.Find(id);
            //if (chartofaccount == null)
            //{
            //    return HttpNotFound();
            //}
            return View();
        }

        // POST: /ChartOfAccount/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(chartOfAccount coas)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            var a = db.chartOfAccounts.Where(x=>x.id==coas.id && x.levelID==coas.levelID).ToList();

            chartOfAccount chartofaccount = new chartOfAccount()
            {
                accountName = a[0].accountName,
                accountNo = a[0].accountNo,
                createdDate = a[0].createdDate,
                createdUser = a[0].createdUser,
                id = a[0].id,
                levelID = a[0].levelID,
                modifiedDate = a[0].modifiedDate,
                modifiedUser = a[0].modifiedUser,
                parentCOAId = a[0].parentCOAId
            };

            ViewBag.parentCOAId0 = new SelectList(db.chartOfAccounts.AsNoTracking().Where(coa => coa.levelID == 0).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName");
            ViewBag.parentCOAId1 = new SelectList(db.chartOfAccounts.AsNoTracking().Where(coa => coa.levelID == 1).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName", chartofaccount.parentCOAId);
            ViewBag.parentCOAId2 = new SelectList(db.chartOfAccounts.AsNoTracking().Where(coa => coa.levelID == 2).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName", chartofaccount.parentCOAId);
            ViewBag.parentCOAId3 = new SelectList(db.chartOfAccounts.AsNoTracking().Where(coa => coa.levelID == 3).Select(x => new { x.id, accountName = "[" + x.accountNo + "] " + x.accountName }), "id", "accountName", chartofaccount.parentCOAId);

            var COAisNotUsedinJournal = db.journalDetails.AsNoTracking().Where(x => x.coaId == chartofaccount.id).ToList();

            if(COAisNotUsedinJournal.Count() > 0)
            {

            }
            else
            {
                db.chartOfAccounts.RemoveRange(db.chartOfAccounts.Where(x => x.id == coas.id && x.levelID == coas.levelID));
                db.SaveChanges();
            }
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

        public JsonResult DistrictList(int idLevel)
        {
            ViewBag.ddlTreeCOA = null;
            if (idLevel == 2)
            {
                ViewBag.parentCOAId = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 1), "parentCOAId", "parentCOAName");
            }
            else if (idLevel == 3)
            { 
                ViewBag.parentCOAId = new SelectList(db.chartOfAccounts.Where(coa => coa.levelID == 2), "parentCOAId", "parentCOAName");
            }

            //var district = from s in District.GetDistrict()
            //               where s.StateName == Id
            //               select s;


            //return Json(new SelectList(district.ToArray(), "StateName", "DistrictName"), JsonRequestBehavior.AllowGet);
            return Json(ViewBag.parentCOAId, JsonRequestBehavior.AllowGet);
        }
    }
}
