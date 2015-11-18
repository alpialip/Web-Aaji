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
using System.Transactions;

namespace WebApp_AAJI.Controllers
{
    public class MenuController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();

        // GET: /Menu/
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            getParentMenuDesc(null);

            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name_" : "";
            ViewBag.ParentSortParm = sortOrder == "Parent" ? "Parent_" : "Parent";
            ViewBag.isParentSortParm = sortOrder == "isParent" ? "isParent_" : "isParent";
            ViewBag.isActiveSortParm = sortOrder == "isActive" ? "isActive_" : "isActive";

            if (searchString != null)
                page = 1;
            else
                searchString = currentFilter;
            ViewBag.CurrentFilter = searchString;

            var menus = from s in db.Menus 
			   //where s.menuID != 30 && s.menuParent != 30 
			   select s;

            if (!String.IsNullOrEmpty(searchString))
            {
                menus = menus.Where(s => s.menuName.Contains(searchString)
                                       || s.menuDescription.Contains(searchString));
            }

            switch (sortOrder)
            {
                case "Name_":
                    menus = menus.OrderByDescending(s => s.menuName);
                    break;
                case "Parent":
                    menus = menus.OrderBy(s => new {s.menuParent, s.menuName });
                    break;
                case "Parent_":
                    menus = menus.OrderByDescending(s => s.menuParent);
                    break;
                case "isParent":
                    menus = menus.OrderBy(s => new { s.menuIsParent, s.menuName });
                    break;
                case "isParent_":
                    menus = menus.OrderByDescending(s => s.menuIsParent);
                    break;
                case "isActive":
                    menus = menus.OrderBy(s => new {s.menuIsActive, s.menuName });
                    break;
                case "isActive_":
                    menus = menus.OrderByDescending(s => s.menuIsActive);
                    break;
                default:
                    menus = menus.OrderBy(s => s.menuName);
                    break;
            }

            int pageSize = 10;
            int pageNumber = (page ?? 1);
            return View(menus.ToPagedList(pageNumber, pageSize));
        }

        // GET: /Menu/Details/5
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
            menu menu = db.Menus.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        // GET: /Menu/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.table_name = new SelectList(
						      (db.Menus.Where(m => m.menuIsParent == true && m.menuIsActive == true) // && m.menuID != 30)//menu personal jangan dikeluarin, klo mo input dari belakang aja
                                                .Select(m => new { m.menuID, m.menuName }).OrderBy(m => m.menuName)
                                                .Union(db.Menus.Where(m => m.menuIsParent == false && m.menuIsActive == true)
                                                .Select(m => new { m.menuID, menuName = "- " + m.menuName }).OrderBy(x => x.menuName))
                                                )
                                                , "menuID", "menuName");
            return View();
        }

        // POST: /Menu/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(menu menu)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                menu.createdDate = DateTime.Now;
                menu.createdUser = lvm.userID;
                db.Menus.Add(menu);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(menu);
        }

        // GET: /Menu/Edit/5
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
            menu menu = db.Menus.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            TempData["isParentMenu"] = menu.menuIsParent;
            ViewBag.table_name = new SelectList((db.Menus.Where(m => m.menuIsParent == true && m.menuIsActive == true ) //&& m.menuID != 30)//menu personal jangan dikeluarin, klo mo input dari belakang aja
                                                .Select(m => new { m.menuID, m.menuName }).OrderBy(m => m.menuName)
                                                .Union(db.Menus.Where(m => m.menuIsParent == false && m.menuIsActive == true)
                                                .Select(m => new { m.menuID, menuName = "- " + m.menuName }).OrderBy(x => x.menuName))), "menuID", "menuName",menu.menuID);
            return View(menu);
        }

        // POST: /Menu/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(menu menu)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                try
                {
                    using(TransactionScope ts = new TransactionScope())
                    {
                        menu.modifiedDate = DateTime.Now;
                        menu.modifiedUser = lvm.userID;
                        db.Entry(menu).State = EntityState.Modified;
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
            return View(menu);
        }

        // GET: /Menu/Delete/5
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
            menu menu = db.Menus.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            getParentMenuDesc(id);
            return View(menu);
        }

        // POST: /Menu/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            menu menu = db.Menus.Find(id);
            db.Menus.Remove(menu);
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

        protected void getParentMenuDesc(int? id)
        {
            if(id == null)
                ViewData["parentMenuDesc"] = db.Menus.Where(m => m.menuIsActive == true && m.menuIsParent == true)// && m.menuID != 30)
			  .ToList();
            else
                ViewData["parentMenuDesc"] = db.Menus.Where(m => m.menuIsActive == true && m.menuIsParent == true) // && m.menuID != 30)
                    .Join(db.Menus.Where(m => m.menuIsActive == true && m.menuIsParent == false && m.menuID == id ) //&& m.menuParent != 30)
		      , a => a.menuID, b => b.menuParent, (a, b) => a).ToList();

        }
    }
}
