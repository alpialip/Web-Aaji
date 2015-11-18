using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class NavController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        private ConnectionController cd = new ConnectionController();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();

        public NavController()
        {

        }
        public ActionResult Menu()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            #region menu personal
            List<menu> parentMenuPersonal = db.Menus.Where(m => m.menuIsParent == true && m.menuIsActive == true && m.menuID == 30).OrderBy(m => m.menuName).ToList();
            ViewData["parentMenuPersonal"] = parentMenuPersonal;
            List<menu> subMenuPersonal = db.Menus.Where(m => m.menuIsParent == false && m.menuIsActive == true && m.menuParent == 30).OrderBy(m => m.menuName).ToList();
            ViewData["subMenuPersonal"] = subMenuPersonal;
            #endregion
            
            IEnumerable<string> menu = db.Menus.Select(m => m.menuName).Distinct().OrderBy(m => m);
            if (Session["sessionUserLogin"] != null)
            {
                #region menu umum
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;

                List<menu> parentMenu = new List<menu>();
                List<menu> subMenu = new List<menu>();
                List<menu> subMenuPurchase = new List<menu>();
                if (lvm.isAdmin == true)
                {
                    #region oldProcess
			  parentMenu = db.Menus.Where(m => m.menuIsParent == true && m.menuIsActive == true && m.menuID != 30 && m.menuParent != 30).OrderBy(m => m.menuName).ToList();
			  subMenu = db.Menus.Where(m => m.menuIsParent == false && m.menuIsActive == true && m.menuID != 30 && m.menuParent != 30).OrderBy(m => m.menuName).ToList();
                    subMenuPurchase = db.Menus.Where(m => m.menuIsParent == false && m.menuIsActive == true && m.menuID != 30 && m.menuParent == 3 ).OrderBy(m => m.menuID).ToList();
                    #endregion
                }
                else
                {
                    #region newProcess
                    string sqlPurchaseSubMenu = string.Empty;
                    string sql = "SELECT c.* ";
                    sql += "FROM [dbo].[MenuValidations] a ";
                    sql += "INNER JOIN [dbo].[MenuValidationDetails] b ON b.menuValIdH = a.menuValId ";
                    sql += "INNER JOIN [dbo].[Menus] c ON c.menuID = b.menuID ";
                    sql += "WHERE userID LIKE '%" + lvm.userID + "%' ";
                    sql += "AND c.menuIsParent = 0 ";
                    sql += "AND c.menuIsActive = 1 ";
		      sql += "AND c.menuID != 30 ";
		      sql += "AND c.menuParent != 30 ";
                    sqlPurchaseSubMenu = sql + " AND menuParent=3 ORDER BY c.menuID ";
                    sql += "ORDER BY c.menuName ";
                    DataTable dtResult = cd.executeReader(sql);
                    DataTable dtResultSubMenuPurchase = cd.executeReader(sqlPurchaseSubMenu);

                    List<int> mnParent = new List<int>();
                    subMenu = new List<menu>();
                    foreach (DataRow dr in dtResult.Rows)
                    {
                        menu mn = new menu();
                        DateTime? createdDate = null;
                        DateTime? modifiedDate = null;
                        //[menuID],[menuName],[menuLink],[menuParent],[menuDescription],[menuIsParent],[menuIsActive],[createdDate],[createdUser],[modifiedDate],[modifiedUser]
                        mn.menuID = int.Parse(dr["menuID"].ToString());
                        mn.menuName = dr["menuName"].ToString();
                        mn.menuLink = dr["menuLink"].ToString();
                        mn.menuParent = int.Parse(dr["menuParent"].ToString());
                        mn.menuDescription = dr["menuDescription"].ToString();
                        mn.menuIsParent = bool.Parse(dr["menuIsParent"].ToString());
                        mn.menuIsActive = bool.Parse(dr["menuIsActive"].ToString());
                        mn.createdDate = dr["createdDate"].ToString() == "" ? createdDate : DateTime.Parse(dr["createdDate"].ToString());
                        mn.createdUser = dr["createdUser"].ToString();
                        mn.modifiedDate = dr["modifiedDate"].ToString() == "" ? modifiedDate : DateTime.Parse(dr["modifiedDate"].ToString());
                        mn.modifiedUser = dr["modifiedUser"].ToString();

                        if (!mnParent.Contains(mn.menuParent))
                            mnParent.Add(mn.menuParent);

                        subMenu.Add(mn);
                    }

                    subMenuPurchase = new List<menu>();
                    foreach (DataRow dr in dtResultSubMenuPurchase.Rows)
                    {
                        menu mn = new menu();
                        DateTime? createdDate = null;
                        DateTime? modifiedDate = null;
                        //[menuID],[menuName],[menuLink],[menuParent],[menuDescription],[menuIsParent],[menuIsActive],[createdDate],[createdUser],[modifiedDate],[modifiedUser]
                        mn.menuID = int.Parse(dr["menuID"].ToString());
                        mn.menuName = dr["menuName"].ToString();
                        mn.menuLink = dr["menuLink"].ToString();
                        mn.menuParent = int.Parse(dr["menuParent"].ToString());
                        mn.menuDescription = dr["menuDescription"].ToString();
                        mn.menuIsParent = bool.Parse(dr["menuIsParent"].ToString());
                        mn.menuIsActive = bool.Parse(dr["menuIsActive"].ToString());
                        mn.createdDate = dr["createdDate"].ToString() == "" ? createdDate : DateTime.Parse(dr["createdDate"].ToString());
                        mn.createdUser = dr["createdUser"].ToString();
                        mn.modifiedDate = dr["modifiedDate"].ToString() == "" ? modifiedDate : DateTime.Parse(dr["modifiedDate"].ToString());
                        mn.modifiedUser = dr["modifiedUser"].ToString();

                        if (!mnParent.Contains(mn.menuParent))
                            mnParent.Add(mn.menuParent);

                        subMenuPurchase.Add(mn);
                    }

                    parentMenu = db.Menus.Where(m => m.menuIsParent == true && m.menuIsActive == true && mnParent.Contains(m.menuID)).OrderBy(m => m.menuName).ToList();
                    #endregion
                }

                ViewData["parentMenu"] = parentMenu;
                ViewData["subMenu"] = subMenu.ToList();
                ViewData["subMenuPurchase"] = subMenuPurchase.ToList();
                #endregion
            }
            
            return PartialView(menu);
        }
	}
}