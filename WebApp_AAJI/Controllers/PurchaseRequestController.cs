using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using WebApp_AAJI.Models;
using PagedList;

namespace WebApp_AAJI.Controllers
{
    public class PurchaseRequestController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private CommonController ccm = new CommonController();
        private ConnectionController con = new ConnectionController();
        string prefix = "PRQ";

        [HttpGet]
        public async Task<ActionResult> ApprovalProcess(string act, string id)
        {
                lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                purchaseRequestHeader model = db.purchaseRequestHeaders.Find(id);
                model.approvedStatus = act == "approved" ? true : false;
                model.approvedDate = DateTime.Now;
                model.approvedBy = lvm.userID;
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index", "PurchaseRequestHeader");
        }

        [HttpGet]
        public async Task<ActionResult> popUpSearchVendor(string keywordId, string keywordSearch, int maxRows, int pageStart)
        {
            string sql = "WITH cte AS ( ";
            sql += "SELECT ROW_NUMBER() OVER(ORDER BY id) AS rn, ";
            sql += "[id],[vendorID],[vendorName],[npwp],[address1],[address2],[telp],[fax],[contactPerson],[ppn],[top],[createdUser],[createdDate],[modifiedUser],[modifiedDate] ";
            sql += "FROM [dbo].[Vendors] a WITH(NOLOCK) ";
            if (keywordId != string.Empty && keywordSearch != string.Empty)
            {
                sql += "WHERE " + keywordId + " LIKE  '%" + keywordSearch + "%' ";
            }
            sql += ") ";
            sql += "SELECT * FROM cte ";
            string sqlAll = sql;
            sql += "WHERE rn BETWEEN " + ((pageStart * maxRows) - (maxRows - 1)) + " AND " + (pageStart * maxRows);

            DataTable dataAll = con.executeReader(sqlAll);
            ViewBag.totalRowsVendor = dataAll.Rows.Count;
            ViewBag.currentPage = pageStart;

            DataTable popUp = con.executeReader(sql);
            List<vendor> model = new List<vendor>();
            if (popUp != null)
            {
                foreach (DataRow dr in popUp.Rows)
                {
                    DateTime? modified = dr["modifiedDate"].ToString() == string.Empty ? DateTime.Now : Convert.ToDateTime(dr["modifiedDate"].ToString());
                    var editor = new vendor()
                    {
                        id = int.Parse(dr["id"].ToString()),
                        vendorID = dr["vendorID"].ToString(),
                        vendorName = dr["vendorName"].ToString(),
                        npwp = dr["npwp"].ToString(),
                        address1 = dr["address1"].ToString(),
                        address2 = dr["address2"].ToString(),
                        telp = dr["telp"].ToString(),
                        fax = dr["fax"].ToString(),
                        contactPerson = dr["contactPerson"].ToString(),
                        ppn = bool.Parse(dr["ppn"].ToString()),
                        top = int.Parse(dr["top"].ToString()),
                        createdUser = dr["createdUser"].ToString(),
                        createdDate = DateTime.Parse(dr["createdDate"].ToString()),
                        modifiedDate = modified,
                        modifiedUser = dr["modifiedUser"].ToString()
                    };
                    model.Add(editor);
                }
                ViewBag.VendorPopUp = model.ToList();
            }

            return PartialView("_PartialPagePopUpVendorSearchSub1");
        }

