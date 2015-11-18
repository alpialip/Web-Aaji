using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
	public class SisaSaldoCutiController : Controller
	{
		private MyDataAccess db = new MyDataAccess();
		LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
		private AccountController acm = new AccountController();
		private CommonController ccm = new CommonController();
		private ConnectionController con = new ConnectionController();

		[HttpGet]
		public async Task<ActionResult> LoadDetailCuti(string year, string dataDetail)
		{
			if (dataDetail != string.Empty)
				saveAbsensi(dataDetail);

			loadAbsensi(int.Parse(year));
			return PartialView("_PartialPageSisaSaldo1");
		}

		// GET: /SisaSaldoCuti/
		public ActionResult Index()
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			//loadAbsensi(DateTime.Now.Year);
			return View(db.sisaSaldoCutis.ToList());
		}

		// GET: /SisaSaldoCuti/Details/5
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
			sisaSaldoCuti sisasaldocuti = db.sisaSaldoCutis.Find(id);
			if (sisasaldocuti == null)
			{
				return HttpNotFound();
			}
			return View(sisasaldocuti);
		}

		// GET: /SisaSaldoCuti/Create
		public ActionResult Create()
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			return View();
		}

		// POST: /SisaSaldoCuti/Create
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "id,year,employeeID,amount")] sisaSaldoCuti sisasaldocuti)
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			if (ModelState.IsValid)
			{
				db.sisaSaldoCutis.Add(sisasaldocuti);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(sisasaldocuti);
		}

		// GET: /SisaSaldoCuti/Edit/5
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
			sisaSaldoCuti sisasaldocuti = db.sisaSaldoCutis.Find(id);
			if (sisasaldocuti == null)
			{
				return HttpNotFound();
			}
			return View(sisasaldocuti);
		}

		// POST: /SisaSaldoCuti/Edit/5
		// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
		// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Edit([Bind(Include = "id,year,employeeID,amount")] sisaSaldoCuti sisasaldocuti)
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			if (ModelState.IsValid)
			{
				db.Entry(sisasaldocuti).State = EntityState.Modified;
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(sisasaldocuti);
		}

		// GET: /SisaSaldoCuti/Delete/5
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
			sisaSaldoCuti sisasaldocuti = db.sisaSaldoCutis.Find(id);
			if (sisasaldocuti == null)
			{
				return HttpNotFound();
			}
			return View(sisasaldocuti);
		}

		// POST: /SisaSaldoCuti/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public ActionResult DeleteConfirmed(int id)
		{
			if (acm.cekSession() == false)
				return RedirectToAction("Logout", "Account");

			lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
			if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
				return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

			sisaSaldoCuti sisasaldocuti = db.sisaSaldoCutis.Find(id);
			db.sisaSaldoCutis.Remove(sisasaldocuti);
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

		public void loadAbsensi(int year)
		{
			var model = new sisaSaldoCuti();
			#region oldProcess
			//var empAbsensi = db.employees
			//                .Join(db.employeePositions
			//                        .Join(db.employeeResigns, c => c.employeeID, d => d.employeeID, (c, d) => new { c, d })
			//                            .Where(e => e.c.positionDate > e.d.resignDate)
			//                       , a => a.employeeID, b => b.c.employeeID, (a, b) => new { a, b })
			//                .Join(db.divisis, f => f.b.c.divisiID, g => g.deptID, (f, g) => new { f, g })
			//                .Join(db.levels, i => i.f.b.c.levelID, j => j.levelID, (i, j) => new {i,j})
			//                .Select(h => new { h.i.f.a.employeeID, h.i.f.a.employeeNIK, h.i.f.a.employeeName, deptName = h.i.g.divisiName, h.j.saldoCuti })
			//                .ToList();

			//for (int i = 0; i < empAbsensi.Count; i++)
			//{
			//    int amount = empAbsensi[i].saldoCuti;
			//    foreach (var att in db.sisaSaldoCutis.Where(x => x.year == year).ToList() as IEnumerable<Models.sisaSaldoCuti>)
			//    {
			//        if (att.employeeID == empAbsensi[i].employeeID)
			//        {
			//            amount = /*amount -*/ att.amount;
			//            break;
			//        }
			//    }

			//    var editor = new sisaSaldoCuti.saldoCutiDetail()
			//    {
			//        employeeID = empAbsensi[i].employeeID,
			//        employeeNIK = empAbsensi[i].employeeNIK,
			//        employeeName = empAbsensi[i].employeeName,
			//        deptName = empAbsensi[i].deptName,
			//        amount = amount
			//    };
			//    model.detailSaldoCuti.Add(editor);
			//}
			#endregion

			#region newProcess
			string sql = string.Empty;
			sql += "SELECT a.[employeeID],[employeeName],[employeeNIK],a.[deptName], ";
			sql += "ISNULL((select amount from [dbo].[SisaSaldoCutis] where employeeID = a.employeeID AND [year] = " + year + "), ";
			sql += "(SELECT saldoCuti from [dbo].[Levels] where levelID = a.levelID) ";
			sql += ") AS amount ";
			sql += "FROM [DB_AAJI].[dbo].[v_employee] a ";
			sql += "WHERE resignDate IS NULL ";
			DataTable dtEmployee = con.executeReader(sql);

			foreach (DataRow dr in dtEmployee.Rows)
			{
				var editor = new sisaSaldoCuti.saldoCutiDetail()
				{
					employeeID = int.Parse(dr["employeeID"].ToString()),
					employeeNIK = dr["employeeNIK"].ToString(),
					employeeName = dr["employeeName"].ToString(),
					deptName = dr["deptName"].ToString(),
					amount = int.Parse(dr["amount"].ToString())
				};
				model.detailSaldoCuti.Add(editor);
			}
			#endregion

			ViewBag.detailSaldoCuti = model.detailSaldoCuti.ToList();
		}

		public void saveAbsensi(string dataDetail)
		{
			int dataInput = 0;
			string[] RowData = dataDetail.Split('|');

			for (int i = 0; i < (RowData.Count() - 1); i++)
			{
				string[] value = RowData[i].Split(';');
				int year = int.Parse(value[0].ToString());
				int employeeId = int.Parse(value[1].ToString());
				int amount = int.Parse(value[2].ToString());

				db.sisaSaldoCutis.RemoveRange(db.sisaSaldoCutis.Where(x => x.year == year && x.employeeID == employeeId));
				sisaSaldoCuti abs = new sisaSaldoCuti();
				abs.year = year;
				abs.employeeID = employeeId;
				abs.amount = amount;
				db.sisaSaldoCutis.Add(abs);
				dataInput++;
			}

			if (dataInput > 0)
				db.SaveChanges();
		}
	}
}
