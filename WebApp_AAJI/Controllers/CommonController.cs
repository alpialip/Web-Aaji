using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;

namespace WebApp_AAJI.Controllers
{
    public class CommonController : Controller
    {
        private MyDataAccess db = new MyDataAccess();

        public string uploadDir = "~/uploads";
        public string uploadPhotoDir = "~/uploadPhoto";

        /// <summary>
        /// Output: Insert (i), Update (u), Delete (d), View (v)
        /// </summary>
        /// <returns></returns>
        public string[,] assignActionValidation()
        {
            return new string[4, 2] { { "i", "Insert" }, { "u", "Update" }, { "d", "Delete" }, { "v", "View" } };
        }

        public string[,] assignMenuValidationExclusive()
        {
            return new string[1, 2] { { "a", "Authorize" } };
        }

        public SelectList ddlMenuValidationExclusive(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "A1_PurchaseRequest", Text = "Authorize Approval Purchase Request" });
            //list.Add(new SelectListItem { Value = "A2_PurchaseRequest", Text = "Authorize Acknowledge Purchase Request" });
            list.Add(new SelectListItem { Value = "A1_PurchaseOrder", Text = "Authorize Approval Purchase Order" });
            //list.Add(new SelectListItem { Value = "A2_PurchaseOrder", Text = "Authorize Acknowledge Purchase Order" });
            list.Add(new SelectListItem { Value = "A1_PurchaseReceive", Text = "Authorize Approval Purchase Receive" });
            //list.Add(new SelectListItem { Value = "A2_PurchaseReceive", Text = "Authorize Acknowledge Purchase Receive" });
            list.Add(new SelectListItem { Value = "A1_AdvancePaymentVoucher", Text = "Authorize Approval Advance Payment Voucher" });
            //list.Add(new SelectListItem { Value = "A2_AdvancePaymentVoucher", Text = "Authorize Acknowledge Advance Payment Voucher" });
            list.Add(new SelectListItem { Value = "A1_BankCashPaymentVoucher", Text = "Authorize Approval Bank Cash Payment Voucher" });
            //list.Add(new SelectListItem { Value = "A2_BankCashPaymentVoucher", Text = "Authorize Acknowledge Bank Cash Payment Voucher" });
            list.Add(new SelectListItem { Value = "A1_CashOnHandAndAdvanceRequest", Text = "Authorize Cash On Hand And Advance Request" });
            list.Add(new SelectListItem { Value = "AINV_Account", Text = "Authorize View Invoice Due Date H-7" });
            list.Add(new SelectListItem { Value = "AEMP_Account", Text = "Authorize View Employee H-7" });
            list.Add(new SelectListItem { Value = "A1_EmployeeClaimMedical", Text = "Authorize Approval Claim Medical Employee" });
            list.Add(new SelectListItem { Value = "A1_EmployeeLoan", Text = "Authorize Approval Loan Employee" });
            list.Add(new SelectListItem { Value = "RCVD_Account", Text = "Authorize View Purchase Request" });
            list.Add(new SelectListItem { Value = "A1_LeaveRequest", Text = "Authorize Approval Leave Request" });
            list.Add(new SelectListItem { Value = "A2_LeaveRequest", Text = "Authorize Acknowledge Leave Request" });