        [HttpGet]
        public async Task<ActionResult> popUpSearch(string keywordId, string keywordSearch, int maxRows, int pageStart)
        {
            string sql = "WITH cte AS ( ";
            sql += "SELECT ROW_NUMBER() OVER(ORDER BY productId) AS rn, ";
            sql += "productID, productCode, productName, unit ";
            sql += "FROM [dbo].[Products] a WITH(NOLOCK) ";
            if (keywordId != string.Empty && keywordSearch != string.Empty)
            {
                sql += "WHERE " + keywordId + " LIKE  '%" + keywordSearch + "%' ";
            }
            sql += ") ";
            sql += "SELECT * FROM cte ";
            string sqlAll = sql;
            sql += "WHERE rn BETWEEN " + ((pageStart * maxRows) - (maxRows - 1)) + " AND " + (pageStart * maxRows);

            DataTable dataAll = con.executeReader(sqlAll);
            ViewBag.totalRows = dataAll.Rows.Count;
            ViewBag.currentPage = pageStart;

            DataTable popUp = con.executeReader(sql);
            var model = new product();
            if (popUp != null)
            {
                foreach (DataRow dr in popUp.Rows)
                {
                    var editor = new product.listProduct()
                    {
                        productID = int.Parse(dr["productId"].ToString()),
                        productCode = dr["productCode"].ToString(),
                        productName = dr["productName"].ToString(),
                        unit = dr["unit"].ToString()
                    };
                    model.productList.Add(editor);
                }
                ViewBag.ProductPopUp = model.productList.ToList();
            }

            return PartialView("_PartialPageProductSearchSub");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDetailPurchaseRequest(string dataDetail, string act)
        {
            string[] data = dataDetail.Split('|');

            var model = new purchaseRequestHeader();
            for (int i = 0; i < data.Count(); i++)
            {
                string[] value = data[i].Split(';');

                if (value[1].ToString() == "")
                    break;

                int t_productID = int.Parse(value[1].ToString());
                string t_description = value[2].ToString();
                int t_qty = value[3].ToString() != "" ? Convert.ToInt32(value[3].ToString()) : 0;
                string t_unit = value[4].ToString();
                int? t_vendorid = int.Parse(value[5].ToString());

                var editor = new purchaseRequestHeader.purchaseRequestDetail()
                {
                    productId = t_productID,
                    description = t_description,
                    qty = t_qty,
                    unit = t_unit,
                    vendorId = t_vendorid
                };
                model.detailPurchaseRequest.Add(editor);
            }
            ViewData["dataDetail"] = model.detailPurchaseRequest.ToList();
            loadDetail(string.Empty, 0);
            return PartialView("_PartialPagePR_Supplier1");
        }

        // GET: /PurchaseRequest/
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, string startDateFilter, string startDate, string endDateFilter, string endDate, int? page)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.ApprovalAuth = false;
            foreach (string b in lvm.listAuthorize as List<string>)
            {
                string[] auth = b.Split('_');
                if (auth[0].ToString() == "A1")
                    if (b.Contains((Url.Action("")).Replace("/", "")))
                    {
                        ViewBag.ApprovalAuth = true;
                    }
            }
            if (ViewBag.ApprovalAuth == false && lvm.isAdmin == true)
                ViewBag.ApprovalAuth = true;

            ViewBag.poHasCreated = db.purchaseOrderHeaders.ToList();
            ViewBag.Dept = db.departments.ToList();
            
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

