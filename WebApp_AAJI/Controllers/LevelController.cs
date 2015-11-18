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
    public class LevelController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        private AccountController acm = new AccountController();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();

        // GET: /Level/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            return View(db.levels.ToList());
        }

        // GET: /Level/Details/5
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
            level level = db.levels.Find(id);
            if (level == null)
            {
                return HttpNotFound();
            }

            loadDataTambahan(id);
            return View(level);
        }

        // GET: /Level/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            return View();
        }

        // POST: /Level/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(level level)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                level.createdUser = lvm.userID;
                level.createdDate = DateTime.Now;
                db.levels.Add(level);
                db.SaveChanges();

                if (level.levelID > 0)
                {
                    //harusnya kesini
                    return RedirectToAction("Edit", "Level", new { id = level.levelID });
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }

            return View(level);
        }

        // GET: /Level/Edit/5
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
            level level = db.levels.Find(id);
            if (level == null)
            {
                return HttpNotFound();
            }

            loadDataTambahan(id);
            return View(level);
        }

        // POST: /Level/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(level level)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            #region collect (penghasilan)
            int countDetail = 0;
            List<string> listElementIdPenghasilan = new List<string>();
            List<string> listElementIdPengurangPenghasilan = new List<string>();

            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("penghasilan_"))
                {
                    if (!listElementIdPenghasilan.Contains(Request.Form.AllKeys[i].ToString()))
                        listElementIdPenghasilan.Add(Request.Form.AllKeys[i].ToString());
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("pengurangPenghasilan_"))
                {
                    if (!listElementIdPengurangPenghasilan.Contains(Request.Form.AllKeys[i].ToString()))
                        listElementIdPengurangPenghasilan.Add(Request.Form.AllKeys[i].ToString());
                }
            }
            #endregion

            loadDataTambahan(level.levelID);
            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                level.modifiedUser = lvm.userID;
                level.modifiedDate = DateTime.Now;
                db.Entry(level).State = EntityState.Modified;
                
                #region collect penghasilan
                #region penghasilan
                db.transMasterPenghasilans.RemoveRange(db.transMasterPenghasilans.Where(x => x.levelID == level.levelID));
                for (int i = 0; i < listElementIdPenghasilan.Count(); i++)
                {
                    string elementId = listElementIdPenghasilan[i];
                    string[] arrayId = elementId.Split('_');
                    decimal amount = decimal.Parse(Request.Form[elementId].ToString());
                    int idPenghasilan = int.Parse(arrayId[1].ToString());

                    transMasterPenghasilan tPenghasilan = new transMasterPenghasilan();
                    tPenghasilan.levelID = level.levelID;
                    tPenghasilan.idPenghasilan = idPenghasilan;
                    tPenghasilan.amount = amount;
                    tPenghasilan.modifiedDate = DateTime.Now;
                    tPenghasilan.modifiedUser = lvm.userID;
                    db.transMasterPenghasilans.Add(tPenghasilan);
                }
                #endregion

                #region Pengurang Penghasilan
                db.transMasterPengurangPenghasilans.RemoveRange(db.transMasterPengurangPenghasilans.Where(x => x.levelID == level.levelID));
                for (int i = 0; i < listElementIdPengurangPenghasilan.Count(); i++)
                {
                    string elementId = listElementIdPengurangPenghasilan[i];
                    string[] arrayId = elementId.Split('_');
                    decimal amount = decimal.Parse(Request.Form[elementId].ToString());
                    int idPengurangPenghasilan = int.Parse(arrayId[1].ToString());

                    transMasterPengurangPenghasilan tPengurangPenghasilan = new transMasterPengurangPenghasilan();
                    tPengurangPenghasilan.levelID = level.levelID;
                    tPengurangPenghasilan.idPengurangPenghasilan = idPengurangPenghasilan;
                    tPengurangPenghasilan.amount = amount;
                    tPengurangPenghasilan.modifiedDate = DateTime.Now;
                    tPengurangPenghasilan.modifiedUser = lvm.userID;
                    db.transMasterPengurangPenghasilans.Add(tPengurangPenghasilan);
                }
                #endregion
                db.SaveChanges(); 
                #endregion

                return RedirectToAction("Index");
            }
            return View(level);
        }

        // GET: /Level/Delete/5
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
            level level = db.levels.Find(id);
            if (level == null)
            {
                return HttpNotFound();
            }

            loadDataTambahan(id);
            return View(level);
        }

        // POST: /Level/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
            
            level level = db.levels.Find(id);
            db.levels.Remove(level);
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

        public void loadDataTambahan(int? levelID)
        {
            ViewBag.masterPenghasilan = db.penghasilans.AsNoTracking().ToList();
            ViewBag.masterPengurangPenghasilan = db.pengurangPenghasilans.AsNoTracking().ToList();

            if(levelID > 0 && levelID != null)
            {                
                #region Penghasilan & Pengurang Penghasilan
                var transMasterPenghasilan = db.transMasterPenghasilans.AsNoTracking().Where(x => x.levelID == levelID).ToList();
                if (transMasterPenghasilan.Count > 0)
                    ViewBag.penghasilan = transMasterPenghasilan;

                var transMasterPengurangPenghasilan = db.transMasterPengurangPenghasilans.AsNoTracking().Where(x => x.levelID == levelID).ToList();
                if (transMasterPengurangPenghasilan.Count > 0)
                    ViewBag.pengurangPenghasilan = transMasterPengurangPenghasilan;
                #endregion
            }
        }
    }
}
