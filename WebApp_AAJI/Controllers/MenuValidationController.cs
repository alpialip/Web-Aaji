using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class MenuValidationController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private CommonController cm = new CommonController();

        // GET: /MenuValidation/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewData["userName"] = db.Users.Where(u => u.isActive == true).ToList();
            ViewData["menuName"] = db.Menus.Where(m => m.menuIsActive == true).ToList();
            return View(db.MenuValidations.ToList());
        }

        // GET: /MenuValidation/Details/5
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
            menuValidation menuvalidation = db.MenuValidations.Find(id);
            if (menuvalidation == null)
            {
                return HttpNotFound();
            }

            loadData(menuvalidation);
            return View(menuvalidation);
        }

        // GET: /MenuValidation/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.MenuParent = cm.ddlMenuParent(string.Empty);
            ViewBag.menuExclusive = cm.ddlMenuValidationExclusive("");
            var menuAssign = db.Menus.Where(m => m.menuIsActive == true && m.menuLink != null) // && m.menuParent != 30 && m.menuID != 30)
		     .ToList();
            ViewBag.userID = new SelectList(db.Users.Where(m => m.isActive == true && m.userID != "admin").ToList(), "userID", "userName");

            var modelMenu = new menuValidation();
            for(int i=0;i<menuAssign.Count;i++)
            {
                var editor = new menuValidation.SelectMenuAuthorize()
                {
                    Id = Convert.ToInt32(menuAssign[i].menuID.ToString()),
                    menuSelected = false,
                    Name = menuAssign[i].menuName.ToString()
                };
                modelMenu.MenuAuth.Add(editor);
            }
            ViewData["menuGeneral"] = modelMenu.MenuAuth.OrderBy(x=>x.Name).ToList();

            #region checkBoxUser
            var modelUser = new user();
            var tempUser = db.Users.Where(x => x.userID != "admin" && x.isActive == true).ToList();
            foreach (var a in tempUser)
            {
                var editor = new user.SelectActionUser()
                {
                    userId = a.userID,
                    userSelected = false,
                    userName = a.userName
                };
                modelUser.userCheckBox.Add(editor);
            }
            ViewData["userAssign"] = modelUser.userCheckBox.OrderBy(x => x.userName).ToList();
            #endregion

            //var ax = new SelectList(db.Menus.Where(m => m.menuIsActive == true && m.menuLink != null), "menuID", "menuName");
            //List<SelectListItem> categoryList = new List<SelectListItem>();
            //foreach(var a in ax.ToList())
            //{
            //    categoryList.Add(new SelectListItem { Text = a.Text, Value = a.Value });
            //}
            //model.CategoryItems = categoryList;
            
            return View();
        }

        // POST: /MenuValidation/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(menuValidation menuvalidation)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.menuExclusive = cm.ddlMenuValidationExclusive("");
            var menuAssign = db.Menus.Where(m => m.menuIsActive == true && m.menuLink != null).ToList();
            ViewBag.userID = new SelectList(db.Users.Where(m => m.isActive == true && m.userID != "admin").ToList(), "userID", "userName");

            #region preparing collect Data
            var countChkMenu = 0;
            var countChkUsr = 0;
            int cbMenu = 0;
            string valAccess = string.Empty;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("menuIDGeneral"))
                {
                    countChkMenu++;
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("rbIsGeneral"))
                {
                    menuvalidation.validationTypeIsGeneral = Convert.ToBoolean(Request.Form["rbIsGeneral"].ToString());
                }
                else if (Request.Form.AllKeys.ToList()[i].ToString() == "menu")
                {
                    //menuvalidation.menuID = Convert.ToInt32(Request.Form["menuID"].ToString());
                    valAccess = Request.Form["menu"].ToString();
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains(".userSelected"))
                {
                    countChkUsr++;
                }
            }
            #endregion

            #region checkBoxMenu
            var modelMenu = new menuValidation();
            for (int i = 0; i < menuAssign.Count; i++)
            {
                var editor = new menuValidation.SelectMenuAuthorize()
                {
                    Id = Convert.ToInt32(menuAssign[i].menuID.ToString()),
                    menuSelected = false,
                    Name = menuAssign[i].menuName.ToString()
                };
                modelMenu.MenuAuth.Add(editor);
            }
            ViewData["menuGeneral"] = modelMenu.MenuAuth.ToList();
            #endregion

            #region checkBoxUser
            var modelUser = new user();
            var tempUser = db.Users.ToList();
            foreach (var a in tempUser)
            {
                var editor = new user.SelectActionUser()
                {
                    userId = a.userID,
                    userSelected = false,
                    userName = a.userName
                };
                modelUser.userCheckBox.Add(editor);
            }
            ViewData["userAssign"] = modelUser.userCheckBox.ToList();
            #endregion

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        string userAssigned = string.Empty;
                        for (int i = 0; i < countChkUsr; i++)
                        {
                            var checkboxValue = Request.Form["[" + i + "].userSelected"].Split(',');

                            if (checkboxValue[0].ToString().ToLower() != "false")
                            {
                                userAssigned += checkboxValue[0].ToString() + ",";
                            }
                        }

                        menuvalidation.userID = userAssigned.Substring(0, userAssigned.Length-1);
                        menuvalidation.createdDate = DateTime.Now;
                        menuvalidation.createdUser = lvm.userID;
                        db.MenuValidations.Add(menuvalidation);
                        db.SaveChanges();

                        if (menuvalidation.menuValId > 0)//var groupIdH = db.UserGroupHs.OrderByDescending(x => x.id).Select(x => x.id).First(); <--cara lain
                        {
                            if(menuvalidation.validationTypeIsGeneral == true)
                                for (int m = 0; m < countChkMenu; m++)
                                {
                                    cbMenu = Convert.ToInt32(Request.Form["menuIDGeneral" + m].ToString());
                                    valAccess = string.Empty;

                                    var cbInsert = Request.Form["insert_" + m].Split(',');
                                    valAccess += cbInsert[0].ToString().ToLower() != "false" ? "i," : "";
                                    var cbUpdate = Request.Form["update_" + m].Split(',');
                                    valAccess += cbUpdate[0].ToString().ToLower() != "false" ? "u," : "";
                                    var cbDelete = Request.Form["delete_" + m].Split(',');
                                    valAccess += cbDelete[0].ToString().ToLower() != "false" ? "d," : "";
                                    var cbView = Request.Form["view_" + m].Split(',');
                                    valAccess += cbView[0].ToString().ToLower() != "false" ? "v," : "";

                                    if (valAccess != string.Empty)
                                    {
                                        db.MenuValidationDetails.Add(new menuValidation.menuValidationDetail(){
                                            menuValIdH = menuvalidation.menuValId,
                                            menuID = cbMenu,
                                            validationAccess =  valAccess.Substring(0, valAccess.Length - 1)
                                        });
                                    }
                                }
                            else
                                db.MenuValidationDetails.Add(new menuValidation.menuValidationDetail()
                                {
                                    menuValIdH = menuvalidation.menuValId,
                                    menuID = cbMenu,
                                    validationAccess = valAccess
                                });
                        }
                        db.SaveChanges();
                        ts.Complete();
                        return RedirectToAction("Index");
                    }
                }
                catch(Exception exc)
                {
                    string a = exc.Message;
                }
            }

            return View(menuvalidation);
        }

        // GET: /MenuValidation/Edit/5
        public ActionResult Edit(int? id/*, int? menuId, string userId*/)
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
            menuValidation menuvalidation = db.MenuValidations.Find(id/*, menuId, userId*/);
            if (menuvalidation == null)
            {
                return HttpNotFound();
            }

            loadData(menuvalidation);
            return View(menuvalidation);
        }

        // POST: /MenuValidation/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(menuValidation menuvalidations)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            menuValidation menuvalidation = db.MenuValidations.Find(menuvalidations.menuValId/*, menuvalidations.menuID, menuvalidations.userID*/);

            ViewBag.menuExclusive = cm.ddlMenuValidationExclusive("");
            var menuAssign = db.Menus.Where(m => m.menuIsActive == true && m.menuLink != null).ToList();
            ViewBag.userID = new SelectList(db.Users.Where(m => m.isActive == true && m.userID != "admin").ToList(), "userID", "userName");

            #region preparing collect Data
            var countChkMenu = 0;
            var countChkUsr = 0;
            int cbMenu = 0;
            string valAccess = string.Empty;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("menuIDGeneral"))
                {
                    countChkMenu++;
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("rbIsGeneral"))
                {
                    menuvalidation.validationTypeIsGeneral = Convert.ToBoolean(Request.Form["rbIsGeneral"].ToString());
                }
                else if (Request.Form.AllKeys.ToList()[i].ToString() == "menu")
                {
                    //menuvalidation.menuID = Convert.ToInt32(Request.Form["menuID"].ToString());
                    valAccess = Request.Form["menu"].ToString();
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains(".userSelected"))
                {
                    countChkUsr++;
                }
            }
            #endregion

            #region checkBoxMenu
            var modelMenu = new menuValidation();
            for (int i = 0; i < menuAssign.Count; i++)
            {
                var editor = new menuValidation.SelectMenuAuthorize()
                {
                    Id = Convert.ToInt32(menuAssign[i].menuID.ToString()),
                    menuSelected = false,
                    Name = menuAssign[i].menuName.ToString()
                };
                modelMenu.MenuAuth.Add(editor);
            }
            ViewData["menuGeneral"] = modelMenu.MenuAuth.ToList();
            #endregion

            #region checkBoxUser
            var modelUser = new user();
            var tempUser = db.Users.Where(x => x.userID != "admin" && x.isActive == true).ToList();
            foreach (var a in tempUser)
            {
                var editor = new user.SelectActionUser()
                {
                    userId = a.userID,
                    userSelected = false,
                    userName = a.userName
                };
                modelUser.userCheckBox.Add(editor);
            }
            ViewData["userAssign"] = modelUser.userCheckBox.ToList();
            #endregion

            if (ModelState.IsValid)
            {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        string userAssigned = string.Empty;
                        for (int i = 0; i < countChkUsr; i++)
                        {
                            var checkboxValue = Request.Form["[" + i + "].userSelected"].Split(',');

                            if (checkboxValue[0].ToString().ToLower() != "false")
                            {
                                userAssigned += checkboxValue[0].ToString() + ",";
                            }
                        }

                        menuvalidation.validationName = menuvalidations.validationName;
                        menuvalidation.userID = userAssigned.Substring(0, userAssigned.Length - 1);
                        menuvalidation.modifiedDate = DateTime.Now;
                        menuvalidation.modifiedUser = lvm.userID;
                        db.Entry(menuvalidation).State = EntityState.Modified;
                        db.SaveChanges();

                        db.MenuValidationDetails.RemoveRange(db.MenuValidationDetails.Where(x => x.menuValIdH == menuvalidation.menuValId));

                        if (menuvalidation.validationTypeIsGeneral == true)
                            for (int m = 0; m < countChkMenu; m++)
                            {
                                cbMenu = Convert.ToInt32(Request.Form["menuIDGeneral" + m].ToString());
                                valAccess = string.Empty;

                                var cbInsert = Request.Form["insert_" + m].Split(',');
                                valAccess += cbInsert[0].ToString().ToLower() != "false" ? "i," : "";
                                var cbUpdate = Request.Form["update_" + m].Split(',');
                                valAccess += cbUpdate[0].ToString().ToLower() != "false" ? "u," : "";
                                var cbDelete = Request.Form["delete_" + m].Split(',');
                                valAccess += cbDelete[0].ToString().ToLower() != "false" ? "d," : "";
                                var cbView = Request.Form["view_" + m].Split(',');
                                valAccess += cbView[0].ToString().ToLower() != "false" ? "v," : "";

                                if (valAccess != string.Empty)
                                {
                                    db.MenuValidationDetails.Add(new menuValidation.menuValidationDetail()
                                    {
                                        menuValIdH = menuvalidation.menuValId,
                                        menuID = cbMenu,
                                        validationAccess = valAccess.Substring(0, valAccess.Length - 1)
                                    });
                                }
                            }
                        else
                            db.MenuValidationDetails.Add(new menuValidation.menuValidationDetail()
                            {
                                menuValIdH = menuvalidation.menuValId,
                                menuID = cbMenu,
                                validationAccess = valAccess
                            });

                        db.SaveChanges();
                        ts.Complete();
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception exc)
                {
                    string a = exc.Message;
                }
            }
            return View(menuvalidation);
        }

        // GET: /MenuValidation/Delete/5
        public ActionResult Delete(int? id/*, int menuId, string userId*/)
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
            menuValidation menuvalidation = db.MenuValidations.Find(id/*,menuId,userId*/);
            if (menuvalidation == null)
            {
                return HttpNotFound();
            }

            loadData(menuvalidation);

            #region oldprocess
            //string[] mn = db.Menus.Where(m => m.menuIsActive == true && m.menuLink != null && m.menuID == menuId).Select(x=>x.menuName).ToArray();
            //ViewData["menuName"] = mn[0].ToString();
            //string[] ui = db.Users.Where(m => m.isActive == true && m.userID == userId).Select(x=>x.userName).ToArray();
            //ViewData["userID"] = ui[0].ToString();

            //var actionSaved = db.MenuValidationDetails.Where(mv=>mv.menuValIdH == id/* && mv.menuID == menuId && mv.userID == userId*/).Select(a => a.validationAccess).ToList();
            //if (actionSaved[0] != null)
            //{ 
            //    string[] ax = actionSaved[0].ToString().Split(',');
            //    string aw = string.Empty;
            //    string[,] listActionAuth = new string[4, 2] { { "i", "Insert" }, { "u", "Update" }, { "d", "Delete" }, { "v", "View" } };
            //    for (int i = 0; i < listActionAuth.GetLength(0); i++)
            //    {
            //        if (ax.Contains(listActionAuth[i, 0]))
            //            aw += listActionAuth[i, 1] + ", ";
            //    }
            //    ViewData["actAssign"] = aw.Substring(0, aw.Length - 2);
            //}
            //else
            //    ViewData["actAssign"] = "";
            #endregion

            return View(menuvalidation);
        }

        // POST: /MenuValidation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id/*, int menuId, string userId*/)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            menuValidation menuvalidation = db.MenuValidations.Find(id/*, menuId, userId*/);
            db.MenuValidations.Remove(menuvalidation);
            db.MenuValidationDetails.RemoveRange(db.MenuValidationDetails.Where(x => x.menuValIdH == menuvalidation.menuValId));
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

        public void loadData(menuValidation menuvalidation)
        {
            var userSaved = menuvalidation.userID.Split(',');
            var menuSaved = db.MenuValidationDetails.Where(x => x.menuValIdH == menuvalidation.menuValId).ToList();
            List<string> menuGeneralSaved = new List<string>();

            #region preparing load Data
            //if(menuvalidation)
            var countChkMenu = 0;
            var countChkUsr = 0;
            for (int i = 0; i < Request.Form.Count; i++)
            {
                if (Request.Form.AllKeys.ToList()[i].Contains("menuIDGeneral"))
                {
                    countChkMenu++;
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("rbIsGeneral"))
                {
                    menuvalidation.validationTypeIsGeneral = Convert.ToBoolean(Request.Form["rbIsGeneral"].ToString());
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains("menuID"))
                {
                    //menuvalidation.menuID = Convert.ToInt32(Request.Form["menuID"].ToString());
                }
                else if (Request.Form.AllKeys.ToList()[i].Contains(".userSelected"))
                {
                    countChkUsr++;
                }
            }

            string menuExclusiveID = string.Empty;
            foreach(var ms in menuSaved)
            {
                menuGeneralSaved.Add(ms.menuID+"|"+ms.validationAccess);
                if (menuvalidation.validationTypeIsGeneral == false)
                    menuExclusiveID = ms.validationAccess;
            }
            ViewData["menuGeneralSaved"] = menuGeneralSaved;

            if (menuExclusiveID.Contains(','))
                menuExclusiveID = menuExclusiveID.Substring(0, menuExclusiveID.Length - 1);
            #endregion

            ViewBag.menuExclusive = cm.ddlMenuValidationExclusive(menuExclusiveID);
            var menuAssign = db.Menus.Where(m => m.menuIsActive == true && m.menuLink != null)// && m.menuParent != 30 && m.menuID != 30)
		     .ToList();
            ViewBag.userID = new SelectList(db.Users.Where(m => m.isActive == true && m.userID != "admin").ToList(), "userID", "userName");

            #region checkBoxMenu
            var modelMenu = new menuValidation();
            for (int i = 0; i < menuAssign.Count; i++)
            {
                var editor = new menuValidation.SelectMenuAuthorize()
                {
                    Id = Convert.ToInt32(menuAssign[i].menuID.ToString()),
                    menuSelected = false,
                    Name = menuAssign[i].menuName.ToString()
                };
                modelMenu.MenuAuth.Add(editor);
            }
            ViewData["menuGeneral"] = modelMenu.MenuAuth.OrderBy(x => x.Name).ToList();
            #endregion

            #region checkBoxUser
            var modelUser = new user();
            var tempUser = db.Users.Where(x=>x.userID != "admin").ToList();
            foreach (var a in tempUser)
            {
                bool isChecked = false;

                for(int i=0; i<userSaved.Length; i++)
                {
                    if(userSaved[i].ToString() == a.userID)
                    {
                        isChecked = true;
                        break;
                    }
                }

                var editor = new user.SelectActionUser()
                {
                    userId = a.userID,
                    userSelected = isChecked,
                    userName = a.userName
                };
                modelUser.userCheckBox.Add(editor);
            }
            ViewData["userAssign"] = modelUser.userCheckBox.OrderBy(x => x.userName).ToList();
            #endregion
        }
    }
}
