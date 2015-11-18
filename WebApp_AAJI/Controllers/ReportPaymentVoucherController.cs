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
    public class ReportPaymentVoucherController : Controller
    {
        public MyDataAccess db = new MyDataAccess();
        public ConnectionController conDB = new ConnectionController();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();

        //
        // GET: /ReportFixedAsset/
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> loadDetail(DateTime startDate, DateTime endDate)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });


            ViewBag.linkAction = "?startDate=" + startDate.ToString() + "&endDate=" + endDate.ToString();
            string strConnection = conDB.conString;// System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    DataTable ds = new DataTable();
                    cmd.CommandText = "[sp_reportPaymentVoucher]";
                    cmd.Connection = new SqlConnection(strConnection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    da.SelectCommand = cmd;
                    da.Fill(ds);

                    foreach (IDataParameter param in cmd.Parameters)
                    {
                        if (param.Value == null) param.Value = DBNull.Value;
                    }

                    //if (ds.Rows.Count > 0)
                    ViewBag.detail = ds;
                }
            }

            return PartialView("ReportPaymentVoucher_PartialPage");
        }
	}
}