using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;
using PagedList;
using System.Data.SqlClient;

namespace WebApp_AAJI.Controllers
{
	public class EmployeeController : Controller
	{
		private MyDataAccess db = new MyDataAccess();
		LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
		private AccountController acm = new AccountController();
		private CommonController ccm = new CommonController();
		public ConnectionController conDB = new ConnectionController();
		string prefix = "";

		[HttpGet]
		public async Task<ActionResult> LoadDetailEmployee(string dataDetail)
		{
			string[] data = dataDetail.Split('|');

			var model = new employee();
			for (int i = 0; i < data.Count(); i++)
			{
				string[] value = data[i].Split(';');

				if (value[0].ToString() == "")
					break;

				//posDate + ";" + posDept + ";" + posDiv + ";" + escape(posTitle) + ";" + posLevel + ";" + posStatus + ";" + resDate + ";" + resRemark
				DateTime posDate = Convert.ToDateTime(value[0].ToString());
				int posDept = int.Parse(value[1].ToString());
				int posDiv = int.Parse(value[2].ToString());
				string posTitle = value[3].ToString();
				int posLevel = int.Parse(value[4].ToString());
				string posStatus = value[5].ToString();
				DateTime resDate = Convert.ToDateTime(value[6].ToString());
				string resRemark = value[7].ToString();

				var editor = new employee.employeePosition()
				{
					positionDate = posDate,
					deptID = posDept,
					divisiID = posDiv,
					jobTitle = posTitle,
					levelID = posLevel,
					employeeStatus = posStatus
				};
				model.positionEmployee.Add(editor);

				var editorRes = new employee.employeeResign()
				{
					resignDate = resDate,
					remarks = resRemark
				};
				model.resignEmployee.Add(editorRes);
			}
			ViewBag.detailPosition = model.positionEmployee.ToList();
			ViewBag.detailResign = model.resignEmployee.ToList();

			Session["detailPositionSession"] = model.positionEmployee.ToList();
			Session["detailResignSession"] = model.resignEmployee.ToList();

			ViewBag.DeptList = ccm.ddlDepartment(string.Empty);
			ViewBag.DivList = ccm.ddlDivisi(string.Empty);
			ViewBag.LevelList = ccm.ddlLevel(string.Empty);
			ViewBag.StatusList = ccm.ddlStatusEmployee(string.Empty);

			return PartialView("_PartialPageEmpPosition1");
		}

		//[HttpGet]
		//public async Task<ActionResult> LoadDetailEmployeeFamily(string dataDetail)
		//{
		//    string[] data = dataDetail.Split('|');

		//    var model = new employee();
		//    for (int i = 0; i < data.Count(); i++)
		//    {
		//        string[] value = data[i].Split(';');

		//        if (value[0].ToString() == "")
		//            break;

		//        //posDate + ";" + posDept + ";" + posDiv + ";" + escape(posTitle) + ";" + posLevel + ";" + posStatus + ";" + resDate + ";" + resRemark
		//        DateTime mateBirthDate = Convert.ToDateTime(value[0].ToString());
		//        int posDept = int.Parse(value[1].ToString());
		//        int posDiv = int.Parse(value[2].ToString());
		//        string posTitle = value[3].ToString();
		//        int posLevel = int.Parse(value[4].ToString());
		//        string posStatus = value[5].ToString();
		//        DateTime resDate = Convert.ToDateTime(value[6].ToString());
		//        string resRemark = value[7].ToString();

		//        var editor = new employee.employeePosition()
		//        {
		//            positionDate = posDate,
		//            deptID = posDept,
		//            divisiID = posDiv,
		//            jobTitle = posTitle,
		//            levelID = posLevel,
		//            employeeStatus = posStatus
		//        };
		//        model.positionEmployee.Add(editor);

		//        var editorRes = new employee.employeeResign()
		//        {
		//            resignDate = resDate,
		//            remarks = resRemark
		//        };
		//        model.resignEmployee.Add(editorRes);
		//    }
		//    ViewBag.detailPosition = model.positionEmployee.ToList();
		//    ViewBag.detailResign = model.resignEmployee.ToList();

		//    Session["detailPositionSession"] = model.positionEmployee.ToList();
		//    Session["detailResignSession"] = model.resignEmployee.ToList();

		//    ViewBag.DeptList = ccm.ddlDepartment(string.Empty);
		//    ViewBag.DivList = ccm.ddlDivisi(string.Empty);
		//    ViewBag.LevelList = ccm.ddlLevel(string.Empty);
		//    ViewBag.StatusList = ccm.ddlStatusEmployee(string.Empty);

		//    return PartialView("_PartialPageEmpFamily");
		//}

		[HttpGet]
		public async Task<ActionResult> previewCV(int employeeID)
		{
			var cv = db.employeeCVs.AsNoTracking().Where(x => x.employeeID == employeeID).Select(x => new { x.linkFileCV }).ToList();

			if (cv.Count > 0)
				ViewBag.linkCV = cv[0].linkFileCV;

			return PartialView("popUpPreviewCV_PartialPage");
		}

		[HttpGet]
		public async Task<ActionResult> LoadDetailDDLDivision(string deptId, string divId, string idx)
		{
			if (deptId != "" && divId == "")
				ViewBag.DivList = ccm.ddlDivisi(string.Empty, int.Parse(deptId));
			else if (deptId != "" && divId != "")
				ViewBag.DivList = ccm.ddlDivisi(divId, int.Parse(deptId));
			else
				ViewBag.DivList = null;

			ViewBag.idx = idx;
			return PartialView("_PartialPageDDLDivision1");
		}

