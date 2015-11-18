using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class ClosingController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private CommonController ccm = new CommonController();
        private ConnectionController cc = new ConnectionController();

        [HttpGet]
        public async Task<ActionResult> posting(string month, string year)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (int.Parse(month) < 10)
                month = "0" + month;

            bool isSuccess = cc.posting(month, year, lvm.userID);
            ViewBag.Posting = isSuccess;

            return PartialView("_PartialPagePosting");
        }

        //
        // GET: /Closing/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.Month = ccm.ddlMonth(string.Empty);
            return View();
        }

        //
        // GET: /Closing/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Closing/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Closing/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Closing/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Closing/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Closing/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Closing/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
