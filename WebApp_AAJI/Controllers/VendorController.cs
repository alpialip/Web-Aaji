using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class VendorController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        private AccountController acm = new AccountController();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        CommonController ccm = new CommonController();

        // GET: /Vendor/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            return View(db.vendors.ToList());
        }

        // GET: /Vendor/Details/5
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
            vendor vendor = db.vendors.Find(id);
            if (vendor == null)
            {
                return HttpNotFound();
            }

            loadDataDetail(id);
            return View(vendor);
        }

        // GET: /Vendor/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            ViewBag.Bank = ccm.ddlBank(string.Empty);
            return View();
        }

        // POST: /Vendor/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(vendor vendor)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                vendor.createdUser = lvm.userID;
                vendor.createdDate = DateTime.Now;
                db.vendors.Add(vendor);
                db.SaveChanges();
                return RedirectToAction("Edit", "Vendor", new { id = vendor.id });
            }

            return View(vendor);
        }

        // GET: /Vendor/Edit/5
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
            vendor vendor = db.vendors.Find(id);
            if (vendor == null)
            {
                return HttpNotFound();
            }

            loadDataDetail(id);
            return View(vendor);
        }

        // POST: /Vendor/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(vendor vendor)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            #region preparing collect Data
            var countChkProduct = 0;
            List<string> listChkProductID = new List<string>();
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("chk_"))
                {
                    countChkProduct++;
                    string[] chkVal = Request.Form.AllKeys.ToList()[i].ToString().Split('_');
                    if (!listChkProductID.Contains(chkVal[1].ToString()))
                    {
                        listChkProductID.Add(chkVal[1].ToString());
                    }
                }
            }
            #endregion

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                vendor.modifiedUser = lvm.userID;
                vendor.modifiedDate = DateTime.Now;
                db.Entry(vendor).State = EntityState.Modified;

                if(listChkProductID.Count() > 0)
                {
                    db.transVendorProducts.RemoveRange(db.transVendorProducts.Where(x=>x.id_vendor == vendor.id));
                    List<transVendorProduct> tvp = new List<transVendorProduct>();
                    for (int i = 0; i < listChkProductID.Count(); i++)
                    {
                        var editor = new transVendorProduct()
                        {
                            id_vendor = vendor.id,
                            productID = int.Parse(listChkProductID[i]),
                            modifiedDate = DateTime.Now,
                            modifiedUser = lvm.userID
                        };
                        db.transVendorProducts.Add(editor);
                    }
                        
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(vendor);
        }

        // GET: /Vendor/Delete/5
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
            vendor vendor = db.vendors.Find(id);
            if (vendor == null)
            {
                return HttpNotFound();
            }

            loadDataDetail(id);
            return View(vendor);
        }

        // POST: /Vendor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            vendor vendor = db.vendors.Find(id);
            db.vendors.Remove(vendor);
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

        public void loadDataDetail(int? id)
        {
            ViewBag.masterProduct = db.products.AsNoTracking().ToList();
            ViewBag.Bank = ccm.ddlBank(string.Empty);

            if (id > 0 && id != null)
            {
                var a = db.vendors.AsNoTracking().Where(x=>x.id==id).ToList();
                ViewBag.Bank = ccm.ddlBank(a[0].bankRekening.ToString());

                var y = a[0].bankRekening;
                var b = db.banks.AsNoTracking().Where(x => x.bankID == y).ToList();
                if (b.Count > 0)
                    ViewBag.BankName = b[0].bankName;

                var transVendorProduct = db.transVendorProducts.AsNoTracking().Where(x => x.id_vendor == id).ToList();
                if (transVendorProduct.Count > 0)
                    ViewBag.TransVendorProduct = transVendorProduct;
            }
        }
    }
}
