using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApp_AAJI.Controllers
{
    public class ConnectionController : Controller
    {
        private SqlConnection conn;
        private SqlCommand command;
        private SqlDataReader reader;
        //
        // GET: /Connection/
        public string conString;
        public static string dbName;
        public string backupPath;

        public ConnectionController()
        {
            conString = ConfigurationManager.ConnectionStrings["MyDataAccess"].ToString();
            dbName = ConfigurationManager.AppSettings["dbName"].ToString();
            backupPath = ConfigurationManager.AppSettings["backupPath"].ToString();
        }

        public DataTable executeReader(string query)
        {
            SqlDataAdapter adap = new SqlDataAdapter(query, conString);
            DataTable data = new DataTable();
            adap.Fill(data);
            return data;
        }

        public object executeScalar(string query)
        {
            SqlDataAdapter adap = new SqlDataAdapter(query, conString);
            DataTable data = new DataTable();
            adap.Fill(data);
            if (data.Columns.Count > 1 || data.Rows.Count > 1 || data.Columns.Count == 0 || data.Rows.Count == 0)
                return null;
            object a = (object)data.Rows[0][0];
            return a;
        }

        public bool backupDB(string backupDirectory)
        {
            try 
            { 
                conn = new SqlConnection(conString);
                conn.Open();
                string sql = "BACKUP DATABASE " + dbName + " TO DISK = '" + backupDirectory + "\\" + dbName + "-" + DateTime.Now.Ticks.ToString() + ".bak'";
                command = new SqlCommand(sql, conn);
                command.ExecuteNonQuery();
                return true;
            }
            catch(Exception exc)
            {
                string a = exc.Message;
                return false;
            }
        }

        public bool posting(string month, string year, string userID)
        {
            try
            {
                conn = new SqlConnection(conString);
                conn.Open();
                string sql = "EXEC [dbo].[sp_posting] '" + month + "','" + year + "','" + userID + "'";
                command = new SqlCommand(sql, conn);
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception exc)
            {
                string a = exc.Message;
                return false;
            }
        }
	}
}