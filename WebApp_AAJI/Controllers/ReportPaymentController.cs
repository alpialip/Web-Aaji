using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class ReportPaymentController : Controller
    {
        public MyDataAccess db = new MyDataAccess();
        public ConnectionController conDB = new ConnectionController();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();

        //
        // GET: /ReportPayment/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

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


            ViewBag.linkAction = "?startDate=" + startDate.ToString("yyyy/MM/dd") + "&endDate=" + endDate.ToString("yyyy/MM/dd");
            string strConnection = conDB.conString;// System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    DataTable ds = new DataTable();
                    cmd.CommandText = "[sp_reportPayment]";
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

            return PartialView("ReportPayment_PartialPage");
        }
        
        [HttpGet]
        public async Task<ActionResult> printPayment(string startDate, string endDate)
        //public ActionResult printPayment(DateTime startDate, DateTime endDate)
        {
            //ViewBag.period = DateTime.ParseExact(startDate.ToString("MM/dd/yyyy"), "dd/MM/yyyy",System.Globalization.CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy") 
            //                +" - " +
            //                DateTime.ParseExact(endDate.ToString("MM/dd/yyyy"), "dd/MM/yyyy",System.Globalization.CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy");
            ViewBag.period = startDate + "-" + endDate;
            string strConnection = conDB.conString;// System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    DataTable ds = new DataTable();
                    cmd.CommandText = "[sp_reportPayment]";
                    cmd.Connection = new SqlConnection(strConnection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@startDate", Convert.ToDateTime(startDate));
                    cmd.Parameters.AddWithValue("@endDate", Convert.ToDateTime(endDate));
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

            return PartialView("printReportPayment_PartialPage");
        }
        
        [HttpGet]
        public async Task<ActionResult> savePaymentTypeReportPayment(string voucherNo, string paymentType)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            transReportPayment transreportpayment = db.transReportPayments.Find(voucherNo);
            if(transreportpayment != null)
            {
                transreportpayment.paymentType = paymentType;
                transreportpayment.modifiedDate = DateTime.Now;
                transreportpayment.modifiedUser = lvm.userID;

                db.Entry(transreportpayment).State = EntityState.Modified;
            }
            else
            {
                transreportpayment.voucherNo = voucherNo;
                transreportpayment.paymentType = paymentType;
                transreportpayment.modifiedDate = DateTime.Now;
                transreportpayment.modifiedUser = lvm.userID;

                db.transReportPayments.Add(transreportpayment);
            }
            db.SaveChanges();

            return PartialView("ReportPayment_PartialPage");
        }

        public ActionResult savePaymentTypeReportPayment_Json(string voucherNo, string paymentType)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            string isResult = string.Empty;
            try
            {
                transReportPayment transreportpayment = db.transReportPayments.Find(voucherNo);
                if (transreportpayment != null)
                {
                    transreportpayment.paymentType = paymentType;
                    transreportpayment.modifiedDate = DateTime.Now;
                    transreportpayment.modifiedUser = lvm.userID;

                    db.Entry(transreportpayment).State = EntityState.Modified;
                }
                else
                {
                    transReportPayment transreportpayment_new = new transReportPayment();
                    transreportpayment_new.voucherNo = voucherNo;
                    transreportpayment_new.paymentType = paymentType;
                    transreportpayment_new.modifiedDate = DateTime.Now;
                    transreportpayment_new.modifiedUser = lvm.userID;

                    db.transReportPayments.Add(transreportpayment_new);
                }
                db.SaveChanges();

                isResult = "OK";
            }
            catch(Exception exc)
            {

            }

            var result = new List<object>();

            result.Add(new { status = isResult, id = voucherNo, Year = 1984 });

            return Json(result, JsonRequestBehavior.AllowGet);

        }

	}
}