            return new SelectList(list, "Value", "Text", currSelection);
        }

        //diisi dengan 'Once','EveryDay','Weekly','Monthly','Annualy'
        public SelectList ddlReminderRepetition(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "Once", Text = "Once" });
            list.Add(new SelectListItem { Value = "Daily", Text = "Daily" });
            list.Add(new SelectListItem { Value = "Weekly", Text = "Weekly" });
            list.Add(new SelectListItem { Value = "Monthly", Text = "Monthly" });
            list.Add(new SelectListItem { Value = "Annualy", Text = "Annualy" });

            return new SelectList(list, "Value", "Text", currSelection);
        }

        //diisi dengan '0_Day','1_Day','2_Day','3_Day','7_Day','14_Day','1_Month'
        public SelectList ddlReminder(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "0_Day", Text = "0 Day" });
            list.Add(new SelectListItem { Value = "1_Day", Text = "1 Day" });
            list.Add(new SelectListItem { Value = "2_Day", Text = "2 Day" });
            list.Add(new SelectListItem { Value = "3_Day", Text = "3 Day" });
            list.Add(new SelectListItem { Value = "7_Day", Text = "7 Day" });
            list.Add(new SelectListItem { Value = "14_Day", Text = "14 Day" });
            list.Add(new SelectListItem { Value = "1_Month", Text = "1 Month" });

            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlMonth(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "1", Text = "January" });
            list.Add(new SelectListItem { Value = "2", Text = "February" });
            list.Add(new SelectListItem { Value = "3", Text = "March" });
            list.Add(new SelectListItem { Value = "4", Text = "April" });
            list.Add(new SelectListItem { Value = "5", Text = "May" });
            list.Add(new SelectListItem { Value = "6", Text = "June" });
            list.Add(new SelectListItem { Value = "7", Text = "July" });
            list.Add(new SelectListItem { Value = "8", Text = "August" });
            list.Add(new SelectListItem { Value = "9", Text = "September" });
            list.Add(new SelectListItem { Value = "10", Text = "October" });
            list.Add(new SelectListItem { Value = "11", Text = "November" });
            list.Add(new SelectListItem { Value = "12", Text = "December" });

            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlBank(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach(var a in db.banks.ToList() as IEnumerable<Models.bank>)
            {
                list.Add(new SelectListItem { Value = a.bankID.ToString(), Text = a.bankName });
            }
            return new SelectList(list, "Value", "Text", currSelection);
        }
        
        public SelectList ddlMenuParent(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var a in db.Menus.Where(x=>x.menuIsParent == true).ToList() as IEnumerable<Models.menu>)
            {
                list.Add(new SelectListItem { Value = a.menuID.ToString(), Text = a.menuName });
            }
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlBankEmployee(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            IEnumerable<string> listBankEmployee = db.employees.Select(m => m.bankName).Distinct().OrderBy(m => m).ToList();
            foreach (var bankName in listBankEmployee as IEnumerable<string>)
            {
                list.Add(new SelectListItem { Value = bankName, Text = bankName });
            }

            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlVendor(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var a in db.vendors.ToList() as IEnumerable<Models.vendor>)
            {
                list.Add(new SelectListItem { Value = a.vendorID.ToString(), Text = a.vendorName });
            }
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlAccountCOA(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var a in db.chartOfAccounts.Where(x=>x.levelID == 3).ToList() as IEnumerable<Models.chartOfAccount>)
            {
                list.Add(new SelectListItem { Value = a.id.ToString(), Text = a.accountName });
            }
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlDepartment(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var a in db.departments.ToList() as IEnumerable<Models.department>)
            {
                list.Add(new SelectListItem { Value = a.deptID.ToString(), Text = a.deptName });
            }
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlDivisi(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var a in db.divisis.ToList() as IEnumerable<Models.divisi>)
            {
                list.Add(new SelectListItem { Value = a.divisiID.ToString(), Text = a.divisiName });
            }
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlDivisi(string currSelection, int deptId)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var a in db.divisis.Where(x=>x.deptID==deptId).ToList() as IEnumerable<Models.divisi>)
            {
                list.Add(new SelectListItem { Value = a.divisiID.ToString(), Text = a.divisiName });
            }
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlLevel(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var a in db.levels.ToList() as IEnumerable<Models.level>)
            {
                list.Add(new SelectListItem { Value = a.levelID.ToString(), Text = a.levelName });
            }
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlStatusEmployee(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "Contract", Text = "Contract" });
            list.Add(new SelectListItem { Value = "Probation", Text = "Probation" });
            list.Add(new SelectListItem { Value = "Permanent", Text = "Permanent" });

            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlStatusFixedAsset(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "Ready", Text = "Ready" });
            list.Add(new SelectListItem { Value = "Sold", Text = "Sold" });
            list.Add(new SelectListItem { Value = "Broken", Text = "Broken" });

            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlTypeAbsensi(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var a in db.typeAbsensis.ToList() as IEnumerable<Models.typeAbsensi>)
            {
                list.Add(new SelectListItem { Value = a.typeAbsensiID.ToString(), Text = a.typeAbsensiName });
            }
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlUserAll(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            foreach (var a in db.Users.Where(x=>x.userID != "admin" && x.isActive==true && x.employeeNIK != null).ToList() as IEnumerable<Models.user>)
            {
                list.Add(new SelectListItem { Value = a.employeeNIK.ToString(), Text = a.userName });
            }
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlActivityType(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "Routine", Text = "Routine" });
            list.Add(new SelectListItem { Value = "Project", Text = "Project" });
            list.Add(new SelectListItem { Value = "Business_Trip", Text = "Business Trip" });

            return new SelectList(list, "Value", "Text", currSelection);
        }//klo ada perubahan, jangan lupa buat update COA_ActivityType() -nya juga

        public int COA_ActivityType(string selectedActivity)
        {
            int result = 0;
            switch(selectedActivity)
            {
                case "Routine" :
                    result = 38;
                    break;
                case "Project" :
                    result = 39;
                    break;
                case "Business_Trip" :
                    result = 40;
                    break;
            }

            return result;
        }

        public SelectList ddlPaymentType(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "Cash", Text = "Cash" });
            list.Add(new SelectListItem { Value = "Transfer", Text = "Transfer" });
            list.Add(new SelectListItem { Value = "Giro", Text = "Giro" });
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlPeriodDepreciation(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "Day", Text = "Days" });
            list.Add(new SelectListItem { Value = "Month", Text = "Months" });
            list.Add(new SelectListItem { Value = "Year", Text = "Years" });
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlAbsensiEmployee(int employeeID, string currSelection)
        {
            var haveSubordinate = db.employees.AsNoTracking()
                                .Join(db.employeePositions
                                        .Join(db.employeeResigns, c => c.employeeID, d => d.employeeID, (c, d) => new { c, d })
                                            .Where(e => e.c.positionDate > e.d.resignDate)
                                       , a => a.employeeID, b => b.c.employeeID, (a, b) => new { a, b })
                                .Join(db.departments, f => f.b.c.deptID, g => g.deptID, (f, g) => new { f, g })
                                .Select(h => new { h.f.a.employeeID, h.f.a.employeeNIK, h.f.a.employeeName, h.g.deptID, h.g.deptName })
                                .ToList(); 

            List<SelectListItem> list = new List<SelectListItem>();
            //foreach (var a in db.typeAbsensis.ToList() as IEnumerable<Models.typeAbsensi>)
            //{
            //    list.Add(new SelectListItem { Value = a.typeAbsensiID.ToString(), Text = a.typeAbsensiName });
            //}
            return new SelectList(list, "Value", "Text", currSelection);
        
            //var popUp = db.employees.AsNoTracking()
            //                    .Join(db.employeePositions
            //                            .Join(db.employeeResigns, c => c.employeeID, d => d.employeeID, (c, d) => new { c, d })
            //                                .Where(e => e.c.positionDate > e.d.resignDate)
            //                           , a => a.employeeID, b => b.c.employeeID, (a, b) => new { a, b })
            //                    .Join(db.departments, f => f.b.c.deptID, g => g.deptID, (f, g) => new { f, g })
            //                    .Select(h => new { h.f.a.employeeID, h.f.a.employeeNIK, h.f.a.employeeName ,h.g.deptID ,h.g.deptName })
            //                    .ToList(); 

            //    var model = new employeeLoan();
            //    for (int i = 0; i < popUp.Count; i++)
            //    {
            //        var editor = new employeeLoan.employeePOPUp()
            //        {
            //            employeeID = popUp[i].employeeID,
            //            employeeNIK = popUp[i].employeeNIK,
            //            employeeName = popUp[i].employeeName,
            //            deptID = popUp[i].deptID,
            //            deptName = popUp[i].deptName
            //        };
            //        model.popUpEmployee.Add(editor);
            //    }
            //    ViewBag.EmpPopUp = model.popUpEmployee.ToList();
        }

        public void saveToLog(string action, string table, object values, string[] wherecond)
        {
            string query = "";

            if (action == "INSERT")
                query = action + " INTO " + table + " (" + values + ") VALUE (" + values + ")"; 
            else if(action == "DELETE")
                query = action + " INTO " + table + " (" + values + ") VALUE (" + values + ")"; 
            else if(action == "UDPATE")
                query = action + " INTO " + table + " (" + values + ") VALUE (" + values + ")";
            else //SELECT
                query = action + " INTO " + table + " (" + values + ") VALUE (" + values + ")"; 
        }

        public decimal depreciationAsset(string period, decimal procentageDepreciation, int valuePeriod, DateTime? purchaseDate, decimal pricePurchase)
        {
            DateTime purchaseDates = DateTime.Now ;
            if(purchaseDate != null)
                purchaseDates = Convert.ToDateTime(purchaseDate);

            decimal depreciationValue = pricePurchase;
            decimal nilaiPenyusutan = ((procentageDepreciation / 100) * pricePurchase);
            double intervalAssetFromPurchaseToCurrent = 0;

            if(period.ToLower() == "day")
            {
                intervalAssetFromPurchaseToCurrent = (DateTime.Now - purchaseDates).TotalDays;
            }
            else if(period.ToLower() == "month")
            {
                intervalAssetFromPurchaseToCurrent = ((((purchaseDates.Year - DateTime.Now.Year) * 12) + purchaseDates.Month - DateTime.Now.Month) * -1);
            }
            else if (period.ToLower() == "year")
            {
                intervalAssetFromPurchaseToCurrent = DateTime.Now.Year - purchaseDates.Year;
            }

            for (int i = 0; i < intervalAssetFromPurchaseToCurrent; i++ )
            {
                depreciationValue = (depreciationValue - nilaiPenyusutan);

                if (valuePeriod > 1)
                    i += valuePeriod;
            }

            return depreciationValue > 0 ? depreciationValue : 0;
        }
        
        //diisi dengan 'Education','Hospital','Renovations'
        public SelectList ddlLoanCategory(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "Education", Text = "Educations" });
            list.Add(new SelectListItem { Value = "Hospital", Text = "Hospitals" });
            list.Add(new SelectListItem { Value = "Renovation", Text = "Renovations" });

            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList ddlMateStatus(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "Current", Text = "Husband/Wife Current" });
            list.Add(new SelectListItem { Value = "Died", Text = "Died" });
            list.Add(new SelectListItem { Value = "Divorce", Text = "Divorce" });

            return new SelectList(list, "Value", "Text", currSelection);
        }

        [HttpGet]
        public async Task<ActionResult> LoadPopUpVendor ()
        {
            ViewBag.VendorPopUp = db.vendors.ToList();
            return PartialView("~/Views/Shared/_PartialPageVendor.cshtml");
        }

        public void LoadPopUpVendors()
        {
            ViewBag.VendorPopUp = db.vendors.ToList();
            ViewData["VendorPopUp"] = db.vendors.ToList();
        }

        [HttpGet]
        public async Task<ActionResult> LoadPopUpPO(string poId)
        {
            var x = db.purchaseOrderHeaders
                    .Join(db.vendors, a => a.vendorId, b => b.vendorID, (a, b) => new { a, b })
                                        .Select(c => new { c.a.poId, c.a.poDate, c.a.vendorId, c.b.vendorName, c.a.poUrgent }).ToList();
            if(poId != string.Empty)
               x = db.purchaseOrderHeaders.Where(po=>po.poId == poId)
                   .Join(db.vendors, a => a.vendorId, b => b.vendorID, (a, b) => new { a, b })
                    .Select(c => new { c.a.poId, c.a.poDate, c.a.vendorId, c.b.vendorName, c.a.poUrgent }).ToList();

            var model = new purchaseOrderHeader();
            foreach(var y in x as IEnumerable<Models.purchaseOrderHeader.POpopUp>)
            {
                var editor = new purchaseOrderHeader.POpopUp()
                {
                    poId = y.poId,
                    poDate = y.poDate,
                    vendorId = y.vendorId,
                    vendorName = y.vendorName,
                    poUrgent = y.poUrgent
                };
                model.PopUpPO.Add(editor);
            }
            ViewBag.PopUpPO = model.PopUpPO.ToList();
            return PartialView("~/Views/Shared/_PartialPagePopUpPO.cshtml");
        }

        public ActionResult LoadPopUpPR(string prId)
        {
            var x = db.purchaseRequestHeaders
                //.Join(db.departments, a => a.requestDeptId, b => b.deptID, (a, b) => new { a, b })
                //                    .Select(c => new { c.a.prId, c.b.deptID, c.b.deptName, c.a.typeOrder, c.a.proposalInclude, c.a.specialInstruction, c.a.projectTimeDelivery })
                    .ToList();
            if (prId != string.Empty)
                x = db.purchaseRequestHeaders.Where(pr => pr.prId == prId)
                    //.Join(db.departments, a => a.requestDeptId, b => b.deptID, (a, b) => new { a, b })
                    //                    .Select(c => new { prId = c.a.prId, deptID = c.b.deptID, deptName = c.b.deptName, typeOrder = c.a.typeOrder, proposalInclude = c.a.proposalInclude, specialInstruction = c.a.specialInstruction, projectTimeDelivery = c.a.projectTimeDelivery })
                    .ToList();

            var model = new purchaseRequestHeader();
            foreach (var y in x as IEnumerable<purchaseRequestHeader>)
            {
                var editor = new purchaseRequestHeader.PRPopUp()
                {
                    prId = y.prId,
                    //deptId = y.deptId,
                    //deptName = y.deptName,
                    typeOrder = y.typeOrder,
                    proposalInclude = y.proposalInclude,
                    specialInstruction = y.specialInstruction,
                    projectTimeDelivery = y.projectTimeDelivery
                };
                model.popUpPR.Add(editor);
            }
            ViewBag.PRPopUp = model.popUpPR.ToList();
            return View();
        }

        public string getUserName(string id)
        {
            var a = db.Users.Where(x => x.userID == id).ToList();
            return a[0].userName;
        }

        public FileResult Download(string file)
        {
            string[] filename = file.Split('\\');
            string filePath = Server.MapPath(file).ToString();
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            var response = new FileContentResult(fileBytes, "application/octet-stream");
            response.FileDownloadName = filename[1].ToString();
            return response;
        }

        public string Encrypt(string clearText)
        {
            string EncryptionKey =  ConfigurationManager.AppSettings["cryptoKey"].ToString();
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public string Decrypt(string cipherText)
        {
            string EncryptionKey = ConfigurationManager.AppSettings["cryptoKey"].ToString();
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public SelectList statusNikah(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "Belum_Kawin", Text = "Belum Kawin" });
            list.Add(new SelectListItem { Value = "Kawin", Text = "Kawin" });
            list.Add(new SelectListItem { Value = "Cerai_Hidup", Text = "Cerai Hidup" });
            list.Add(new SelectListItem { Value = "Cerai_Mati", Text = "Cerai Mati" });

            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList kewarganegaraan(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "WNI", Text = "WNI" });
            list.Add(new SelectListItem { Value = "WNA", Text = "WNA" });

            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList genders(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "L", Text = "Male" });
            list.Add(new SelectListItem { Value = "P", Text = "Female" });

            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList agama(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "Islam", Text = "Islam" });
            list.Add(new SelectListItem { Value = "Kristen", Text = "Kristen" });
            list.Add(new SelectListItem { Value = "Khatolik", Text = "Khatolik" });
            list.Add(new SelectListItem { Value = "Buddha", Text = "Buddha" });
            list.Add(new SelectListItem { Value = "Hindu", Text = "Hindu" });

            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList typeClaimEmployee(string currSelection)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem { Value = "Penyakit_Kritis", Text = "Penyakit Kritis" });
            list.Add(new SelectListItem { Value = "Kecelakaan", Text = "Kecelakaan" });
            list.Add(new SelectListItem { Value = "Perawatan_Gigi", Text = "Perawatan Gigi" });

            return new SelectList(list, "Value", "Text", currSelection);
        }
	}
}