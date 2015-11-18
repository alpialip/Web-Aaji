using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
	public class AccountController : Controller
	{
		private MyDataAccess db = new MyDataAccess();
		LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
		private ConnectionController cd = new ConnectionController();
		//private AccountController acm = new AccountController();
		private CommonController ccm = new CommonController();

		[HttpGet]
		public async Task<ActionResult> changePassword(string userId, string password)
		{
			string encPassword = ccm.Encrypt(password.Trim());
			user usr = db.Users.Find(userId);
			usr.password = encPassword;
			usr.modifiedUser = lvm.userID;
			usr.modifiedDate = DateTime.Now;
			db.Entry(usr).State = EntityState.Modified;
			db.SaveChanges();
			return PartialView("_PartialPageChangePassword");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult changeVirtualSign(string hdnUserIdVirtualSign)
		{
			//if (acm.cekSession() == false)
			//    return RedirectToAction("Logout", "Account");

			var employeeID = 0;
			var employeeNIK = string.Empty;
			if (hdnUserIdVirtualSign == "admin")
			{

			}
			else
			{
				var usr = db.Users.Where(x => x.userID == hdnUserIdVirtualSign)
					     .Join(db.employees.AsNoTracking(), a => a.employeeNIK, b => b.employeeNIK, (a, b) => new { a, b })
					     .Select(z => new { z.b.employeeID, z.b.employeeNIK })
					     .ToList();
				employeeID = usr[0].employeeID;
				employeeNIK = usr[0].employeeNIK;
			}

			string t_imageUrl = null;
			var file = Request.Files["linkFileVirtualSign"];
			if (file != null && file.ContentLength > 0)
			{
				string extension = Path.GetExtension(file.FileName);
				string fileNameMask = employeeNIK + "_vs" /* +"_" + file.FileName*/+ extension;
				var imagePath = Path.Combine(Server.MapPath(ccm.uploadPhotoDir), fileNameMask);
				//t_imageUrl = Path.Combine(ccm.uploadPhotoDir, file.FileName);
				t_imageUrl = Path.Combine(ccm.uploadPhotoDir, fileNameMask);
				file.SaveAs(imagePath);

				Session["succeedSaveVirtualSign"] = false;
				if (hdnUserIdVirtualSign != "admin")
				{
					employee emp = db.employees.Find(employeeID);
					emp.virtualSign = t_imageUrl;
					db.Entry(emp).State = EntityState.Modified;
					db.SaveChanges();
					Session["succeedSaveVirtualSign"] = true;
				}
			}


			//return PartialView("_PartialPageVirtualSign");
			return RedirectToAction("Index", "Account");
		}

		[HttpGet]
		public async Task<ActionResult> previewVirtualSign(string nik)
		{
			var virtualSign = db.employees.Where(x => x.employeeNIK == nik).Select(x => new { x.virtualSign }).ToList();

			if (virtualSign.Count > 0)
				ViewBag.linkFileVirtualSign = virtualSign[0].virtualSign;

			return PartialView("popUpPreviewVirtualSign_PartialPage");
		}

		public ActionResult Index()
		{
			if (cekSession() == false)
				return RedirectToAction("Logout", "Account");

			#region personal Info
			lvm = System.Web.HttpContext.Current.Session["sessionUserLogin"] as LoginViewModel.userLogin;
			ViewBag.userId = lvm.userID;
			ViewBag.Name = lvm.employeeName;
			ViewBag.NIK = lvm.employeeNIK;

			var resultPosition = db.employeePositions.Where(x => x.employeeID == lvm.employeeId)
					      .Join(db.employeePositions.Where(y => y.positionId == lvm.employeePositionId), a => a.employeeID, b => b.employeeID, (a, b) => new { a, b })
					      .Join(db.departments, c => c.b.deptID, d => d.deptID, (c, d) => new { c, d })
					      .Join(db.divisis.Where(z => z.divisiID == lvm.divID), e => e.c.a.divisiID, f => f.divisiID, (e, f) => new { e, f })
					      .Select(g => new { g.e.d.deptName, g.f.divisiName, g.e.c.a.jobTitle })
					      .ToList();

			ViewBag.Position = string.Empty;
			if (lvm.isAdmin == false)
				if (resultPosition.Count > 0)
					ViewBag.Position = resultPosition[0].deptName + " - " + resultPosition[0].divisiName + " - " + resultPosition[0].jobTitle;

			TimeSpan ts = (DateTime.Now - lvm.datePosition);
			int years = ts.Days / 365;
			int months = (ts.Days % 365) / 31;
			int days = (ts.Days % 365);

			string servicePeriod = string.Empty;
			if (years > 0)
				servicePeriod += years + " Year(s) ";
			if (months > 0)
				servicePeriod += months + " Month(s) ";
			if (days > 0)
				servicePeriod += days + " Day(s) ";
			if (lvm.isAdmin == false)
				ViewBag.ServicePeriod = servicePeriod;
			else
				ViewBag.ServicePeriod = string.Empty;

			ViewBag.LeaveRemaining = string.Empty;
			//using (SqlCommand cmd = new SqlCommand())
			//{
			//    using (SqlDataAdapter da = new SqlDataAdapter())
			//    {
			//        DataTable ds = new DataTable();
			//        cmd.CommandText = "[dbo].[sp_cekSisaSaldoCuti]";
			//        cmd.Connection = new SqlConnection(cd.conString);
			//        cmd.CommandType = CommandType.StoredProcedure;
			//        cmd.Parameters.AddWithValue("@year", DateTime.Now);
			//        cmd.Parameters.AddWithValue("@employeeID", lvm.employeeId);
			//        da.SelectCommand = cmd;
			//        da.Fill(ds);

			//        foreach (IDataParameter param in cmd.Parameters)
			//        {
			//            if (param.Value == null) param.Value = DBNull.Value;
			//        }

			//        if (lvm.isAdmin == false)
			//        {
			//            foreach (DataRow dr in ds.Rows)
			//            {
			//                //year, cutiApprove, tambahanCuti, sisaSaldoCuti, totalCuti   
			//                ViewBag.LeaveRemaining = dr["sisaSaldoCuti"].ToString();
			//            }
			//        }
			//    }
			//}

			string strConnection = cd.conString;// System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
			using (SqlCommand cmd = new SqlCommand())
			{
				using (SqlDataAdapter da = new SqlDataAdapter())
				{
					DataTable ds = new DataTable();
					cmd.CommandText = "[dbo].[sp_cekSisaSaldoCuti]";
					cmd.Connection = new SqlConnection(strConnection);
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.Parameters.AddWithValue("@year", DateTime.Now.Year);
					cmd.Parameters.AddWithValue("@employeeID", lvm.employeeId);
					da.SelectCommand = cmd;
					da.Fill(ds);

					foreach (IDataParameter param in cmd.Parameters)
					{
						if (param.Value == null) param.Value = DBNull.Value;
					}

					if (lvm.isAdmin == false)
					{
						foreach (DataRow dr in ds.Rows)
						{
							//year, cutiApprove, tambahanCuti, sisaSaldoCuti, totalCuti   
							ViewBag.LeaveRemaining = dr["sisaSaldoCuti"].ToString();
						}
					}
				}
			}
			#endregion

			#region personal Reminder
			var curDate = DateTime.Now;
			ViewBag.reminder = db.transReminders.Where(x => x.createdUser == lvm.userID || lvm.employeeEmail.Contains(x.email) && x.reminderDate == curDate.Date).ToList();
			#endregion

			ViewBag.Dept = db.divisis.ToList();

			#region Purchase Request
			ViewBag.ViewRequest = null;
			foreach (string b in lvm.listAuthorize as List<string>)
			{
				string[] auth = b.Split('_');
				if (auth[0].ToString() == "RCVD")
					if (b.Contains((Url.Action("")).Replace("/", "")))
					{
						var z = db.purchaseRequestHeaders.Where(x => x.receivedStatus == false).ToList();
						if (z.Count > 0)
							ViewBag.ViewRequest = z;
					}
			}

			ViewBag.RequestApproved = null;
			foreach (string b in lvm.listAuthorize as List<string>)
			{
				if (b.Contains("A1_PurchaseRequest"))
				{
					var z = db.purchaseRequestHeaders.Where(x => x.approvedStatus == true && x.receivedStatus == false).ToList();
					if (z.Count > 0)
						ViewBag.RequestApproved = z;
				}
			}
			//ViewBag.RequestAcknowledge = db.purchaseRequestHeaders.Where(x => x.approvedStatus == true && x.receivedStatus == true).ToList();
			#endregion

			#region Purchase Order
			foreach (string b in lvm.listAuthorize as List<string>)
			{
				if (b.Contains("A1_PurchaseOrder"))
				{
					ViewBag.OrderApproved = db.purchaseOrderHeaders.Where(x => x.approvedStatus == false).ToList();
					break;
				}
			}
			#endregion

			#region Purchase Receive
			foreach (string b in lvm.listAuthorize as List<string>)
			{
				if (b.Contains("A1_PurchaseReceive"))
				{
					ViewBag.ReceivedApproved = db.purchaseReceives.Where(x => x.proposedStatus == true && x.receivedStatus == true).ToList();
					break;
				}
			}
			#endregion

			#region Purchase Receive Outstanding
			foreach (string b in lvm.listAuthorize as List<string>)
			{
				if (b.Contains("A1_PurchaseReceive"))
				{
					ViewBag.ReceivedOutstanding = db.purchaseReceives.Where(x => x.proposedStatus == true && x.receivedStatus == false).ToList();
					break;
				}
			}
			#endregion

			#region Purchase Invoice Due Date H - 7
			//foreach (string b in lvm.listAuthorize as List<string>)
			//{
			//    if (b.Contains("A1_PurchaseReceive"))
			//    {
			//        ViewBag.InvoiceDueDate = db.purchaseInvoices.Where(x => x.proposedStatus == true && x.receivedStatus == false).ToList();
			//        break;
			//    }
			//}
			#endregion

			#region Employee Will Expired Contract
			//foreach (string b in lvm.listAuthorize as List<string>)
			//{
			//    if (b.Contains("A1_PurchaseReceive"))
			//    {
			//        ViewBag.InvoiceDueDate = db.purchaseInvoices.Where(x => x.proposedStatus == true && x.receivedStatus == false).ToList();
			//        break;
			//    }
			//}
			#endregion

			ViewBag.Bank = db.banks.ToList();

			#region Advance Payment Voucher
			foreach (string b in lvm.listAuthorize as List<string>)
			{
				if (b.Contains("A1_AdvancePaymentVoucher"))
				{
					ViewBag.AdvancePaymentVoucher = db.advancePaymentVouchers.Where(x => x.preparedStatus == true && x.approvedStatus == false).ToList();
					break;
				}
			}
			#endregion

			#region Bank Cash Payment Voucher
			foreach (string b in lvm.listAuthorize as List<string>)
			{
				if (b.Contains("A1_BankCashPaymentVoucher"))
				{
					ViewBag.BankCashPaymentVoucher = db.bankCashPaymentVouchers.Where(x => x.preparedStatus == true && x.approvedStatus == false).ToList();
					break;
				}
			}
			#endregion

			#region Leave Request
			string sql = "";
			sql += "SELECT * ";
			sql += "FROM [dbo].[v_leaveRequestApproveOrAcknowledge] b  WITH(NOLOCK) ";
			sql += "WHERE b.resignDate IS NULL AND levelID < " + lvm.levelID + " AND divisiID = " + lvm.divID;
			string sqlAll = sql;

			DataTable dataLeaveRequest = cd.executeReader(sqlAll);
			ViewBag.LeaveRequest = dataLeaveRequest;

			ViewBag.ApprovalAuthLeaveRequest = false;
			foreach (string b in lvm.listAuthorize as List<string>)
			{
				string[] auth = b.Split('_');
				if (auth[0].ToString() == "A1" || auth[0].ToString() == "A2")
					if (b.Contains((Url.Action("")).Replace("/", "")))
					{
						if (auth[0].ToString() == "A1")
							ViewBag.ApprovalAuthLeaveRequest = true;
						else if (auth[0].ToString() == "A2")
						{
							ViewBag.ApprovalAuthLeaveRequest = true;
						}
					}
			}
			if (ViewBag.ApprovalAuthLeaveRequest == false && lvm.isAdmin == true)
				ViewBag.ApprovalAuthLeaveRequest = true;
			#endregion

			#region data employee

			var allemployes = db.employees.ToList();
			var employeresign = db.employeeResigns.Select(x => x.employeeID).Distinct().ToList();

			var employedata = new Dictionary<string, int>();
			employedata["active"] = allemployes.Count - employeresign.Count;
			employedata["resign"] = employeresign.Count;
			employedata["laki2"] = db.employees.Count(x => x.jenisKelamin == "L");
			employedata["perempuan"] = db.employees.Count(x => x.jenisKelamin == "P");

			var level = new Dictionary<string, int>();
			var jobtitle = new Dictionary<string, int>();
			var status = new Dictionary<string, int>();

			var allemployeeposition = db.employeePositions.ToList();
			foreach (var item in allemployeeposition)
			{
				if (!level.ContainsKey("Level " + item.levelID))
				{
					level["Level " + item.levelID] = 0;
				}
				if (!jobtitle.ContainsKey(item.jobTitle))
				{
					jobtitle[item.jobTitle] = 0;
				}
				if (!status.ContainsKey(item.employeeStatus))
				{
					status[item.employeeStatus] = 0;
				}
				level["Level " + item.levelID]++;
				jobtitle[item.jobTitle]++;
				status[item.employeeStatus]++;
			}
			ViewBag.dataemployee = employedata;
			ViewBag.level = level;
			ViewBag.jobtitle = jobtitle;
			ViewBag.status = status;

			#endregion
			return View();
		}

		[AllowAnonymous]
		public ActionResult Login()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		public ActionResult Login(LoginViewModel model, string returnUrl)
		{
			if (ModelState.IsValid)
			{
				if (Authenticate(model.UserID, model.Password))
				{
					FormsAuthentication.SetAuthCookie(model.UserID, false);
					return Redirect(returnUrl ?? Url.Action("Index", "Account"));
				}
				else
				{
					ModelState.AddModelError("", "Incorect UserID Or Password");
					return View();
				}
			}
			else
			{
				return View();
			}
		}

		public bool Authenticate(string userID, string password)
		{
			string encPassword = ccm.Encrypt(password.Trim());
			string decPassword = ccm.Decrypt(encPassword);
			//string encPassword = password;

			var result = db.Users.AsNoTracking().Where(u => u.userID == userID.Trim() && u.password == encPassword).ToList();
			string employeeName = string.Empty;
			int employeeId = 0;
			int employeePositionId = 0;
			int employeeDivisionId = 0;
			int employeeLevelId = 0;
			DateTime employeeDatePosition = DateTime.Now;

			if (result.Count > 0)
			{
				if (result[0].isAdmin == false)
				{
					string nik = result[0].employeeNIK;
					var dtEmp = db.employees.Where(x => x.employeeNIK == nik).ToList();
					if (dtEmp.Count > 0)//jika user merupakan employee
					{
						int empId = int.Parse(dtEmp[0].employeeID.ToString());
						var currDate = DateTime.Now;
						var dtEmpPos = db.employeePositions
							     .Where(x => x.employeeID == empId)
							     .OrderBy(x => x.positionDate <= currDate.Date)
							     .Take(1)
							     .ToList();

						employeeName = dtEmp[0].employeeName;
						employeeId = dtEmpPos[0].employeeID;
						employeePositionId = dtEmpPos[0].positionId;
						employeeDivisionId = dtEmpPos[0].divisiID;
						employeeDatePosition = dtEmpPos[0].positionDate;
						employeeLevelId = dtEmpPos[0].levelID;
					}
				}

				user user = db.Users.Find(userID.Trim());
				user.lastLogin = DateTime.Now;
				db.Entry(user).State = EntityState.Modified;
				db.SaveChanges();

				LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
				lvm.userID = userID.Trim();
				lvm.password = encPassword;
				lvm.userName = result[0].userName;
				lvm.deptID = employeeDivisionId; //department
				lvm.divID = result[0].deptID; //division
				lvm.isAdmin = result[0].isAdmin;
				lvm.employeeNIK = result[0].employeeNIK;
				lvm.employeeEmail = result[0].email;
				lvm.employeeId = employeeId;
				lvm.employeeName = employeeName;
				lvm.employeePositionId = employeePositionId;
				lvm.datePosition = employeeDatePosition;
				lvm.levelID = employeeLevelId;

				string sql = "SELECT b.validationAccess FROM [dbo].[MenuValidations] a ";
				sql += "INNER JOIN [dbo].[MenuValidationDetails] b ON b.menuValIdH = a.menuValId AND b.menuID = 0 ";
				sql += "WHERE a.userID LIKE '%" + lvm.userID + "%' ";
				DataTable dtListAuthorize = cd.executeReader(sql);
				//lvm.listAuthorize = null;
				foreach (DataRow dr in dtListAuthorize.Rows)
				{
					lvm.listAuthorize.Add(dr["validationAccess"].ToString());
				}

				createSessionUserLogin(lvm);
				return true;
			}
			else
			{
				return false;
			}
		}

		public void createSessionUserLogin(LoginViewModel.userLogin lvm)
		{
			System.Web.HttpContext.Current.Session["sessionUserLogin"] = lvm;
			System.Web.HttpContext.Current.Session.Timeout = 30;//minutes
		}

		public bool cekSession()
		{
			if (System.Web.HttpContext.Current.Session["sessionUserLogin"] == null)
				return false;
			return true;
		}

		public ActionResult Logout()
		{
			return View("Login");
		}

		public ActionResult NotAuthorized(string menu)
		{
			string[] mn = menu.Split('/');
			string menuName = mn[1].ToString();
			string menuAccess = mn[2].ToString();
			if (menuAccess == "Index")
				menuAccess = "";
			else if (menuAccess == "Create")
				menuAccess = "Insert";
			else if (menuAccess == "Edit")
				menuAccess = "Update";
			else if (menuAccess == "Delete")
				menuAccess = "Delete";

			@ViewBag.page = menuAccess + " " + menuName;
			return View("NotAuthorized");
		}

		public bool cekValidation(string urlAction)
		{
			string[] url = urlAction.Split('/');
			string menuLink = url[1].ToString();
			string action = url[2].ToString();
			if (action.Contains("?"))
			{
				string[] act = action.Split('?');
				action = act[0].ToString();
			}

			if (action == "Index")
				action = "v";
			else if (action == "Create")
				action = "i";
			else if (action == "Edit")
				action = "u";
			else if (action == "Details")
				action = "v";
			else if (action == "Delete")
				action = "d";

			bool isValidated = lvm.isAdmin == true ? true : false;
			lvm = System.Web.HttpContext.Current.Session["sessionUserLogin"] as LoginViewModel.userLogin;
			string sqlUser = "SELECT userID FROM [dbo].[Users] WHERE employeeNIK = '" + lvm.employeeNIK + "'";
			object userID = cd.executeScalar(sqlUser);

			string sql = "SELECT b.validationAccess FROM [dbo].[MenuValidations] a ";
			sql += "INNER JOIN [dbo].[MenuValidationDetails] b ON b.menuValIdH = a.menuValId  AND b.menuID <> 0 ";
			sql += "INNER JOIN [dbo].[Menus] c ON c.menuID = b.menuID ";
			sql += "WHERE a.userID LIKE '%" + userID + "%'";
			sql += "AND c.menuLink = '" + menuLink + "' ";
			object execSql = cd.executeScalar(sql);

			if (execSql != null)
			{
				string[] dtResult = cd.executeScalar(sql).ToString().Split(',');
				for (int i = 0; i < dtResult.Length; i++)
				{
					if (action == dtResult[i].ToString())
					{
						isValidated = true;
						break;
					}
				}
			}

			//if(isValidated == false)
			//    isValidated = cekValidationExclusive(urlAction);

			return isValidated;
		}

		public bool cekValidationExclusive(string urlActton)
		{
			string[] url = urlActton.Split('/');
			string menuLink = url[1].ToString();
			string action = url[2].ToString();

			if (action == "Index")
				action = "v";
			else if (action == "Create")
				action = "i";
			else if (action == "Edit")
				action = "u";
			else if (action == "Details")
				action = "v";
			else if (action == "Delete")
				action = "d";

			bool isValidated = lvm.isAdmin == true ? true : false;
			lvm = System.Web.HttpContext.Current.Session["sessionUserLogin"] as LoginViewModel.userLogin;
			string sql = "SELECT b.validationAccess FROM [dbo].[MenuValidations] a ";
			sql += "INNER JOIN [dbo].[MenuValidationDetails] b ON b.menuValIdH = a.menuValId AND b.menuID = 0 ";
			sql += "WHERE a.userID = (SELECT userID FROM [dbo].[Users] WHERE employeeNIK = '" + lvm.employeeNIK + "') ";
			sql += "AND b.[validationAccess] LIKE '%" + menuLink + "%' ";
			object execSql = cd.executeScalar(sql);

			if (execSql != null)
			{
				//string[] dtResult = cd.executeScalar(sql).ToString().Split(',');
				//for (int i = 0; i < dtResult.Length; i++)
				//{
				if (action == "v")
				{
					isValidated = true;
					//        break;
				}
				//}
			}

			return isValidated;
		}

	}
}
