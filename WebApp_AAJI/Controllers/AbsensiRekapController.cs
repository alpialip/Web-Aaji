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
    public class AbsensiRekapController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private CommonController ccm = new CommonController();
        private ConnectionController cd = new ConnectionController();

        [HttpGet]
        public async Task<ActionResult> LoadDetailAbsensi(string date, string dataDetail)
        {
            if (dataDetail != string.Empty)
                saveAbsensi(dataDetail);

            loadAbsensi(DateTime.Parse(date));
            return PartialView("_PartialPageAbsensi1");
        }

        // GET: /AbsensiRekap/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            //loadAbsensi(DateTime.Now);
            return View(db.absensiRekaps.ToList());
        }

        // GET: /AbsensiRekap/Details/5
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
            absensiRekap absensirekap = db.absensiRekaps.Find(id);
            if (absensirekap == null)
            {
                return HttpNotFound();
            }
            return View(absensirekap);
        }

        // GET: /AbsensiRekap/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            return View();
        }

        // POST: /AbsensiRekap/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="id,absensiDate,employeeID,typeAbsensiID,remarks")] absensiRekap absensirekap)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                db.absensiRekaps.Add(absensirekap);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(absensirekap);
        }

        // GET: /AbsensiRekap/Edit/5
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
            absensiRekap absensirekap = db.absensiRekaps.Find(id);
            if (absensirekap == null)
            {
                return HttpNotFound();
            }
            return View(absensirekap);
        }

        // POST: /AbsensiRekap/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="id,absensiDate,employeeID,typeAbsensiID,remarks")] absensiRekap absensirekap)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
                db.Entry(absensirekap).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(absensirekap);
        }

        // GET: /AbsensiRekap/Delete/5
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
            absensiRekap absensirekap = db.absensiRekaps.Find(id);
            if (absensirekap == null)
            {
                return HttpNotFound();
            }
            return View(absensirekap);
        }

        // POST: /AbsensiRekap/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            absensiRekap absensirekap = db.absensiRekaps.Find(id);
            db.absensiRekaps.Remove(absensirekap);
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

        public void loadAbsensi(DateTime date)
        {
            var dtp = date;
            ViewBag.ddlTypeAbsensi = ccm.ddlTypeAbsensi(string.Empty);

            var empAbsensi = db.employees
                            .Join(db.employeePositions
                                    .Join(db.employeeResigns, c => c.employeeID, d => d.employeeID, (c, d) => new { c, d })
                                        .Where(e => e.c.positionDate > e.d.resignDate)
                                   , a => a.employeeID, b => b.c.employeeID, (a, b) => new { a, b })
                            .Join(db.departments, f => f.b.c.deptID, g => g.deptID, (f, g) => new { f, g })
                            .Select(h => new { h.f.a.employeeID, h.f.a.employeeNIK, h.f.a.employeeName, h.g.deptName})
                            .ToList();

            string sql = string.Empty;
            sql = "SELECT ";
            sql += "a.employeeID, a.employeeNIK, a.employeeName, ";
            sql += "d.deptID, d.deptName ";
            sql += "FROM [dbo].[Employees] a ";
            sql += "INNER JOIN ";
            sql += "(SELECT a.employeeID, b.positionId ";
            sql += "FROM [dbo].[EmployeeResigns] a ";
            sql += "INNER JOIN [dbo].[EmployeePositions] b ON (b.employeeID = a.employeeID AND b.positionDate > a.resignDate) ";
            sql += "UNION ";
            sql += "SELECT employeeID, ";
            sql += "(SELECT positionId FROM [dbo].[EmployeePositions] WHERE employeeID = a.employeeID)positionId ";
            sql += "FROM [dbo].[Employees] a ";
            sql += "WHERE a.employeeID NOT IN (SELECT employeeID ";
            sql += "							FROM [dbo].[EmployeeResigns]) ";
            sql += ") b ON b.employeeID = a.employeeID ";
            sql += "LEFT JOIN [dbo].[EmployeePositions] c ON c.employeeID = b.employeeID AND c.positionId = b.positionId ";
            sql += "LEFT JOIN [dbo].[Departments] d ON d.deptID = c.deptID ";
            DataTable dtEmployee = cd.executeReader(sql);

            var model = new absensiRekap();
            foreach (DataRow dr in dtEmployee.Rows)
            {
                int absensiTipe = 0;
                string ket = string.Empty;
                foreach (var att in db.absensiRekaps.Where(x => x.absensiDate == dtp.Date).ToList() as IEnumerable<Models.absensiRekap>)
                {
                    if (att.employeeID == int.Parse(dr["employeeID"].ToString()))
                    {
                        absensiTipe = att.typeAbsensiID;
                        ket = att.remarks;
                        break;
                    }
                }

                var editor = new absensiRekap.RekapAbsensiDetail()
                {
                    employeeID = int.Parse(dr["employeeID"].ToString()),
                    employeeNIK = dr["employeeNIK"].ToString(),
                    employeeName = dr["employeeName"].ToString(),
                    deptName = dr["deptName"].ToString(),
                    typeAbsensiId = absensiTipe,
                    remarks = ket
                };
                model.detailRekapAbsensi.Add(editor);
            }
            ViewBag.detailRekapAbsensi = model.detailRekapAbsensi.ToList();
        }

        public void saveAbsensi(string dataDetail)
        {
            int dataInput = 0;
             string[] RowData = dataDetail.Split('|');

             for (int i = 0; i < (RowData.Count()-1); i++)
             {
                 string[] value = RowData[i].Split(';');
                 DateTime dateAbsensi = DateTime.Parse(value[0].ToString());
                 int employeeId = int.Parse(value[1].ToString());
                 int absensiType = int.Parse(value[2].ToString());
                 string remark = value[3].ToString();

                db.absensiRekaps.RemoveRange(db.absensiRekaps.Where(x => x.absensiDate == dateAbsensi && x.employeeID == employeeId));
                absensiRekap abs = new absensiRekap();
                abs.absensiDate = dateAbsensi;
                abs.employeeID = employeeId;
                abs.typeAbsensiID = absensiType;
                abs.remarks = remark;
                db.absensiRekaps.Add(abs);
                 dataInput++;
             }

            if(dataInput > 0)
                db.SaveChanges();
        }
    }
}
