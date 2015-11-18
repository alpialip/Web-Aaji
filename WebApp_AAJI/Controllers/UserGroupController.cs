using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;
using System.Transactions;

namespace WebApp_AAJI.Controllers
{
    public class UserGroupController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();

        // GET: /UserGroup/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            return View(db.UserGroupHs.ToList());
        }

        // GET: /UserGroup/Details/5
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
            userGroupH usergroup = db.UserGroupHs.Find(id);
            ViewData["userAssigned"] = db.UserGroupDs.Where(uD => uD.groupHId == id)
                                        .Join(db.Users.Where(uM => uM.isActive == true), a => a.userID, b => b.userID, (a, b) => b).ToList();            

            if (usergroup == null)
            {
                return HttpNotFound();
            }
            return View(usergroup);
        }

        // GET: /UserGroup/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            var model = new userGroupH();
            foreach (var person in db.Users)
            {
                var editorViewModel = new userGroupH.SelectPersonEditorViewModel()
                {
                    Id = person.userID,
                    Name = string.Format("{0}", person.userName),
                    Selected = false
                };
                model.User.Add(editorViewModel);
            }
            ViewData["userAssign"] = model.User.ToList();            

            return View();
        }

        // POST: /UserGroup/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(/*[Bind(Include="id,groupName,groupDesc,isActive,createdDate,createdUser,modifiedDate,modifiedUser")]*/ userGroupH usergroup)
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
                        usergroup.createdDate = DateTime.Now;
                        usergroup.createdUser = lvm.userID;
                        db.UserGroupHs.Add(usergroup);
                        db.SaveChanges();
                        //usergroup.id = 4;
                        if (usergroup.id > 0)//var groupIdH = db.UserGroupHs.OrderByDescending(x => x.id).Select(x => x.id).First(); <--cara lain
                        { 
                            userGroupH.userGroupD ugD = new userGroupH.userGroupD();

                            #region cara pertama ambil checkbox, pastikan id/nama attribut sama pada chekbox group ~unused
                            //if (Request.Form.Count>0)
                            //{
                            //    var checkboxValue = Request.Form["User[0].Selected"].ToString().Split(',').ToList();
                            //    for (int i = 0; i < checkboxValue.Count-1; i++ )
                            //    {
                            //        if (checkboxValue[i].ToString().ToLower() != "false")
                            //        {
                            //            db.UserGroupDs.Add(new userGroupH.userGroupD 
                            //                                { groupHId = Convert.ToInt32(groupIdH), 
                            //                                  userID = checkboxValue[i].ToString(), 
                            //                                  modifiedDate = DateTime.Now, 
                            //                                  modifiedUser = "" 
                            //                                });
                            //        }
                            //    }
                            //}
                            #endregion

                            #region cara kedua ambil checkbox
                            var countChk = 0;

                            for (int i = 0; i < Request.Form.Count; i++)
                            {
                                if (Request.Form.AllKeys.ToList()[i].Contains(".Selected"))
                                    countChk++;
                            }

                            for (int i = 0; i < countChk; i++)
                            {
                                var checkboxValue = Request.Form["[" + i + "].Selected"].Split(',');

                                if (checkboxValue[0].ToString().ToLower() != "false")
                                {
                                    db.UserGroupDs.Add(new userGroupH.userGroupD
                                    {
                                        groupHId = Convert.ToInt32(usergroup.id),
                                        userID = checkboxValue[0].ToString(),
                                        modifiedDate = DateTime.Now,
                                        modifiedUser = ""
                                    });
                                }
                            }
                            #endregion

                            db.SaveChanges();
                        }

                        ts.Complete();
                        return RedirectToAction("Index");
                    }                    
                }
                catch(Exception exc)
                {
                    string a = exc.Message;
                }
            }

            return View(usergroup);
        }

        // GET: /UserGroup/Edit/5
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
            userGroupH usergroup = db.UserGroupHs.Find(id);
            if (usergroup == null)
            {
                return HttpNotFound();
            }

            ViewData["userAssigned"] = db.UserGroupDs.Where(u => u.groupHId == id).ToList();
            var model = new userGroupH();

            foreach (var person in db.Users)
            {
                var editorViewModel = new userGroupH.SelectPersonEditorViewModel();
                var isChecked = false;
                foreach(var a in ViewData["userAssigned"] as IEnumerable<userGroupH.userGroupD>)
                {
                    if(a.userID == person.userID)
                    {
                        isChecked = true;
                    }
                }

                editorViewModel.Id = person.userID;
                editorViewModel.Name = string.Format("{0}", person.userName);
                editorViewModel.Selected = isChecked;
                model.User.Add(editorViewModel);
            }
            ViewData["userAssign"] = model.User.ToList();

            return View(usergroup);
        }

        // POST: /UserGroup/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,groupName,groupDesc,isActive,createdDate,createdUser,modifiedDate,modifiedUser")] userGroupH usergroup)
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
                        usergroup.modifiedDate = DateTime.Now;
                        usergroup.modifiedUser = lvm.userID;
                        db.Entry(usergroup).State = EntityState.Modified;
                        //db.SaveChanges();

                        userGroupH.userGroupD ugD = new userGroupH.userGroupD();

                        var countChk = 0;
                        for (int i = 0; i < Request.Form.Count; i++)
                        {
                            if (Request.Form.AllKeys.ToList()[i].Contains(".Selected"))
                                countChk++;
                        }

                        bool runFirst = true;
                        for (int i = 0; i < countChk; i++)
                        {
                            if(runFirst == true)
                            {
                                var data = db.UserGroupDs.Where(x => x.groupHId == usergroup.id);
                                foreach(var a in data)
                                {
                                    userGroupH.userGroupD isFind = db.UserGroupDs.Find(a.groupHId, a.userID);
                                    db.UserGroupDs.Remove(isFind);
                                }
                                db.SaveChanges();
                                runFirst = false;
                            }

                            var checkboxValue = Request.Form["[" + i + "].Selected"].Split(',');
                            if (checkboxValue[0].ToString().ToLower() != "false")
                            {
                                db.UserGroupDs.Add(new userGroupH.userGroupD
                                {
                                    groupHId = Convert.ToInt32(usergroup.id),
                                    userID = checkboxValue[0].ToString(),
                                    modifiedDate = DateTime.Now,
                                    modifiedUser = ""
                                });
                            }

                            #region old
                            //if (checkboxValue[0].ToString().ToLower() != "false" && isFind.Equals(null))//jika checked=true & isFind=false, maka perlu lakukan penambahan
                            //{ 
                            //    db.UserGroupDs.Add(new userGroupH.userGroupD
                            //    {
                            //        groupHId = Convert.ToInt32(usergroup.id),
                            //        userID = checkboxValue[0].ToString(),
                            //        modifiedDate = DateTime.Now,
                            //        modifiedUser = ""
                            //    });
                            //}
                            //else if(checkboxValue[0].ToString().ToLower() == "false" && !isFind.Equals(null))//jika checked=false & isFind=true, maka hapus
                            //{
                            //}
                            #endregion
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
            return View(usergroup);
        }

        // GET: /UserGroup/Delete/5
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
            userGroupH usergroup = db.UserGroupHs.Find(id);
            if (usergroup == null)
            {
                return HttpNotFound();
            }
            return View(usergroup);
        }

        // POST: /UserGroup/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            userGroupH usergroup = db.UserGroupHs.Find(id);
            db.UserGroupHs.Remove(usergroup);
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
