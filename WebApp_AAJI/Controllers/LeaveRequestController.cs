using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
	public class LeaveRequestController : Controller
	{
		private MyDataAccess db = new MyDataAccess();
		private AccountController acm = new AccountController();
		LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
		private ConnectionController con = new ConnectionController();
		int maxRows = 10;

		[HttpGet]
		public async Task<ActionResult> LoadDetailEmployeeSearch(string keywordId, string keywordSearch /*, int maxRows*/, int pageStart)
		{
			loadPopUpResult(keywordId, keywordSearch, pageStart);

			return PartialView("_PartialPageEmployeeSearchSub");
		}

		public void loadPopUpResult(string keywordId, string keywordSearch, int pageStart)
		{
			////[employeeID],[employeeName],[employeeNIK],[email],[npwp],[ktp],[address],[city],[bankName],[rekening],[tempatLahir],[tanggalLahir]
			////,[linkFilePhoto],[positionDate],[positionId],[resignDate],[divisiID],[deptName],[deptID],[divisiName],[gapok],[levelID],[employeeStatus]
			string sql = "WITH cte AS ( ";
			sql += "SELECT ROW_NUMBER() OVER(ORDER BY employeeID) AS rn, ";
			sql += "[employeeID], [employeeNIK], [employeeName], [deptName], [employeeStatus] ";
			sql += "FROM [dbo].[v_employee] b  WITH(NOLOCK) ";
			sql += "WHERE b.resignDate IS NULL ";

			if (!string.IsNullOrEmpty(keywordId) && !string.IsNullOrEmpty(keywordSearch))
			{
				if (keywordId == "employeeStatus")
					sql += "AND " + keywordId + " = " + keywordSearch;
				else
					sql += "AND " + keywordId + " LIKE '%" + keywordSearch + "%'";
			}
			sql += ") ";
			sql += "SELECT * FROM cte ";
			string sqlAll = sql;
			sql += "WHERE rn BETWEEN " + ((pageStart * maxRows) - (maxRows - 1)) + " AND " + (pageStart * maxRows);

			DataTable dataAll = con.executeReader(sqlAll);
			ViewBag.totalRows = dataAll.Rows.Count;
			ViewBag.currentPage = pageStart;

			DataTable popUp = con.executeReader(sql);
			ViewBag.resultPopUp = popUp;
		}

		// GET: /LeaveRequest/
		public ActionResult Index()
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			return View(db.transLeaveRequests.ToList());
		}

		// GET: /LeaveRequest/Details/5
		public ActionResult Details(int? id, int empId)
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
			var tlq = db.transLeaveRequests.Where(x => x.leaveRequestID == id && x.employeeID == empId).ToList();
			if (tlq == null)
			{
				return HttpNotFound();
			}

			transLeaveRequest transleaverequest = new transLeaveRequest();
			foreach (var a in tlq as IEnumerable<transLeaveRequest>)
			{
				transleaverequest.acknowledgeBy = a.acknowledgeBy;
				transleaverequest.acknowledgeDate = a.acknowledgeDate;
				transleaverequest.acknowledgeStatus = a.acknowledgeStatus;
				transleaverequest.approvedBy = a.approvedBy;
				transleaverequest.approvedDate = a.approvedDate;
				transleaverequest.approvedStatus = a.approvedStatus;
				transleaverequest.createdDate = a.createdDate;
				transleaverequest.createdUser = a.createdUser;
				transleaverequest.employeeID = a.employeeID;
				transleaverequest.category = a.category;
				transleaverequest.cutiInHoliday = a.cutiInHoliday;
				transleaverequest.leaveDescription = a.leaveDescription;
				transleaverequest.leaveEndDate = a.leaveEndDate;
				transleaverequest.leaveRequestID = a.leaveRequestID;
				transleaverequest.leaveStartDate = a.leaveStartDate;
				transleaverequest.modifiedDate = a.modifiedDate;
				transleaverequest.modifiedUser = a.modifiedUser;

				var employeeName = db.employees.Where(x => x.employeeID == a.employeeID).ToList();
				ViewBag.employeeName = employeeName[0].employeeName;
			}
			return View(transleaverequest);
		}

		// GET: /LeaveRequest/Create
		public ActionResult Create()
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			var DataCuti = new ViewModelCuti();

			DataCuti.employeeID = lvm.employeeId;
			DataCuti.employeeName = string.Empty;
			if (lvm.isAdmin == true)
				loadPopUpResult(string.Empty, string.Empty, 1);
			else
			{
				DataCuti.employeeID = lvm.employeeId;
				DataCuti.employeeName = lvm.employeeName;

				var posistion = db.employeePositions.Where(x => x.employeeID == lvm.employeeId).ToList();
				var level = posistion.LastOrDefault().levelID;
				DataCuti.level = level;

				takeDataCuti(DataCuti);

			}
			return View(DataCuti);
		}

		private void takeDataCuti(ViewModelCuti datacuti)
		{
			var masterLevel = db.levels.Where(x => x.levelID == datacuti.level).Single();
			var sisaCuti = db.sisaSaldoCutis.Where(x => x.employeeID == lvm.employeeId && x.year == DateTime.Now.Year).Single();

			datacuti.saldoCuti = masterLevel.saldoCuti;
			datacuti.saldoCuti2 = masterLevel.saldoCuti2;
			datacuti.saldoCuti3 = masterLevel.saldoCuti3;
			datacuti.saldoCuti4 = masterLevel.saldoCuti4;

			datacuti.amount = sisaCuti.amount;
			datacuti.amount2 = sisaCuti.amount2;
			datacuti.amount3 = sisaCuti.amount3;
			datacuti.amount4 = sisaCuti.amount4;

			datacuti.countedApproved = sisaCuti.countedApproved;
			datacuti.countedApproved2 = sisaCuti.countedApproved2;
			datacuti.countedApproved3 = sisaCuti.countedApproved3;
			datacuti.countedApproved4 = sisaCuti.countedApproved4;
		}

		// POST: /LeaveRequest/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create(ViewModelCuti dataCuti)
		{
			var buch = "tampan";
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			var timespan = dataCuti.leaveEndDate - dataCuti.leaveStartDate;
			var JumlahPermintaan = timespan.Days + 1; //belum dihitung hari libur

			int cutidiharilibur = FindCutiInHoliday(dataCuti.leaveStartDate, dataCuti.leaveEndDate); //jumlah cuti di hari libur pada tanggal yg diminta

			dataCuti.cutiInHoliday = cutidiharilibur;
			JumlahPermintaan = JumlahPermintaan - cutidiharilibur;
			
			var jumsisacuti = 0;
			if (dataCuti.category.Equals("tahunan"))
			{
				jumsisacuti = dataCuti.amount;
			}
			else if (dataCuti.category.Equals("persalinan"))
			{
				jumsisacuti = dataCuti.amount2;
			}
			else if (dataCuti.category.Equals("kemalangan"))
			{
				jumsisacuti = dataCuti.amount3;
			}
			else if (dataCuti.category.Equals("lain-lain"))
			{
				jumsisacuti = dataCuti.amount4;
			}


			if (JumlahPermintaan > jumsisacuti)
			{
				ModelState.AddModelError("leaveEndDate", "Jatah cuti tidak mencukupi permintaan anda");
			}

			if (DateTime.Compare(dataCuti.leaveEndDate, dataCuti.leaveStartDate) < 0)
			{
				ModelState.AddModelError("leaveEndDate", "Tanggal akhir tidak boleh kurang dari Tanggal awal");
			}

			if (ModelState.IsValid)
			{
				var CutiRequest = new transLeaveRequest
				{
					createdDate = DateTime.Now,
					createdUser = lvm.userID,
					employeeID = dataCuti.employeeID,
					leaveStartDate = dataCuti.leaveStartDate,
					leaveEndDate = dataCuti.leaveEndDate,
					category = dataCuti.category,
					leaveDescription = dataCuti.leaveDescription,
					cutiInHoliday = dataCuti.cutiInHoliday
				};
				db.transLeaveRequests.Add(CutiRequest);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(dataCuti);
		}

		private int FindCutiInHoliday(DateTime StartDate, DateTime EndDate)
		{
			var listholiday = db.holidays.ToList();
			int cutidiharilibur = 0;

			var timespan = EndDate - StartDate;
			var JumlahPermintaan = timespan.Days + 1; //belum dihitung hari libur

			foreach (var item2 in listholiday)
			{
				if (DateTime.Compare(StartDate, item2.holidayDate) <= 0)
				{
					if (DateTime.Compare(item2.holidayDate, EndDate) <= 0)
					{
						cutidiharilibur++;
						if (item2.holidayDate.DayOfWeek == DayOfWeek.Saturday ||
							item2.holidayDate.DayOfWeek == DayOfWeek.Sunday)
						{
							cutidiharilibur--;
						}
					}
				}
			}
			for (int b = 0; b < JumlahPermintaan; b++)
			{
				var testDate = StartDate.AddDays(b);
				switch (testDate.DayOfWeek)
				{
					case DayOfWeek.Saturday:
					case DayOfWeek.Sunday:
						cutidiharilibur++;
						break;
				}
			}
			return cutidiharilibur;
		}

		// GET: /LeaveRequest/Edit/5
		public ActionResult Edit(int? id, int empId)
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
			var tlq = db.transLeaveRequests.Where(x => x.leaveRequestID == id && x.employeeID == empId).ToList();
			if (tlq == null)
			{
				return HttpNotFound();
			}

			transLeaveRequest transleaverequest = new transLeaveRequest();
			foreach (var a in tlq as IEnumerable<transLeaveRequest>)
			{
				transleaverequest.acknowledgeBy = a.acknowledgeBy;
				transleaverequest.acknowledgeDate = a.acknowledgeDate;
				transleaverequest.acknowledgeStatus = a.acknowledgeStatus;
				transleaverequest.approvedBy = a.approvedBy;
				transleaverequest.approvedDate = a.approvedDate;
				transleaverequest.approvedStatus = a.approvedStatus;
				transleaverequest.createdDate = a.createdDate;
				transleaverequest.createdUser = a.createdUser;
				transleaverequest.employeeID = a.employeeID;
				transleaverequest.leaveDescription = a.leaveDescription;
				transleaverequest.category = a.category;
				transleaverequest.cutiInHoliday = a.cutiInHoliday;
				transleaverequest.leaveEndDate = a.leaveEndDate;
				transleaverequest.leaveRequestID = a.leaveRequestID;
				transleaverequest.leaveStartDate = a.leaveStartDate;
				transleaverequest.modifiedDate = a.modifiedDate;
				transleaverequest.modifiedUser = a.modifiedUser;

				var employeeName = db.employees.Where(x => x.employeeID == a.employeeID).ToList();
				ViewBag.employeeName = employeeName[0].employeeName;
			}
			return View(transleaverequest);
		}

		// POST: /LeaveRequest/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit(transLeaveRequest transleaverequest)
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			if (ModelState.IsValid)
			{
				var originTLQ = db.transLeaveRequests.Where(x => x.leaveRequestID == transleaverequest.leaveRequestID).Single();

				originTLQ.modifiedDate = DateTime.Now;
				originTLQ.modifiedUser = lvm.userID;

				originTLQ.category = transleaverequest.category;
				originTLQ.leaveDescription = transleaverequest.leaveDescription;

				if (transleaverequest.approvedStatus != null &&
					transleaverequest.approvedStatus != originTLQ.approvedStatus)
				{
					originTLQ.approvedDate = DateTime.Now;
					originTLQ.approvedBy = lvm.userID;
					originTLQ.approvedStatus = transleaverequest.approvedStatus;
					if (originTLQ.approvedStatus == true)
					{
						var SisaCuti = db.sisaSaldoCutis.Where(x => x.employeeID == lvm.employeeId && x.year == DateTime.Now.Year).Single();

						var timespan = originTLQ.leaveEndDate - originTLQ.leaveStartDate;
						var JumlahPermintaan = timespan.Days + 1; //belum dihitung hari libur

						if (originTLQ.leaveStartDate == transleaverequest.leaveStartDate &&
							originTLQ.leaveEndDate == transleaverequest.leaveEndDate)
						{
							int cutiYgdisetujui = JumlahPermintaan - originTLQ.cutiInHoliday;
							editSisaCuti(SisaCuti, cutiYgdisetujui, originTLQ.category);
						}
						else
						{
							int cutiDiharilibur = FindCutiInHoliday(transleaverequest.leaveStartDate, transleaverequest.leaveEndDate);
							int cutiYgdisetujui = JumlahPermintaan - cutiDiharilibur;
							originTLQ.cutiInHoliday = cutiDiharilibur;
							editSisaCuti(SisaCuti, cutiYgdisetujui, originTLQ.category);
						}

						db.Entry(SisaCuti).State = EntityState.Modified;
						db.SaveChanges();
					}
				}

				if (transleaverequest.acknowledgeStatus != null &&
					transleaverequest.acknowledgeStatus != originTLQ.acknowledgeStatus)
				{
					originTLQ.acknowledgeDate = DateTime.Now;
					originTLQ.acknowledgeBy = lvm.userID;
					originTLQ.acknowledgeStatus = transleaverequest.acknowledgeStatus;
				}

				originTLQ.leaveStartDate = transleaverequest.leaveStartDate;
				originTLQ.leaveEndDate = transleaverequest.leaveEndDate;

				db.Entry(originTLQ).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			else
			{
				var employeeName = db.employees.Where(x => x.employeeID == transleaverequest.employeeID).ToList();
				ViewBag.employeeName = employeeName[0].employeeName;
			}
			return View(transleaverequest);
		}

		private static void editSisaCuti(sisaSaldoCuti SisaCuti, int cutiYgdisetujui, string kategori)
		{
			if (kategori.Equals("tahunan"))
			{
				SisaCuti.countedApproved = SisaCuti.countedApproved + cutiYgdisetujui;
				SisaCuti.amount = SisaCuti.amount - cutiYgdisetujui;
			}
			else if (kategori.Equals("persalinan"))
			{
				SisaCuti.countedApproved2 = SisaCuti.countedApproved2 + cutiYgdisetujui;
				SisaCuti.amount2 = SisaCuti.amount2 - cutiYgdisetujui;
			}
			else if (kategori.Equals("kemalangan"))
			{
				SisaCuti.countedApproved3 = SisaCuti.countedApproved3 + cutiYgdisetujui;
				SisaCuti.amount3 = SisaCuti.amount3 - cutiYgdisetujui;
			}
			else if (kategori.Equals("lain-lain"))
			{
				SisaCuti.countedApproved4 = SisaCuti.countedApproved4 + cutiYgdisetujui;
				SisaCuti.amount4 = SisaCuti.amount4 - cutiYgdisetujui;
			}
			
		}

		// GET: /LeaveRequest/Delete/5
		public ActionResult Delete(int? id, int empId)
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

			var tlq = db.transLeaveRequests.Where(x => x.leaveRequestID == id && x.employeeID == empId).ToList();
			if (tlq == null)
			{
				return HttpNotFound();
			}

			transLeaveRequest transleaverequest = new transLeaveRequest();
			foreach (var a in tlq as IEnumerable<transLeaveRequest>)
			{
				transleaverequest.acknowledgeBy = a.acknowledgeBy;
				transleaverequest.acknowledgeDate = a.acknowledgeDate;
				transleaverequest.acknowledgeStatus = a.acknowledgeStatus;
				transleaverequest.approvedBy = a.approvedBy;
				transleaverequest.approvedDate = a.approvedDate;
				transleaverequest.approvedStatus = a.approvedStatus;
				transleaverequest.createdDate = a.createdDate;
				transleaverequest.createdUser = a.createdUser;
				transleaverequest.employeeID = a.employeeID;
				transleaverequest.leaveDescription = a.leaveDescription;
				transleaverequest.category = a.category;
				transleaverequest.cutiInHoliday = a.cutiInHoliday;
				transleaverequest.leaveEndDate = a.leaveEndDate;
				transleaverequest.leaveRequestID = a.leaveRequestID;
				transleaverequest.leaveStartDate = a.leaveStartDate;
				transleaverequest.modifiedDate = a.modifiedDate;
				transleaverequest.modifiedUser = a.modifiedUser;

				var employeeName = db.employees.Where(x => x.employeeID == a.employeeID).ToList();
				ViewBag.employeeName = employeeName[0].employeeName;
			}
			return View(transleaverequest);
		}

		// POST: /LeaveRequest/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id, int empId)
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			db.transLeaveRequests.RemoveRange(db.transLeaveRequests.Where(x => x.employeeID == empId && x.leaveRequestID == id));
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

		public ActionResult Approval(int? id, int empId)
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;

			ViewBag.ApprovalAuth = false;
			ViewBag.AcknowledgeAuth = false;
			foreach (string b in lvm.listAuthorize as List<string>)
			{
				string[] auth = b.Split('_');
				if (auth[0].ToString() == "A1" || auth[0].ToString() == "A2")
					if (b.Contains((Url.Action("")).Replace("/", "")))
					{
						if (auth[0].ToString() == "A1")
							ViewBag.ApprovalAuth = true;
						else if (auth[0].ToString() == "A2")
						{
							ViewBag.ApprovalAuth = true;
							ViewBag.AcknowledgeAuth = true;
						}
					}
			}
			if (ViewBag.ApprovalAuth == false && lvm.isAdmin == true)
				ViewBag.ApprovalAuth = true;

			if (ViewBag.ApprovalAuth == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}

			var tlq = db.transLeaveRequests.Where(x => x.leaveRequestID == id && x.employeeID == empId).ToList();
			if (tlq == null)
			{
				return HttpNotFound();
			}

			transLeaveRequest transleaverequest = new transLeaveRequest();
			foreach (var a in tlq as IEnumerable<transLeaveRequest>)
			{
				transleaverequest.acknowledgeBy = a.acknowledgeBy;
				transleaverequest.acknowledgeDate = a.acknowledgeDate;
				transleaverequest.acknowledgeStatus = a.acknowledgeStatus;
				transleaverequest.approvedBy = a.approvedBy;
				transleaverequest.approvedDate = a.approvedDate;
				transleaverequest.approvedStatus = a.approvedStatus;
				transleaverequest.createdDate = a.createdDate;
				transleaverequest.createdUser = a.createdUser;
				transleaverequest.employeeID = a.employeeID;
				transleaverequest.leaveDescription = a.leaveDescription;
				transleaverequest.category = a.category;
				transleaverequest.cutiInHoliday = a.cutiInHoliday;
				transleaverequest.leaveEndDate = a.leaveEndDate;
				transleaverequest.leaveRequestID = a.leaveRequestID;
				transleaverequest.leaveStartDate = a.leaveStartDate;
				transleaverequest.modifiedDate = a.modifiedDate;
				transleaverequest.modifiedUser = a.modifiedUser;

				var employeeName = db.employees.Where(x => x.employeeID == a.employeeID).ToList();
				ViewBag.employeeName = employeeName[0].employeeName;
			}
			return View(transleaverequest);
		}

		[HttpGet]
		public async Task<ActionResult> ApprovalProcess(string act, int id, int empID)
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;

			string authorizer = string.Empty;
			ViewBag.ApprovalAuth = false;
			ViewBag.AcknowledgeAuth = false;
			foreach (string b in lvm.listAuthorize as List<string>)
			{
				string[] auth = b.Split('_');
				if (auth[0].ToString() == "A1" || auth[0].ToString() == "A2")
					if (b.Contains((Url.Action("")).Replace("/", "")))
					{
						authorizer += auth[0].ToString();
						if (auth[0].ToString() == "A1")
							ViewBag.ApprovalAuth = true;
						else if (auth[0].ToString() == "A2")
						{
							ViewBag.ApprovalAuth = true;
							ViewBag.AcknowledgeAuth = true;
						}
					}
			}
			if (ViewBag.ApprovalAuth == false && lvm.isAdmin == true)
				ViewBag.ApprovalAuth = true;

			var tlq = db.transLeaveRequests.AsNoTracking().Where(x => x.leaveRequestID == id && x.employeeID == empID).ToList();
			if (tlq == null)
			{
				return HttpNotFound();
			}

			transLeaveRequest model = new transLeaveRequest();
			foreach (var a in tlq as IEnumerable<transLeaveRequest>)
			{
				model.acknowledgeBy = a.acknowledgeBy;
				model.acknowledgeDate = a.acknowledgeDate;
				model.acknowledgeStatus = a.acknowledgeStatus;
				model.approvedBy = a.approvedBy;
				model.approvedDate = a.approvedDate;
				model.approvedStatus = a.approvedStatus;
				model.createdDate = a.createdDate;
				model.createdUser = a.createdUser;
				model.employeeID = a.employeeID;
				model.leaveDescription = a.leaveDescription;
				model.leaveEndDate = a.leaveEndDate;
				model.leaveRequestID = a.leaveRequestID;
				model.leaveStartDate = a.leaveStartDate;
				model.modifiedDate = a.modifiedDate;
				model.modifiedUser = a.modifiedUser;
			}

			if (authorizer == "A1")
			{
				model.approvedStatus = act == "approved" ? true : false;
				model.approvedDate = DateTime.Now;
				model.approvedBy = lvm.userID;
			}
			else if (authorizer == "A2")
			{
				model.acknowledgeStatus = act == "approved" ? true : false;
				model.acknowledgeDate = DateTime.Now;
				model.acknowledgeBy = lvm.userID;
			}
			else if (authorizer == "A1A2")
			{
				model.approvedStatus = act == "approved" ? true : false;
				model.approvedDate = DateTime.Now;
				model.approvedBy = lvm.userID;
				model.acknowledgeStatus = act == "approved" ? true : false;
				model.acknowledgeDate = DateTime.Now;
				model.acknowledgeBy = lvm.userID;

			}

			try
			{
				var bucho = "tampan";
				db.Entry(model).State = EntityState.Modified;
				db.SaveChanges();

				SqlCommand cmd = new SqlCommand();
				cmd.CommandText = "[dbo].[sp_recountSisaSaldoCuti]";
				cmd.Connection = new SqlConnection(con.conString);
				cmd.Connection.Open();
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.Parameters.AddWithValue("@leaveRequestID", id);
				cmd.Parameters.AddWithValue("@year", DateTime.Now.Year);
				cmd.ExecuteNonQuery();
				cmd.Connection.Close();
			}
			catch (Exception exc)
			{
				var bucho = "tampan";
			}

			return RedirectToAction("Index");
		}
	}
}