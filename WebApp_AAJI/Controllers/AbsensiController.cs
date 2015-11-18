using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class AbsensiController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        private AccountController acm = new AccountController();
        private CommonController ccm = new CommonController();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();

        [HttpGet]
        public async Task<ActionResult> LoadDetailAbsensi(string periode, string dataDetail, string employeeId)
        {
            if (dataDetail != string.Empty)
                saveAbsensiEmployee(dataDetail, int.Parse(employeeId));

            string[] period = periode.Split('-');
            ViewBag.maxDate = DateTime.DaysInMonth(int.Parse(period[0]), int.Parse(period[1]));
            ViewBag.period = periode;
            ViewBag.employeeID = employeeId;
            ViewBag.ddlTypeAbsensi = ccm.ddlTypeAbsensi(string.Empty);

            loadAbsensiEmployee(int.Parse(period[0]), int.Parse(period[1]), int.Parse(employeeId));
            return PartialView("_PartialPageAbsensiEmployee1");
        }

        //
        // GET: /Absensi/
        public ActionResult Index()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");
            
            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = lvm.employeeId.ToString(), Text = lvm.employeeName });
            

            //ViewBag.EmployeeList = ccm.ddlAbsensiEmployee(0, string.Empty);//munculin bawahan ~ga dipake dl, karena ga ada setup atassan
            ViewBag.EmployeeList = new SelectList(list, "Value", "Text", lvm.employeeId);
            ViewBag.employeeID = lvm.employeeId;
            return View();
        }

        //
        // GET: /Absensi/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Absensi/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Absensi/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Absensi/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Absensi/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Absensi/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Absensi/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public void loadAbsensiEmployee(int year, int month, int employeeID)
        {
            int maxDate = DateTime.DaysInMonth(year, month);
            var start = new DateTime(year, month, 1);
            var end = new DateTime(year, month, maxDate);

            var empAbsensi = db.absensiEmployees.Where(x=>x.date >= start.Date && x.date <= end.Date && x.employeeID == employeeID).ToList();

            var model = new absensiEmployee();
            for (int i = 0; i < empAbsensi.Count; i++)
            {
                var editor = new absensiEmployee.absensiEmployeeDetail()
                {
                    id = empAbsensi[0].id,
                    employeeID = empAbsensi[0].employeeID,
                    date = empAbsensi[0].date,
                    checkIn = empAbsensi[0].checkIn,
                    checkOut = empAbsensi[0].checkOut,
                    typeAbsensiID = empAbsensi[0].typeAbsensiID,
                    remarks = empAbsensi[0].remarks
                };
                model.detailEmployeeAbsensi.Add(editor);
            }
            ViewBag.detailAbsensiEmployee = model.detailEmployeeAbsensi.ToList();
        }

        public void saveAbsensiEmployee(string dataDetail, int employeeId)
        {
            int dataInput = 0;
            string[] RowData = dataDetail.Split('|');

            for (int i = 0; i < (RowData.Count() - 1); i++)
            {
                string[] value = RowData[i].Split(';');
                DateTime dateAbsensi = Convert.ToDateTime(value[0].ToString());
                TimeSpan? hourIn = null;
                TimeSpan? hourOut = null;
                int? absensiType = null;
                string remark = null;

                try
                {
                    var getDataBefore = db.absensiEmployees.Where(x => x.employeeID == employeeId && x.date == dateAbsensi.Date).ToList();

                    var id = 0;
                    if (getDataBefore.Count > 0)
                    {
                        id = getDataBefore[0].id;
                    }

                    if (id != 0 && value[1].ToString() == string.Empty && value[2].ToString() == string.Empty && value[3].ToString() == string.Empty && value[4].ToString() == string.Empty)
                    {
                        absensiEmployee ae = db.absensiEmployees.Find(id);
                        db.absensiEmployees.Remove(ae);
                        db.SaveChanges();
                    }
                    else
                    {
                        if(value[3].ToString() == "")
                        {
                            hourIn = TimeSpan.Parse(value[1].ToString());
                            hourOut = TimeSpan.Parse(value[2].ToString());
                        }
                        else
                        { 
                            absensiType = int.Parse(value[3].ToString());
                            remark = value[4].ToString();
                        }

                
                        absensiEmployee ae = db.absensiEmployees.Find(id);
                        if (ae != null)
                        {
                            ae.checkIn = hourIn;
                            ae.checkOut = hourOut;
                            ae.typeAbsensiID = absensiType;
                            ae.remarks = remark;
                            db.Entry(ae).State = EntityState.Modified;
                        }
                        else
                        {
                            absensiEmployee abe = new absensiEmployee();
                            abe.employeeID = employeeId;
                            abe.date = dateAbsensi.Date;
                            abe.checkIn = hourIn;
                            abe.checkOut = hourOut;
                            abe.typeAbsensiID = absensiType;
                            abe.remarks = remark;
                            db.absensiEmployees.Add(abe);
                        }
                        db.SaveChanges();
                    }
                }
                catch(Exception exc)
                {
                    string a = exc.Message;
                }
            }

            if (dataInput > 0)
                db.SaveChanges();
        }
    }
}