		// GET: /Employee/
		public ActionResult Index(string sortOrder, string currentFilter, string searchString, string startDateFilter, string startDate, string endDateFilter, string endDate, int? page, int? layout)
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });
			if (layout == 0)
			{
				ViewBag.lays = true;
			}
			else
			{
				ViewBag.lays = false;
			}

			#region employeeResign
			string strConnection = conDB.conString;// System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
			using (SqlCommand cmd = new SqlCommand())
			{
				using (SqlDataAdapter da = new SqlDataAdapter())
				{
					DataTable ds = new DataTable();
					cmd.CommandText = "SELECT DISTINCT employeeID FROM dbo.v_employee WHERE resignDate IS NOT NULL";
					cmd.Connection = new SqlConnection(strConnection);
					cmd.CommandType = CommandType.Text;
					da.SelectCommand = cmd;
					da.Fill(ds);

					foreach (IDataParameter param in cmd.Parameters)
					{
						if (param.Value == null) param.Value = DBNull.Value;
					}

					//if (ds.Rows.Count > 0)
					ViewBag.EmployeeResign = ds;
				}
			}
			#endregion

			#region paging
			ViewBag.CurrentSort = sortOrder;
			ViewBag.VoucherSortParm = String.IsNullOrEmpty(sortOrder) ? "date_" : "";
			ViewBag.DateSortParm = sortOrder == "voucherNo" ? "voucherNo_" : "voucherNo";

			if (searchString != null || (startDate != null && endDate != null))
				page = 1;
			else
			{
				searchString = currentFilter;
				startDate = startDateFilter;
				endDate = endDateFilter;
			}
			ViewBag.CurrentFilter = searchString;
			ViewBag.startDateFilter = startDate;
			ViewBag.endDateFilter = endDate;

			var data = from s in db.employees select s;
			//if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate) && !String.IsNullOrEmpty(searchString))
			//{
			//    DateTime sDate = DateTime.Parse(startDate);
			//    DateTime eDate = DateTime.Parse(endDate);
			//    data = data.Where(s => s.klaimDate >= sDate && s.klaimDate <= eDate && s.klaimID.Contains(searchString));
			//}
			//else 
			//if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
			//{
			//    DateTime sDate = DateTime.Parse(startDate);
			//    DateTime eDate = DateTime.Parse(endDate);
			//    data = data.Where(s => s.klaimDate >= sDate && s.klaimDate <= eDate);
			//}
			//else 
			if (!String.IsNullOrEmpty(searchString))
			{
				data = data.Where(s => s.employeeName.Contains(searchString) || s.employeeNIK.Contains(searchString));
			}

			switch (sortOrder)
			{
				case "date_":
					data = data.OrderByDescending(s => s.employeeName);
					break;
				//case "voucherNo":
				//    data = data.OrderBy(s => new { s.purchaseInvoiceNo });
				//    break;
				//case "voucherNo_":
				//    data = data.OrderByDescending(s => s.purchaseInvoiceNo);
				//    break;
				//case "isParent":
				//    data = data.OrderBy(s => new { s.menuIsParent, s.menuName });
				//    break;
				//case "isParent_":
				//    data = data.OrderByDescending(s => s.menuIsParent);
				//    break;
				//case "isActive":
				//    data = data.OrderBy(s => new { s.menuIsActive, s.menuName });
				//    break;
				//case "isActive_":
				//    data = data.OrderByDescending(s => s.menuIsActive);
				//    break;
				default:
					data = data.OrderBy(s => s.createdDate);
					break;
			}

			int pageSize = 10;
			int pageNumber = (page ?? 1);
			return View(data.ToPagedList(pageNumber, pageSize));
			#endregion

			//return View(db.employees.ToList());
		}

		// GET: /Employee/Details/5
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
			employee employee = db.employees.Find(id);
			if (employee == null)
			{
				return HttpNotFound();
			}

			loadDetailEmployee(employee.employeeID, employee.employeeNIK, employee.bankName, employee);
			return View(employee);
		}

		// GET: /Employee/Create
		public ActionResult Create()
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			ViewBag.typeCreate = "G,G,";//default
			ViewBag.NIK = string.Empty;//default
			ViewBag.ListBankExistingOnEmployee = ccm.ddlBankEmployee(string.Empty);
			ViewBag.userID = string.Empty;
			ViewBag.posDate = DateTime.Now;
			ViewBag.resDate = DateTime.Now;
			ViewBag.DeptList = ccm.ddlDepartment(string.Empty);
			ViewBag.DivList = ccm.ddlDivisi(string.Empty);
			ViewBag.LevelList = ccm.ddlLevel(string.Empty);
			ViewBag.StatusList = ccm.ddlStatusEmployee(string.Empty);
			//ketika gender dibuat default, on change dari view create tidak akan berjalan
			//ViewBag.Gender = "L";//default 
			ViewBag.statusNikah = ccm.statusNikah(string.Empty);
			ViewBag.agama = ccm.agama(string.Empty);
			ViewBag.kewarganegaraan = ccm.kewarganegaraan(string.Empty);
			return View();
		}

		// POST: /Employee/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(employee employee)
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });



			#region collect userid
			string userID = string.Empty;
			string typeCreated = string.Empty;
			string statusNikah = string.Empty;
			string agama = string.Empty;
			string kewarganegaraan = string.Empty;
			bool existingBank = false;
			string namaBank = string.Empty;
			for (int i = 0; i < Request.Form.Count; i++)
			{
				if (Request.Form.AllKeys.ToList()[i].Contains("userID"))
				{
					userID = Request.Form["userID"].ToString();
				}
				else if (Request.Form.AllKeys.ToList()[i].Contains("typeCreate"))
				{
					typeCreated += Request.Form["typeCreate"].ToString().Substring(0, 1) + ",";
				}
				else if (Request.Form.AllKeys.ToList()[i].Contains("typeCreateUserId"))
				{
					typeCreated += Request.Form["typeCreateUserId"].ToString().Substring(0, 1);
				}
				else if (Request.Form.AllKeys.ToList()[i].Contains("existingBank"))
				{
					string[] chkExistingBank = Request.Form["existingBank"].ToString().Split(',');
					existingBank = bool.Parse(chkExistingBank[0]);
				}
				else if (Request.Form.AllKeys.ToList()[i].Contains("ddlBank"))
				{
					namaBank = Request.Form["ddlBank"].ToString();
				}
				else if (Request.Form.AllKeys.ToList()[i].Contains("userID"))
				{
					userID = Request.Form["userID"].ToString();
				}
				else if (Request.Form.AllKeys.ToList()[i].Contains("userID"))
				{
					userID = Request.Form["userID"].ToString();
				}
				else if (Request.Form.AllKeys.ToList()[i].Contains("userID"))
				{
					userID = Request.Form["userID"].ToString();
				}
			}
			#endregion

			ViewBag.NIK = employee.employeeNIK;
			ViewBag.typeCreate = typeCreated;
			ViewBag.userID = userID;
			ViewBag.ListBankExistingOnEmployee = ccm.ddlBankEmployee(namaBank);
			ViewBag.existingBank = existingBank;
			//ViewBag.posDate = DateTime.Now;
			//ViewBag.resDate = DateTime.Now;
			//ViewBag.DeptList = ccm.ddlDepartment(string.Empty);
			//ViewBag.DivList = ccm.ddlDivisi(string.Empty);
			//ViewBag.LevelList = ccm.ddlLevel(string.Empty);
			//ViewBag.StatusList = ccm.ddlStatusEmployee(string.Empty);

			ViewBag.Gender = employee.jenisKelamin;
			ViewBag.statusNikah = ccm.statusNikah(employee.statusNikah);
			ViewBag.agama = ccm.agama(employee.agama);
			ViewBag.kewarganegaraan = ccm.kewarganegaraan(employee.kewarganegaraan);
			if (ModelState.IsValid)
			{
				List<employee.employeePosition> empPos = Session["detailPositionSession"] as List<employee.employeePosition>;
				try
				{
					using (TransactionScope ts = new TransactionScope())
					{
						string prefix = DateTime.Now.Year.ToString().Substring(2) + DateTime.Now.Month.ToString("d2");
						var nikCreated = db.employees.Where(x => x.employeeNIK.Contains(prefix)).OrderByDescending(x => x.employeeID).Select(x => x.employeeNIK).ToList();
						if (nikCreated.Count == 0)
						{
							employee.employeeNIK = prefix + "001";
						}
						else
						{
							employee.employeeNIK = prefix + (Convert.ToInt32(nikCreated[0].Substring((nikCreated[0].Length - 3))) + 1).ToString().PadLeft(3, '0');
						}

						lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
						employee.createdUser = lvm.userID;
						employee.createdDate = DateTime.Now;
						employee.bankName = existingBank == true ? namaBank : employee.bankName;
						db.employees.Add(employee);

						#region create user
						string encPassword = ccm.Encrypt(userID.Trim());

						user usr = new user();
						usr.userID = userID;
						usr.userName = employee.employeeName;
						usr.email = employee.email;
						usr.isActive = true;
						usr.password = encPassword;
						usr.deptID = 0;
						usr.createdDate = employee.createdDate;
						usr.createdUser = employee.createdUser;
						usr.employeeNIK = employee.employeeNIK;
						db.Users.Add(usr);
						#endregion

						db.SaveChanges();
						ts.Complete();
					}
					//return RedirectToAction("Index");

					var employeeID = db.employees
							  .Join(db.Users.Where(x => x.employeeNIK == employee.employeeNIK), a => a.employeeNIK, b => b.employeeNIK, (a, b) => new { a, b })
							  .Select(x => new { x.a.employeeID }).ToList();
					return RedirectToAction("Edit", "Employee", new { id = employeeID[0].employeeID });
				}
				catch (Exception exc)
				{
					string a = exc.Message;
				}
			}

			return View(employee);
		}

		// GET: /Employee/Edit/5
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
			employee employee = db.employees.Find(id);
			if (employee == null)
			{
				return HttpNotFound();
			}

			loadDetailEmployee(employee.employeeID, employee.employeeNIK, employee.bankName, employee);
			return View(employee);
		}

		// POST: /Employee/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(employee employee)
		{

			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			var b = db.Users.Where(x => x.employeeNIK == employee.employeeNIK).Select(x => new { x.userID }).ToList();
			ViewBag.userID = b[0].userID.ToString();
			ViewBag.ListBankExistingOnEmployee = ccm.ddlBankEmployee(employee.bankName);
			var c = db.employees.AsNoTracking().Where(x => x.bankName == employee.bankName).ToList();
			ViewBag.bankIsExisting = c.Count > 0 ? true : false;

			#region collect bank detail
			bool existingBank = false;
			string namaBank = string.Empty;
			for (int i = 0; i < Request.Form.Count; i++)
			{
				if (Request.Form.AllKeys.ToList()[i].Contains("existingBank"))
				{
					string[] chkExistingBank = Request.Form["existingBank"].ToString().Split(',');
					existingBank = bool.Parse(chkExistingBank[0]);
				}
				else if (Request.Form.AllKeys.ToList()[i].Contains("ddlBank"))
				{
					namaBank = Request.Form["ddlBank"].ToString();
				}
			}
			#endregion

			#region collect position & resign & (penghasilan) & family
			int countDetail = 0;
			int countChild = 0;
			int countOccupation = 0;
			List<string> listElementIdPenghasilan = new List<string>();
			List<string> listElementIdPengurangPenghasilan = new List<string>();

			for (int i = 0; i < Request.Form.Count; i++)
			{
				if (Request.Form.AllKeys.ToList()[i].Contains("posDate_"))
				{
					countDetail++;
				}
				else if (Request.Form.AllKeys.ToList()[i].Contains("penghasilan_"))
				{
					if (!listElementIdPenghasilan.Contains(Request.Form.AllKeys[i].ToString()))
						listElementIdPenghasilan.Add(Request.Form.AllKeys[i].ToString());
				}
				else if (Request.Form.AllKeys.ToList()[i].Contains("pengurangPenghasilan_"))
				{
					if (!listElementIdPengurangPenghasilan.Contains(Request.Form.AllKeys[i].ToString()))
						listElementIdPengurangPenghasilan.Add(Request.Form.AllKeys[i].ToString());
				}
				else if (Request.Form.AllKeys.ToList()[i].Contains("childName_"))
				{
					countChild++;
				}
				else if (Request.Form.AllKeys.ToList()[i].Contains("company_"))
				{
					countOccupation++;
				}
			}
			#endregion

			//ViewBag.posDate = posDate;
			ViewBag.DeptList = ccm.ddlDepartment(string.Empty);
			ViewBag.DivList = ccm.ddlDivisi(string.Empty);
			//ViewBag.posTitle = posTitle;
			ViewBag.LevelList = ccm.ddlLevel(string.Empty);
			ViewBag.StatusList = ccm.ddlStatusEmployee(string.Empty);
			ViewBag.MateStatusList = ccm.ddlMateStatus(string.Empty);

			//ViewBag.resDate = resDate;
			//ViewBag.resRemark = resRemark;
			loadDetailEmployee(employee.employeeID, employee.employeeNIK, employee.bankName, employee);

			if (ModelState.IsValid)
			{
				try
				{
					using (TransactionScope ts = new TransactionScope())
					{
						var file = Request.Files["linkFilePhoto"];
						if (file != null && file.ContentLength > 0)
						{
							string extension = Path.GetExtension(file.FileName);
							string fileNameMask = employee.employeeNIK /* +"_" + file.FileName*/+ extension;
							var imagePath = Path.Combine(Server.MapPath(ccm.uploadPhotoDir), fileNameMask);
							var imageUrl = Path.Combine(ccm.uploadPhotoDir, fileNameMask);
							file.SaveAs(imagePath);
							employee.linkFilePhoto = imageUrl;
						}
						else
						{
							var getOldLinkFilePhoto = db.employees.AsNoTracking().Where(x => x.employeeNIK == employee.employeeNIK).ToList();
							employee.linkFilePhoto = getOldLinkFilePhoto[0].linkFilePhoto;
						}

						lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
						employee.modifiedUser = lvm.userID;
						employee.modifiedDate = DateTime.Now;

						employee.bankName = existingBank == true ? namaBank : employee.bankName;
						db.Entry(employee).State = EntityState.Modified;

						#region collect position & resign & (penghasilan) & Family

						#region position & resign
						db.employeePositions.RemoveRange(db.employeePositions.Where(x => x.employeeID == employee.employeeID));
						db.employeeResigns.RemoveRange(db.employeeResigns.Where(x => x.employeeID == employee.employeeID));

						DateTime posDate = DateTime.Now;
						int posDept = 0;
						int posDiv = 0;
						string posTitle = string.Empty;
						int posLevel = 0;
						string posStatus = string.Empty;
						DateTime resDate = DateTime.Now;
						string resRemark = string.Empty;

						for (int i = 0; i < countDetail; i++)
						{
							if (!string.IsNullOrEmpty(Request.Form["posDate_" + i]))
								posDate = Convert.ToDateTime(Request.Form["posDate_" + i].ToString());
							if (!string.IsNullOrEmpty(Request.Form["posDept_" + i]))
								posDept = int.Parse(Request.Form["posDept_" + i].ToString());
							if (!string.IsNullOrEmpty(Request.Form["posDiv_" + i]))
								posDiv = int.Parse(Request.Form["posDiv_" + i].ToString());
							if (!string.IsNullOrEmpty(Request.Form["posTitle_" + i]))
								posTitle = Request.Form["posTitle_" + i].ToString();
							if (!string.IsNullOrEmpty(Request.Form["posLevel_" + i]))
								posLevel = int.Parse(Request.Form["posLevel_" + i].ToString());
							if (!string.IsNullOrEmpty(Request.Form["posStatus_" + i]))
								posStatus = Request.Form["posStatus_" + i].ToString();
							if (!string.IsNullOrEmpty(Request.Form["resDate_" + i]))
								resDate = Convert.ToDateTime(Request.Form["resDate_" + i].ToString());
							if (!string.IsNullOrEmpty(Request.Form["resRemark_" + i]))
								resRemark = Request.Form["resRemark_" + i].ToString();

							employee.employeePosition empPos = new employee.employeePosition();
							empPos.positionDate = posDate;
							empPos.employeeID = employee.employeeID;
							empPos.jobTitle = posTitle;
							empPos.deptID = posDept;
							empPos.divisiID = posDiv;
							empPos.levelID = posLevel;
							empPos.employeeStatus = posStatus;
							db.employeePositions.Add(empPos);

							if (resRemark != string.Empty)
							{
								employee.employeeResign empRes = new employee.employeeResign();
								empRes.resignDate = resDate;
								empRes.remarks = resRemark;
								empRes.employeeID = employee.employeeID;
								db.employeeResigns.Add(empRes);
							}
						}
						#endregion

						#region penghasilan
						db.transPenghasilans.RemoveRange(db.transPenghasilans.Where(x => x.employeeNIK == employee.employeeNIK));
						for (int i = 0; i < listElementIdPenghasilan.Count(); i++)
						{
							string elementId = listElementIdPenghasilan[i];
							string[] arrayId = elementId.Split('_');
							decimal amount = decimal.Parse(Request.Form[elementId].ToString());
							int idPenghasilan = int.Parse(arrayId[1].ToString());

							transPenghasilan tPenghasilan = new transPenghasilan();
							tPenghasilan.employeeNIK = employee.employeeNIK;
							tPenghasilan.idPenghasilan = idPenghasilan;
							tPenghasilan.amount = amount;
							tPenghasilan.modifiedDate = DateTime.Now;
							tPenghasilan.modifiedUser = lvm.userID;
							db.transPenghasilans.Add(tPenghasilan);
						}
						#endregion

						#region Pengurang Penghasilan
						db.transPengurangPenghasilans.RemoveRange(db.transPengurangPenghasilans.Where(x => x.employeeNIK == employee.employeeNIK));
						for (int i = 0; i < listElementIdPengurangPenghasilan.Count(); i++)
						{
							string elementId = listElementIdPengurangPenghasilan[i];
							string[] arrayId = elementId.Split('_');
							decimal amount = decimal.Parse(Request.Form[elementId].ToString());
							int idPengurangPenghasilan = int.Parse(arrayId[1].ToString());

							transPengurangPenghasilan tPengurangPenghasilan = new transPengurangPenghasilan();
							tPengurangPenghasilan.employeeNIK = employee.employeeNIK;
							tPengurangPenghasilan.idPengurangPenghasilan = idPengurangPenghasilan;
							tPengurangPenghasilan.amount = amount;
							tPengurangPenghasilan.modifiedDate = DateTime.Now;
							tPengurangPenghasilan.modifiedUser = lvm.userID;
							db.transPengurangPenghasilans.Add(tPengurangPenghasilan);
						}
						#endregion

						#region Family
						db.employeeMates.RemoveRange(db.employeeMates.Where(x => x.employeeID == employee.employeeID));
						db.employeeChilds.RemoveRange(db.employeeChilds.Where(x => x.employeeID == employee.employeeID));

						string coupleName = Request.Form["coupleName_"].ToString();
						DateTime coupleBirthDate = DateTime.Parse(Request.Form["coupleBirthDate_"].ToString());
						string coupleBirthPlace = Request.Form["coupleBirthPlace_"].ToString();
						string coupleLastEducation = Request.Form["coupleLastEducation_"].ToString();
						string coupleOccupation = Request.Form["coupleOccupation_"].ToString();
						string coupleStatus = Request.Form["mateStatus_"].ToString();
						string coupleGender = Request.Form["mateGender_"].ToString();

						if (coupleName != string.Empty)
						{
							employee.employeeMate empMate = new employee.employeeMate();
							empMate.employeeID = employee.employeeID;
							empMate.coupleName = coupleName;
							empMate.coupleBirthDate = coupleBirthDate;
							empMate.coupleBirthPlace = coupleBirthPlace;
							empMate.lastEducation = coupleLastEducation;
							empMate.occupation = coupleOccupation;
							empMate.status = coupleStatus;
							empMate.gender = coupleGender;
							db.employeeMates.Add(empMate);
						}

						string childName = string.Empty;
						DateTime childBirthDate = DateTime.Now;
						string childBirthPlace = string.Empty;
						string childLastEducation = string.Empty;
						string childGender = string.Empty;

						for (int i = 0; i < countChild; i++)
						{
							childName = Request.Form["childName_" + i].ToString();
							childBirthDate = Convert.ToDateTime(Request.Form["childBirthDate_" + i].ToString());
							childBirthPlace = Request.Form["childBirthPlace_" + i].ToString();
							childLastEducation = Request.Form["childLastEducation_" + i].ToString();
							childGender = Request.Form["childGender_" + i].ToString();

							if (childName != string.Empty)
							{
								employee.employeeChild empChild = new employee.employeeChild();
								empChild.childName = childName;
								empChild.childBirthDate = childBirthDate;
								empChild.childBirthPlace = childBirthPlace;
								empChild.lastEducation = childLastEducation;
								empChild.employeeID = employee.employeeID;
								empChild.gender = childGender;
								db.employeeChilds.Add(empChild);
							}
						}
						#endregion

						#region Occupation
						var fileCV = Request.Files["linkFileCV_"];

						db.employeeCVs.RemoveRange(db.employeeCVs.Where(x => x.employeeID == employee.employeeID));

						employee.employeeCV empCV = new employee.employeeCV();
						empCV.employeeID = employee.employeeID;

						if (fileCV != null && fileCV.ContentLength > 0 && String.IsNullOrEmpty(Request.Form["hdnlinkFileCV_"].ToString()))
						{
							string extension = Path.GetExtension(fileCV.FileName);
							string fileNameMask = employee.employeeNIK + "_cv" /* +"_" + fileCV.FileName*/+ extension;
							var imagePath = Path.Combine(Server.MapPath(ccm.uploadDir), fileNameMask);
							var imageUrl = Path.Combine(ccm.uploadDir, fileNameMask);
							fileCV.SaveAs(imagePath);
							empCV.linkFileCV = imageUrl;
						}
						else if (!String.IsNullOrEmpty(Request.Form["hdnlinkFileCV_"].ToString()))
						{
							var getOldLinkFilePhoto = db.employeeCVs.AsNoTracking().Where(x => x.employeeID == employee.employeeID).ToList();
							if (getOldLinkFilePhoto.Count() > 0)
								empCV.linkFileCV = getOldLinkFilePhoto[0].linkFileCV;
						}
						db.employeeCVs.Add(empCV);

						db.employeeEducations.RemoveRange(db.employeeEducations.Where(x => x.employeeID == employee.employeeID));
						db.employeeOccupations.RemoveRange(db.employeeOccupations.Where(x => x.employeeID == employee.employeeID));

						string sdNamaSekolah_ = Request.Form["sdNamaSekolah_"].ToString();
						string sdKotaSekolah_ = Request.Form["sdKotaSekolah_"].ToString();
						string sdJurusanSekolah_ = Request.Form["sdJurusanSekolah_"].ToString();
						string sdPeriodStart_ = Request.Form["sdPeriodStart_"].ToString();
						string sdPeriodEnd_ = Request.Form["sdPeriodEnd_"].ToString();
						string sdKeterangan_ = Request.Form["sdKeterangan_"].ToString();

						string smpNamaSekolah_ = Request.Form["smpNamaSekolah_"].ToString();
						string smpKotaSekolah_ = Request.Form["smpKotaSekolah_"].ToString();
						string smpJurusanSekolah_ = Request.Form["smpJurusanSekolah_"].ToString();
						string smpPeriodStart_ = Request.Form["smpPeriodStart_"].ToString();
						string smpPeriodEnd_ = Request.Form["smpPeriodEnd_"].ToString();
						string smpKeterangan_ = Request.Form["smpKeterangan_"].ToString();

						string smaNamaSekolah_ = Request.Form["smaNamaSekolah_"].ToString();
						string smaKotaSekolah_ = Request.Form["smaKotaSekolah_"].ToString();
						string smaJurusanSekolah_ = Request.Form["smaJurusanSekolah_"].ToString();
						string smaPeriodStart_ = Request.Form["smaPeriodStart_"].ToString();
						string smaPeriodEnd_ = Request.Form["smaPeriodEnd_"].ToString();
						string smaKeterangan_ = Request.Form["smaKeterangan_"].ToString();

						string akademiNamaSekolah_ = Request.Form["akademiNamaSekolah_"].ToString();
						string akademiKotaSekolah_ = Request.Form["akademiKotaSekolah_"].ToString();
						string akademiJurusanSekolah_ = Request.Form["akademiJurusanSekolah_"].ToString();
						string akademiPeriodStart_ = Request.Form["akademiPeriodStart_"].ToString();
						string akademiPeriodEnd_ = Request.Form["akademiPeriodEnd_"].ToString();
						string akademiKeterangan_ = Request.Form["akademiKeterangan_"].ToString();

						string pascaNamaSekolah_ = Request.Form["pascaNamaSekolah_"].ToString();
						string pascaKotaSekolah_ = Request.Form["pascaKotaSekolah_"].ToString();
						string pascaJurusanSekolah_ = Request.Form["pascaJurusanSekolah_"].ToString();
						string pascaPeriodStart_ = Request.Form["pascaPeriodStart_"].ToString();
						string pascaPeriodEnd_ = Request.Form["pascaPeriodEnd_"].ToString();
						string pascaKeterangan_ = Request.Form["pascaKeterangan_"].ToString();

						employee.employeeEducation empEducation = new employee.employeeEducation();

							empEducation.sdNamaSekolah = sdNamaSekolah_;
							empEducation.sdKotaSekolah = sdKotaSekolah_;
							empEducation.sdJurusanSekolah = sdJurusanSekolah_;
							empEducation.sdPeriod = sdPeriodStart_ + "-" + sdPeriodEnd_;
							empEducation.sdKeterangan = sdKeterangan_;

							empEducation.smpNamaSekolah = smpNamaSekolah_;
							empEducation.smpKotaSekolah = smpKotaSekolah_;
							empEducation.smpJurusanSekolah = smpJurusanSekolah_;
							empEducation.smpPeriod = smpPeriodStart_ + "-" + smpPeriodEnd_;
							empEducation.smpKeterangan = smpKeterangan_;

							empEducation.smaNamaSekolah = smaNamaSekolah_;
							empEducation.smaKotaSekolah = smaKotaSekolah_;
							empEducation.smaJurusanSekolah = smaJurusanSekolah_;
							empEducation.smaPeriod = smaPeriodStart_ + "-" + smaPeriodEnd_;
							empEducation.smaKeterangan = smaKeterangan_;

							empEducation.akademiNamaSekolah = akademiNamaSekolah_;
							empEducation.akademiKotaSekolah = akademiKotaSekolah_;
							empEducation.akademiJurusanSekolah = akademiJurusanSekolah_;
							empEducation.akademiPeriod = akademiPeriodStart_ + "-" + akademiPeriodEnd_;
							empEducation.akademiKeterangan = akademiKeterangan_;

							empEducation.pascaNamaSekolah = pascaNamaSekolah_;
							empEducation.pascaKotaSekolah = pascaKotaSekolah_;
							empEducation.pascaJurusanSekolah = pascaJurusanSekolah_;
							empEducation.pascaPeriod = pascaPeriodStart_ + "-" + pascaPeriodEnd_;
							empEducation.pascaKeterangan = pascaKeterangan_;
						
						if (sdNamaSekolah_ != string.Empty || 
							smpNamaSekolah_ != string.Empty ||
							smaNamaSekolah_ != string.Empty || 
							akademiNamaSekolah_ != string.Empty || 
							pascaNamaSekolah_ != string.Empty)
						{
							empEducation.employeeID = employee.employeeID;
							db.employeeEducations.Add(empEducation);
						}

						DateTime? periodStart = null;
						DateTime? periodEnd = null;
						string company_ = string.Empty;
						string jobdesc_ = string.Empty;

						for (int i = 0; i < countOccupation; i++)
						{
							if (!String.IsNullOrEmpty(Request.Form["periodStartMonth_" + i]) && !String.IsNullOrEmpty(Request.Form["periodStartYear_" + i]))
							{
								string year = int.Parse(Request.Form["periodStartYear_" + i].ToString()) < int.Parse(DateTime.MinValue.Year.ToString()) ? DateTime.MinValue.Year.ToString() : Request.Form["periodStartYear_" + i].ToString();
								string date = year + "-" + Request.Form["periodStartMonth_" + i].ToString() + "-" + "01";
								periodStart = Convert.ToDateTime(date);
							}

							if (!String.IsNullOrEmpty(Request.Form["periodEndMonth_" + i]) && !String.IsNullOrEmpty(Request.Form["periodEndYear_" + i]))
							{
								string year = int.Parse(Request.Form["periodEndYear_" + i].ToString()) < int.Parse(DateTime.MinValue.Year.ToString()) ? DateTime.MinValue.Year.ToString() : Request.Form["periodEndYear_" + i].ToString();
								string date = year + "-" + Request.Form["periodEndMonth_" + i].ToString() + "-" + "01";
								periodEnd = Convert.ToDateTime(date);
							}

							company_ = Request.Form["company_" + i].ToString();
							jobdesc_ = Request.Form["jobdesc_" + i].ToString();

							if (company_ != string.Empty)
							{
								employee.employeeOccupation empOccupation = new employee.employeeOccupation();
								empOccupation.employeeID = employee.employeeID;
								empOccupation.sId = i;
								empOccupation.periodStart = periodStart;
								empOccupation.periodEnd = periodEnd;
								empOccupation.company = company_;
								empOccupation.jobdesc = jobdesc_;
								db.employeeOccupations.Add(empOccupation);
							}
						}
						#endregion

						#endregion

						var getUser = db.Users.AsNoTracking().Where(x => x.employeeNIK == employee.employeeNIK).ToList();
						user users = db.Users.Find(getUser[0].userID);
						users.deptID = posDiv;
						var buchy = "tampan";
						db.Entry(users).State = EntityState.Modified;

						db.SaveChanges();
						ts.Complete();
					}

					return RedirectToAction("Index");
				}
				catch (Exception exc)
				{
					string a = exc.Message;
					var bucho = 0;
				}
			}

			var errors = ModelState.Values.SelectMany(v => v.Errors);
			var buchi = 0;
			return View(employee);
		}

		// GET: /Employee/Delete/5
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
			employee employee = db.employees.Find(id);
			if (employee == null)
			{
				return HttpNotFound();
			}

			loadDetailEmployee(employee.employeeID, employee.employeeNIK, employee.bankName, employee);
			return View(employee);
		}

		// POST: /Employee/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			employee employee = db.employees.Find(id);
			db.employees.Remove(employee);
			db.employeeResigns.RemoveRange(db.employeeResigns.Where(x => x.employeeID == employee.employeeID));
			db.employeePositions.RemoveRange(db.employeePositions.Where(x => x.employeeID == employee.employeeID));
			db.Users.RemoveRange(db.Users.Where(x => x.employeeNIK == employee.employeeNIK));
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

		public void loadDetailEmployee(int employeeID, string employeeNIK, string bankName, employee employee)
		{
			var b = db.Users.AsNoTracking().Where(x => x.employeeNIK == employeeNIK).Select(x => new { x.userID }).ToList();
			ViewBag.userID = b[0].userID.ToString();
			ViewBag.ListBankExistingOnEmployee = ccm.ddlBankEmployee(bankName);
			var a = db.employees.AsNoTracking().Where(x => x.bankName == bankName).ToList();
			ViewBag.bankIsExisting = a.Count > 0 ? true : false;

			var c = db.employees.AsNoTracking().Where(x => x.employeeNIK == employeeNIK).ToList();
			ViewBag.PhotoDir = c[0].linkFilePhoto == null ? null : c[0].linkFilePhoto;

			ViewBag.Gender = employee.jenisKelamin;
			ViewBag.statusNikahs = ccm.statusNikah(employee.statusNikah);
			ViewBag.agamas = ccm.agama(employee.agama);
			ViewBag.kewarganegaraans = ccm.kewarganegaraan(employee.kewarganegaraan);

			//ViewBag.posDate = DateTime.Now;
			ViewBag.DeptList = ccm.ddlDepartment(string.Empty);
			//ViewBag.posTitle = string.Empty;
			ViewBag.LevelList = ccm.ddlLevel(string.Empty);
			ViewBag.StatusList = ccm.ddlStatusEmployee(string.Empty);

			//ViewBag.resDate = DateTime.Now;
			//ViewBag.resRemark = string.Empty;


			var model = new employee();
			foreach (var empPos in db.employeePositions.AsNoTracking().Where(x => x.employeeID == employeeID).ToList() as IEnumerable<Models.employee.employeePosition>)
			{
				var editor = new employee.employeePosition()
				{
					positionDate = empPos.positionDate,
					deptID = empPos.deptID,
					divisiID = empPos.divisiID,
					jobTitle = empPos.jobTitle,
					levelID = empPos.levelID,
					employeeStatus = empPos.employeeStatus
				};
				model.positionEmployee.Add(editor);
			}
			ViewBag.detailPosition = model.positionEmployee.Count == 0 ? null : model.positionEmployee.ToList();

			foreach (var empRes in db.employeeResigns.AsNoTracking().Where(x => x.employeeID == employeeID).ToList() as IEnumerable<Models.employee.employeeResign>)
			{
				var editor = new employee.employeeResign()
				{
					resignDate = empRes.resignDate,
					remarks = empRes.remarks
				};
				model.resignEmployee.Add(editor);
			}
			ViewBag.detailResign = model.resignEmployee.Count == 0 ? null : model.resignEmployee.ToList();

			if (model.positionEmployee.Count > 0)
				ViewBag.DivList = ccm.ddlDivisi(string.Empty);
			else
				ViewBag.DivList = null;

			#region Penghasilan & Pengurang Penghasilan
			var transPenghasilan = db.transPenghasilans.AsNoTracking().Where(x => x.employeeNIK == employeeNIK).ToList();
			if (transPenghasilan.Count > 0)
				ViewBag.penghasilan = transPenghasilan;
			ViewBag.masterPenghasilan = db.penghasilans.AsNoTracking().ToList();

			var transPengurangPenghasilan = db.transPengurangPenghasilans.AsNoTracking().Where(x => x.employeeNIK == employeeNIK).ToList();
			if (transPengurangPenghasilan.Count > 0)
				ViewBag.pengurangPenghasilan = transPengurangPenghasilan;
			ViewBag.masterPengurangPenghasilan = db.pengurangPenghasilans.AsNoTracking().ToList();
			#endregion

			#region Family
			ViewBag.MateStatusList = ccm.ddlMateStatus(string.Empty);
			ViewBag.genderList = ccm.genders(string.Empty);

			foreach (var empMate in db.employeeMates.Where(x => x.employeeID == employeeID).ToList() as IEnumerable<Models.employee.employeeMate>)
			{
				var editor = new employee.employeeMate()
				{
					coupleName = empMate.coupleName,
					coupleBirthDate = empMate.coupleBirthDate,
					coupleBirthPlace = empMate.coupleBirthPlace,
					lastEducation = empMate.lastEducation,
					occupation = empMate.occupation,
					status = empMate.status,
					gender = empMate.gender
				};
				model.mateEmployee.Add(editor);
			}
			ViewBag.detailMate = model.mateEmployee.Count == 0 ? null : model.mateEmployee.ToList();

			foreach (var empChild in db.employeeChilds.Where(x => x.employeeID == employeeID).ToList() as IEnumerable<Models.employee.employeeChild>)
			{
				var editor = new employee.employeeChild()
				{
					childName = empChild.childName,
					childBirthDate = empChild.childBirthDate,
					childBirthPlace = empChild.childBirthPlace,
					lastEducation = empChild.lastEducation,
					gender = empChild.gender
				};
				model.childEmployee.Add(editor);
			}
			ViewBag.detailChild = model.childEmployee.Count == 0 ? null : model.childEmployee.ToList();

			#endregion

			#region CV
			var cv = db.employeeCVs.AsNoTracking().Where(x => x.employeeID == employeeID).ToList();
			if (cv.Count() > 0)
				ViewBag.detailCV = cv;

			var education = db.employeeEducations.AsNoTracking().Where(x => x.employeeID == employeeID).ToList();
			if (education.Count() > 0)
				ViewBag.detailEducation = education;

			var occupation = db.employeeOccupations.AsNoTracking().Where(x => x.employeeID == employeeID).ToList();
			if (occupation.Count() > 0)
				ViewBag.detailOccupation = occupation;
			#endregion
		}
	}
}