            var data = from s in db.purchaseRequestHeaders select s;
            if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate) && !String.IsNullOrEmpty(searchString))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.prDate >= sDate && s.prDate <= eDate && s.prId.Contains(searchString));
            }
            else if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.prDate >= sDate && s.prDate <= eDate);
            }                
            else if (!String.IsNullOrEmpty(searchString))
            {
                data = data.Where(s => s.prId.Contains(searchString));
            }
            

            if (lvm.isAdmin == false && ViewBag.ApprovalAuth == false)
                data = data.Where(x => x.requestDeptId == lvm.deptID && x.createdUser == lvm.userID);

            switch (sortOrder)
            {
                case "date_":
                    data = data.OrderByDescending(s => s.prDate);
                    break;
                case "voucherNo":
                    data = data.OrderBy(s => new { s.prId });
                    break;
                case "voucherNo_":
                    data = data.OrderByDescending(s => s.prId);
                    break;
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

            #region default header
            //if (lvm.isAdmin == false && ViewBag.ApprovalAuth == false)
            //    return View(db.purchaseRequestHeaders.Where(x => x.requestDeptId == lvm.deptID && x.createdUser == lvm.userID).ToList());
            //else if (lvm.isAdmin == false && ViewBag.ApprovalAuth == true)
            //    return View(db.purchaseRequestHeaders.ToList());
            //else
            //    return View(db.purchaseRequestHeaders.ToList());
            #endregion
        }

        // GET: /PurchaseRequest/Details/5
        public ActionResult Details(string id)
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
            purchaseRequestHeader purchaserequest = db.purchaseRequestHeaders.Find(id);
            if (purchaserequest == null)
            {
                return HttpNotFound();
            }
            
            loadDetail(id, 0);
            #region sweeping departmen ato divisi
            //var a = db.departments.Where(x => x.deptID == purchaserequest.requestDeptId).Select(x => new { x.deptName }).ToList(); ~oldProcess
            var a = db.divisis.Where(x => x.divisiID == purchaserequest.requestDeptId).Select(x => new { deptName = x.divisiName }).ToList();
            ViewBag.Dept = a[0].deptName;
            #endregion
            ViewBag.poHasCreated = db.purchaseOrderHeaders.ToList();
            return View(purchaserequest);
        }

        // GET: /PurchaseRequest/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            loadDetail(string.Empty, 0);
            return View();
        }

        // POST: /PurchaseRequest/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(purchaseRequestHeader purchaserequest)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.Dept = new SelectList(db.departments.Select(x => new { x.deptID, x.deptName }).ToList(), "deptID", "deptName", purchaserequest.requestDeptId);
            if (ModelState.IsValid)
            {
                bool isProposed = false;
                #region collect detail purchase request
                var countChk = 0;
                for (int i = 0; i < Request.Form.Count; i++)
                {
                    if (Request.Form.AllKeys.ToList()[i].Contains("txtProductID_"))
                        countChk++;
                    else if (Request.Form.AllKeys.ToList()[i].Contains("hdnTypeSubmit"))
                        if (Request.Form["hdnTypeSubmit"].ToString() == "requested")
                            isProposed = true;
                }

                var model = new purchaseRequestHeader();
                for (int i = 1; i <= countChk; i++)
                {
                    var colVal = Request.Form["txtProductID_" + i];
                    if (colVal == "")
                        break;

                    int t_productID = int.Parse(Request.Form["txtProductID_" + i].ToString());
                    string t_description = Request.Form["txtDescription_" + i].ToString();
                    int t_qty = Request.Form["txtQty_" + i].ToString() != "" ? Convert.ToInt32(Request.Form["txtQty_" + i].ToString().Replace('.', ',')) : 0;
                    string t_unit = Request.Form["txtUnit_" + i].ToString();
                    int? t_vendorid = int.Parse(Request.Form["txtVendorID_" + i].ToString());

                    var editor = new purchaseRequestHeader.purchaseRequestDetail()
                    {
                        productId = t_productID,
                        description = t_description,
                        qty = t_qty,
                        unit = t_unit,
                        vendorId = t_vendorid
                    };
                    model.detailPurchaseRequest.Add(editor);
                }
                ViewData["dataDetail"] = model.detailPurchaseRequest.ToList();
                #endregion 

                try
                {
                    using(TransactionScope ts = new TransactionScope())
                    {
                        string generateID = prefix + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("d2");
                        var prIDCreated = db.purchaseRequestHeaders.Where(x => x.prId.Contains(generateID)).OrderByDescending(x => x.createdDate).Select(x => x.prId).ToList();
                        if (prIDCreated.Count == 0)
                        {
                            generateID = generateID + "0001";
                        }
                        else
                        {
                            generateID = generateID + (Convert.ToInt32(prIDCreated[0].Substring((prIDCreated[0].Length - 4))) + 1).ToString().PadLeft(4, '0');
                        }

                        lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                        purchaserequest.prId = generateID;
                        purchaserequest.proposedBy = lvm.userID;
                        purchaserequest.proposedDate = DateTime.Now;
                        purchaserequest.proposedStatus = isProposed;
                        purchaserequest.createdUser = lvm.userID;
                        purchaserequest.createdDate = DateTime.Now;
                        db.purchaseRequestHeaders.Add(purchaserequest);
                        //db.SaveChanges();

                        #region insertDetail
                        for (int i = 1; i <= countChk; i++)
                        {
                            var colVal = Request.Form["txtProductID_" + i];
                            if (colVal == "")
                                break;

                            int t_productID = int.Parse(Request.Form["txtProductID_" + i].ToString());
                            string t_description = Request.Form["txtDescription_" + i].ToString();
                            int t_qty = Request.Form["txtQty_" + i].ToString() != "" ? Convert.ToInt32(Request.Form["txtQty_" + i].ToString().Replace('.', ',')) : 0;
                            string t_unit = Request.Form["txtUnit_" + i].ToString();
                            int? t_vendorid = int.Parse(Request.Form["txtVendorID_" + i].ToString());

                            var editor = new purchaseRequestHeader.purchaseRequestDetail()
                            {
                                prId = generateID,
                                productId = t_productID,
                                description = t_description,
                                qty = t_qty,
                                unit = t_unit,
                                vendorId = t_vendorid
                            };
                            db.purchaseRequestDetails.Add(editor);
                        }
                        #endregion

                        db.SaveChanges();
                        ts.Complete();
                    }
                    
                    return RedirectToAction("Index");
                }
                catch (Exception exc)
                {
                    string a = exc.Message;
                }
            }

            return View(purchaserequest);
        }

        // GET: /PurchaseRequest/Edit/5
        public ActionResult Edit(string id)
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
            purchaseRequestHeader purchaserequest = db.purchaseRequestHeaders.Find(id);
            if (purchaserequest == null)
            {
                return HttpNotFound();
            }
            
            loadDetail(id, purchaserequest.requestDeptId);
            return View(purchaserequest);
        }

        // POST: /PurchaseRequest/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(purchaseRequestHeader purchaserequest)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.Dept = new SelectList(db.departments.Select(x => new { x.deptID, x.deptName }).ToList(), "deptID", "deptName", purchaserequest.requestDeptId);
            if (ModelState.IsValid)
            {
                #region collect detail purchase request
                var countChk = 0;
                bool isProposed = false;
                for (int i = 0; i < Request.Form.Count; i++)
                {
                    if (Request.Form.AllKeys.ToList()[i].Contains("txtProductID_"))
                        countChk++;
                    else if (Request.Form.AllKeys.ToList()[i].Contains("hdnTypeSubmit"))
                        if (Request.Form["hdnTypeSubmit"].ToString() == "requested")
                            isProposed = true;
                }

                var model = new purchaseRequestHeader();
                for (int i = 1; i <= countChk; i++)
                {
                    var colVal = Request.Form["txtProductID_" + i];
                    if (colVal == "")
                        break;

                    int t_productID = int.Parse(Request.Form["txtProductID_" + i].ToString());
                    string t_description = Request.Form["txtDescription_" + i].ToString();
                    int t_qty = Request.Form["txtQty_" + i].ToString() != "" ? Convert.ToInt32(Request.Form["txtQty_" + i].ToString().Replace('.', ',')) : 0;
                    string t_unit = Request.Form["txtUnit_" + i].ToString();
                    int? t_vendorid = int.Parse(Request.Form["txtVendorID_" + i].ToString());

                    var editor = new purchaseRequestHeader.purchaseRequestDetail()
                    {
                        productId = t_productID,
                        description = t_description,
                        qty = t_qty,
                        unit = t_unit,
                        vendorId = t_vendorid
                    };
                    model.detailPurchaseRequest.Add(editor);
                }
                ViewData["dataDetail"] = model.detailPurchaseRequest.ToList();
                #endregion 

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                        purchaserequest.proposedStatus = isProposed;
                        purchaserequest.modifiedDate = DateTime.Now;
                        purchaserequest.modifiedUser = lvm.userID;
                        purchaserequest.approvedStatus = null;
                        db.Entry(purchaserequest).State = EntityState.Modified;

                        #region insertDetail
                        bool runFirst = true;
                        for (int i = 1; i <= countChk; i++)
                        {
                            if (runFirst == true)
                            {
                                db.purchaseRequestDetails.RemoveRange(db.purchaseRequestDetails.Where(x => x.prId == purchaserequest.prId));
                                runFirst = false;
                            }

                            var colVal = Request.Form["txtProductID_" + i];
                            if (colVal == "")
                                break;

                            int t_productID = int.Parse(Request.Form["txtProductID_" + i].ToString());
                            string t_description = Request.Form["txtDescription_" + i].ToString();
                            int t_qty = Request.Form["txtQty_" + i].ToString() != "" ? Convert.ToInt32(Request.Form["txtQty_" + i].ToString().Replace('.', ',')) : 0;
                            string t_unit = Request.Form["txtUnit_" + i].ToString();
                            int? t_vendorid = int.Parse(Request.Form["txtVendorID_" + i].ToString());

                            var editor = new purchaseRequestHeader.purchaseRequestDetail()
                            {
                                prId = purchaserequest.prId,
                                productId = t_productID,
                                description = t_description,
                                qty = t_qty,
                                unit = t_unit,
                                vendorId = t_vendorid
                            };
                            db.purchaseRequestDetails.Add(editor);
                        }
                        #endregion

                        db.SaveChanges();
                        ts.Complete();
                    }
                    return RedirectToAction("Index");
                }
                catch(Exception exc)
                {
                    string a = exc.Message;
                }
            }
            return View(purchaserequest);
        }

        // GET: /PurchaseRequest/Delete/5
        public ActionResult Delete(string id)
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
            purchaseRequestHeader purchaserequest = db.purchaseRequestHeaders.Find(id);
            if (purchaserequest == null)
            {
                return HttpNotFound();
            }

            loadDetail(id, 0);
            #region sweeping departmen ato divisi
            //var a = db.departments.Where(x => x.deptID == purchaserequest.requestDeptId).Select(x => new { x.deptName }).ToList(); ~oldProcess
            var a = db.divisis.Where(x => x.divisiID == purchaserequest.requestDeptId).Select(x => new { deptName = x.divisiName }).ToList();
            ViewBag.Dept = a[0].deptName;
            #endregion
            return View(purchaserequest);
        }

        // POST: /PurchaseRequest/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            purchaseRequestHeader purchaserequest = db.purchaseRequestHeaders.Find(id);
            db.purchaseRequestHeaders.Remove(purchaserequest);
            db.purchaseRequestDetails.RemoveRange(db.purchaseRequestDetails.Where(x => x.prId == id));
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

        protected void loadDetail(string id, int deptId)
        {
            #region default header
            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            var usr = db.Users.Where(x => x.userID == lvm.userID).ToList();
            if(deptId == 0)
                deptId = usr[0].deptID;

            if (lvm.isAdmin == false)
                //ViewBag.Dept = new SelectList(db.departments.Where(x=>x.deptID==deptId).Select(x => new { x.deptID, x.deptName }).ToList(), "deptID", "deptName");
                ViewBag.Dept = new SelectList(db.divisis.Where(x => x.divisiID == deptId).Select(x => new { deptID = x.divisiID, deptName = x.divisiName }).ToList(), "deptID", "deptName");
            else
                //ViewBag.Dept = new SelectList(db.departments.Select(x => new { x.deptID, x.deptName }).ToList(), "deptID", "deptName");
                ViewBag.Dept = new SelectList(db.divisis.Select(x => new { deptID = x.divisiID, deptName = x.divisiName }).ToList(), "deptID", "deptName");
            #endregion

            var popUp = db.products.ToList();
            var modelProd = new product();
            if (popUp != null)
            {
                for (int i = 0; i < popUp.Count; i++ )
                {
                    var editor = new product.listProduct()
                    {
                        productID = popUp[i].productID,
                        productCode = popUp[i].productCode,
                        productName = popUp[i].productName,
                        unit = popUp[i].unit
                    };
                    modelProd.productList.Add(editor);
                }
                ViewBag.ProductPopUp = modelProd.productList.ToList();
            }
            ViewBag.totalRows = popUp.Count;

            ViewBag.VendorPopUp = db.vendors.ToList();
            ViewBag.totalRowsVendor = db.vendors.Count();
            
            var prod = new product();
            foreach (var g in db.products.ToList() as IEnumerable<Models.product>)
            {
                var editor = new product.listProduct()
                {
                    productID = g.productID,
                    productCode = g.productCode,
                    productName = g.productName,
                    unit = g.unit,
                    createdUser = g.createdUser,
                    createdDate = g.createdDate,
                    modifiedUser = g.modifiedUser,
                    modifiedDate = g.modifiedDate
                };
                prod.productList.Add(editor);
            }

            if(id != string.Empty)
            {
                var model = new purchaseRequestHeader();
                var tempDetail = db.purchaseRequestDetails.Where(x => x.prId == id).ToList();
                foreach (var a in tempDetail)
                {
                    int t_productID = a.productId;
                    string t_description = a.description.ToString();
                    int t_qty = a.qty.ToString() != "" ? Convert.ToInt32(a.qty.ToString().Replace('.', ',')) : 0;
                    string t_unit = a.unit.ToString();
                    int? t_vendorid = a.vendorId;

                    var editor = new purchaseRequestHeader.purchaseRequestDetail()
                    {
                        productId = t_productID,
                        description = t_description,
                        qty = t_qty,
                        unit = t_unit,
                        vendorId = t_vendorid
                    };
                    model.detailPurchaseRequest.Add(editor);
                }
                ViewData["dataDetail"] = model.detailPurchaseRequest.ToList();

                var g = db.products
                    .Join(db.purchaseRequestDetails.Where(x => x.prId == id), y => y.productID, z => z.productId, (y, z) => new { y })
                    .Select(c => new { c.y.productID, c.y.productCode, c.y.productName, c.y.unit, c.y.createdUser, c.y.createdDate, c.y.modifiedUser, c.y.modifiedDate }).ToList();

                prod.productList.Clear();
                for(int i=0; i<g.Count; i++)
                {
                    var editor = new product.listProduct()
                    {                        
                        productID = g[i].productID,
                        productCode = g[i].productCode,
                        productName = g[i].productName,
                        unit = g[i].unit,
                        createdUser = g[i].createdUser,
                        createdDate = g[i].createdDate,
                        modifiedUser = g[i].modifiedUser,
                        modifiedDate = g[i].modifiedDate
                    };
                    prod.productList.Add(editor);
                }
            }
            ViewBag.product = prod.productList.ToList();
        }
        
        public ActionResult Approval(string id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;

            ViewBag.ApprovalAuth = false;
            foreach (string b in lvm.listAuthorize as List<string>)
            {
                string[] auth = b.Split('_');
                if (auth[0].ToString() == "A1")
                    if (b.Contains((Url.Action("")).Replace("/", "")))
                    {
                        ViewBag.ApprovalAuth = true;
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
            purchaseRequestHeader purchaserequest = db.purchaseRequestHeaders.Find(id);
            if (purchaserequest == null)
            {
                return HttpNotFound();
            }

            loadDetail(id, 0);
            #region sweeping departmen ato divisi
            //var a = db.departments.Where(x => x.deptID == purchaserequest.requestDeptId).Select(x => new { x.deptName }).ToList(); ~oldProcess
            var a = db.divisis.Where(x => x.divisiID == purchaserequest.requestDeptId).Select(x => new { deptName = x.divisiName }).ToList();
            ViewBag.Dept = a[0].deptName;
            #endregion
            return View(purchaserequest);
        }
    }
}
