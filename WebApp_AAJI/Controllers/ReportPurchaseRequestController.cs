using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace WebApp_AAJI.Controllers
{
    public class ReportPurchaseRequestController : Controller
    {
        public ConnectionController conDB = new ConnectionController();
        //
        // GET: /ReportPurchaseRequest/
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> loadDetail(DateTime startDate, DateTime endDate)
        {
            ViewBag.linkAction = "?startDate=" + startDate.ToString() + "&endDate=" + endDate.ToString();
            string strConnection = conDB.conString;// System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlCommand cmd = new SqlCommand())
            {
                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    DataTable ds = new DataTable();
                    cmd.CommandText = "sp_reportPurchaseRequest";
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

            return PartialView("ReportPurchaseRequest_PartialPage");
        }

        //public ActionResult GenerateReport(DateTime startDate, DateTime endDate)
        //{

        //    //SqlConnection con = new SqlConnection(/*conDB.conString*/"Data Source=USER-PC\\SQLEXPRESS;Initial Catalog=DB_AAJI;User ID=sa;Password=admin123;");

        //    //DataTable dt = new DataTable();
        //    //try
        //    //{
        //    //    con.Open();
        //    //    SqlCommand cmd = new SqlCommand("SELECT * FROM dbo.PurchaseRequestHeaders", con);
        //    //    SqlDataAdapter adp = new SqlDataAdapter(cmd);
        //    //    adp.Fill(dt);
        //    //}
        //    //catch (Exception ex)
        //    //{ 

        //    //}

        //    //ReportClass rptH = new ReportClass();
        //    //rptH.FileName = Server.MapPath("/Content/Reports/PurchaseRequestReport.rpt");
        //    //rptH.Load();
        //    ////rptH.SetDataSource(dt);
        //    //Stream stream = rptH.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);

        //    //ReportDocument rd = new ReportDocument();
        //    //rd.Load(Path.Combine(Server.MapPath("~/Content/Reports"), "ReportPurchaseRequest.rpt"));
        //    //rd.SetDataSource(dt);

        //    //try
        //    //{
        //    //    Stream streams = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //    //    streams.Seek(0, SeekOrigin.Begin);
        //    //    //return File(streams, "application/pdf", "PurchaseRequestReport.pdf");
        //    //    return File(streams, "application/pdf", "ReportPurchaseRequest.pdf");   
        //    //}
        //    //catch(Exception exc)
        //    //{
        //    //    throw;
        //    //}

        //    string strConnection = conDB.conString;// System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //    using (SqlCommand cmd = new SqlCommand())
        //    {
        //        using (SqlDataAdapter da = new SqlDataAdapter())
        //        {
        //            ReportDocument rpt = new ReportDocument();
        //            rpt.Load(Path.Combine(Server.MapPath("~/Content/Reports"), "ReportPurchaseRequest.rpt"));
        //            DataSet ds = new DataSet();
        //            cmd.CommandText = "sp_reportPurchaseRequest";
        //            cmd.Connection = new SqlConnection(strConnection);
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.AddWithValue("@startDate", startDate);
        //            cmd.Parameters.AddWithValue("@endDate", endDate);
        //            da.SelectCommand = cmd;
        //            da.Fill(ds);
        //            rpt.SetParameterValue("@startDate", startDate);
        //            rpt.SetParameterValue("@endDate", endDate);
        //            //rpt.SetDataSource(ds);
        //            //rpt.ExportToHttpResponse(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, Response, false, "test.pdf");

        //            foreach (IDataParameter param in cmd.Parameters)
        //            {
        //                if (param.Value == null) param.Value = DBNull.Value;
        //            }

        //            Stream streams = rpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //            streams.Seek(0, SeekOrigin.Begin);
        //            return File(streams, "application/pdf", "ReportPurchaseRequest.pdf");   
        //        }
        //    }
        //}

        //[HttpPost]
        public ActionResult GenerateReport(DateTime startDate, DateTime endDate)
        {
            DataTable dtSalesReport = new DataTable();
 
            using (SqlConnection conn = new SqlConnection(conDB.conString))
            {
                SqlCommand cmd = new SqlCommand();
                SqlDataAdapter da = new SqlDataAdapter();
                 
 
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_reportPurchaseRequest";
 
                conn.Open();
                da.SelectCommand = cmd;
                da.Fill(dtSalesReport);
 
            }

            //ReportDocument reportDocument = new ReportDocument();
            //reportDocument.Load(Path.Combine(Server.MapPath("~/Content/Reports"), "ReportPurchaseRequest.rpt"));
            //reportDocument.SetDataSource(dtSalesReport);

            ReportClass reportDocument = new ReportClass();
            reportDocument.FileName = Server.MapPath("/Content/Reports/PurchaseRequestReport.rpt");
            reportDocument.Load();

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            try
            { 
                Stream fstream = reportDocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                fstream.Seek(0, SeekOrigin.Begin);
                return File(fstream, "application/pdf");
            }
            catch(Exception exc)
            {
                string a = exc.Message;
                throw;
            }
 
        }
	}
}