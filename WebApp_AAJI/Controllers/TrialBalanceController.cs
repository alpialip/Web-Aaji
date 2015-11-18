using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class TrialBalanceController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private CommonController ccm = new CommonController();
        private ConnectionController cc = new ConnectionController();

        [HttpGet]
        public async Task<ActionResult> showTrialBalance(string month, string year)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (int.Parse(month) < 10)
                month = "0" + month;

            string sql = string.Empty;
            sql = "EXEC [dbo].[sp_trialbalance2] '"+month+"','"+year+"' ";
            DataTable dtTrialBalance = cc.executeReader(sql);
            ViewBag.SaldoGL = dtTrialBalance;

            ViewBag.AccountLvl1 = db.chartOfAccounts.Where(x => x.levelID == 1).ToList();
            ViewBag.AccountLvl2 = db.chartOfAccounts.Where(x => x.levelID == 2).ToList();
            ViewBag.AccountLvl3 = db.chartOfAccounts.Where(x => x.levelID == 3).ToList();

            return PartialView("_PartialPageDetailTrialBalance");
        }

        //
        // GET: /TrialBalance/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.Month = ccm.ddlMonth(DateTime.Now.Month.ToString());

            return View();
        }

        [HttpGet]
        public async Task<ActionResult> loadDetailTrialBalancePerAccount(string month, string year, int coaId)
        {
            string strConnection = cc.conString;// System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    try
                    {
                        DataTable ds = new DataTable();
                        cmd.CommandText = "[sp_detailTrialBalancePerAccount]";
                        cmd.Connection = new SqlConnection(strConnection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@month", month);
                        cmd.Parameters.AddWithValue("@year", year);
                        cmd.Parameters.AddWithValue("@coaID", coaId);
                        da.SelectCommand = cmd;
                        da.Fill(ds);

                        foreach (IDataParameter param in cmd.Parameters)
                        {
                            if (param.Value == null) param.Value = DBNull.Value;
                        }

                        //ViewBag.detailTrialBalancePerAccount = ds;
                        ViewData["detailTrialBalancePerAccount"] = ds;
                        System.Web.HttpContext.Current.Session["ses_detailTrialBalancePerAccount"] = ds;
                    }
                    catch(Exception exc)
                    {
                        string a = exc.Message;
                    }
                }
            }

            return PartialView("popUpDetailTrialBalancePerAccount_PartialPage");
        }
        
        [HttpGet]
        public async Task<ActionResult> printTrialBalance(string month, string year)
        {
            string sql = string.Empty;
            sql = "EXEC [dbo].[sp_trialbalance2] '" + month + "','" + year + "' ";
            DataTable dtTrialBalance = cc.executeReader(sql);
            ViewBag.SaldoGL = dtTrialBalance;

            ViewBag.AccountLvl1 = db.chartOfAccounts.Where(x => x.levelID == 1).ToList();
            ViewBag.AccountLvl2 = db.chartOfAccounts.Where(x => x.levelID == 2).ToList();
            ViewBag.AccountLvl3 = db.chartOfAccounts.Where(x => x.levelID == 3).ToList();

            return PartialView("popUpPrintTrialBalance_PartialPage");
        }
	}
}