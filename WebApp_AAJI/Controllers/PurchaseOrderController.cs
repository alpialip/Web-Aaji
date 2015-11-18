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
using System.Data.SqlClient;

namespace WebApp_AAJI.Controllers
{
    public class PurchaseOrderController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private CommonController ccm = new CommonController();
        private ConnectionController con = new ConnectionController();
        string prefix = "POD";
        int pageStarted = 1;
        int maxRows = 10;

        #region unused
        //public ActionResult loadTOPMultiple(string id)
        //{
        //    var a = db.vendors.Where(x => x.vendorID == id).ToList();
        //    string returnVal = string.Empty;
        //    foreach (var item in a as IEnumerable<vendor>)
        //    {
        //        returnVal = item.address1 + "|" + item.contactPerson + "|" + item.telp;
        //    }
        //    return PartialView("_PartialPageMultipleTOP");
        //}

        //[HttpGet]
        //public async Task<ActionResult> loadPopUpPR()
        //{
        //    var x = db.purchaseRequestHeaders.AsNoTracking()
        //                        .Join(db.departments, a => a.requestDeptId, b => b.deptID, (a, b) => new { a, b })
        //                        .Select(c => new { c.a.prId, c.a.requestDeptId, c.b.deptName, c.a.typeOrder, c.a.proposalInclude, c.a.specialInstruction, c.a.projectTimeDelivery }).ToList();

        //    ViewData["PRPopUp"] = x;

        //    var model = new purchaseOrderHeader();
        //    var editor = new purchaseOrderHeader.prPopUp()
        //    {
        //        prId = x[0].prId,
        //        requestDeptId = x[0].requestDeptId,
        //        deptName = x[0].deptName,
        //        typeOrder = x[0].typeOrder,
        //        proposalInclude = x[0].proposalInclude,
        //        specialInstruction = x[0].specialInstruction,
        //        projectTimeDelivery = x[0].projectTimeDelivery
        //    };
        //    model.PopUpPR.Add(editor);
        //    ViewBag.PRPopUp = model.PopUpPR.ToList();

        //    return PartialView("_PartialPagePR");
        //}
        #endregion

        [HttpGet]
        public async Task<ActionResult> ApprovalProcess(string act, string id)
        {
            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            purchaseOrderHeader model = db.purchaseOrderHeaders.Find(id);
            model.approvedStatus = act == "approved" ? true : false;
            model.approvedDate = DateTime.Now;
            model.approvedBy = lvm.userID;
            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult GetDetailVendor(int id)
        {
            var a = db.vendors.Where(x => x.id == id).ToList();
            string returnVal = string.Empty;
            foreach(var item in a as IEnumerable<vendor>)
            {
                returnVal = item.address1 + "|" + item.contactPerson + "|" + item.telp;
            }
            return Content(returnVal);
        }

        [HttpGet]
        public async Task<ActionResult> LoadDetailPRSearch(string keywordId, string keywordSearch /*, int maxRows*/, int pageStart)
        {
            #region oldProcess
            //string sql = "with cte as ( ";
            //sql += "SELECT ROW_NUMBER() OVER(ORDER BY prId) AS rn, a.prId, a.requestDeptId, b.deptName, a.typeOrder, a.proposalInclude, a.specialInstruction, a.projectTimeDelivery ";
            //sql += "FROM [dbo].[PurchaseRequestHeaders] a WITH(NOLOCK)  ";
            //sql += "INNER JOIN [dbo].[Departments] b ON b.deptID = a.requestDeptId ";
            //sql += "WHERE a.prId NOT IN (SELECT prId FROM [dbo].PurchaseOrderHeaders) ";
            //sql += "AND a.proposedStatus = 1";
            //if (keywordId != string.Empty && keywordSearch != string.Empty)
            //{
            //    if (keywordId == "a.proposalInclude")
            //        sql += "AND " + keywordId + " =  " + keywordSearch;
            //    else
            //        sql += "AND " + keywordId + " LIKE  '%" + keywordSearch + "%' ";
            //}
            //sql += ") ";
            //sql += "SELECT * FROM cte ";
            //string sqlAll = sql;
            //sql += "WHERE rn BETWEEN " + ((pageStart * maxRows) - (maxRows - 1)) + " AND " + (pageStart * maxRows);

            //DataTable dataAll = con.executeReader(sqlAll);
            //ViewBag.totalRows = dataAll.Rows.Count;
            //ViewBag.currentPage = pageStart;

            //DataTable popUp = con.executeReader(sql);
            //var model = new purchaseOrderHeader();
            //if (popUp != null)
            //{
            //    foreach (DataRow dr in popUp.Rows)
            //    {
            //        var editor = new purchaseOrderHeader.prPopUp()
            //        {
            //            prId = dr["prId"].ToString(),
            //            requestDeptId = int.Parse(dr["requestDeptId"].ToString()),
            //            deptName = dr["deptName"].ToString(),
            //            typeOrder = dr["typeOrder"].ToString(),
            //            proposalInclude = bool.Parse(dr["proposalInclude"].ToString()),
            //            specialInstruction = dr["specialInstruction"].ToString(),
            //            projectTimeDelivery = dr["projectTimeDelivery"].ToString()
            //        };
            //        model.PopUpPR.Add(editor);
            //    }
            //    ViewBag.PRPopUp = model.PopUpPR.ToList();
            //}
            #endregion

            #region newProcess
                getListPR(keywordId, keywordSearch, maxRows, pageStart, true);
            #endregion

            return PartialView("_PartialPagePRSearchSub");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDetailPR(string prId)
        {

            var model = new purchaseRequestHeader();
            foreach(var a in db.purchaseRequestDetails.Where(x=>x.prId==prId).ToList() as IEnumerable<purchaseRequestHeader.purchaseRequestDetail>)
            {
                var editor = new purchaseRequestHeader.purchaseRequestDetail()
                {
                    productId = a.productId,
                    description = a.description,
                    qty = a.qty,
                    unit = a.unit
                };
                model.detailPurchaseRequest.Add(editor);
            }
            ViewData["dataDetail"] = model.detailPurchaseRequest.ToList();

            return PartialView("_PartialPagePR-PO");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDetailPR_byVendorID(string prId, int vendorID)
        {
            var model = new purchaseRequestHeader();
            #region oldProcess
            //foreach (var a in db.purchaseRequestDetails.Where(x => x.prId == prId).ToList() as IEnumerable<purchaseRequestHeader.purchaseRequestDetail>)
            //{
            //    foreach(var b in db.transVendorProducts.Where(x=>x.id_vendor==vendorID).ToList() as IEnumerable<transVendorProduct>)
            //    { 
            //        if(b.productID == a.productId)
            //        { 
            //            var editor = new purchaseRequestHeader.purchaseRequestDetail()
            //            {
            //                productId = a.productId,                            
            //                description = a.description,
            //                qty = a.qty,
            //                unit = a.unit
            //            };
            //            model.detailPurchaseRequest.Add(editor);
            //            break;
            //        }
            //    }
            //}
            #endregion

            #region new Process
            string sql = "SELECT DISTINCT b.* ";
            sql += "FROM [dbo].[transVendorProducts] a ";
            sql += "INNER JOIN [dbo].[PurchaseRequestDetails] b ON b.productID = a.productID ";
            sql += "INNER JOIN [dbo].[Vendors] c ON c.id = a.id_vendor ";
            sql += "LEFT JOIN (SELECT x.*, y.prId ";
            sql += "	FROM [dbo].[PurchaseOrderDetails] x ";
            sql += "	INNER JOIN [dbo].[PurchaseOrderHeaders] y ON y.poId = x.poId) d ON d.prId = b.prId AND d.productID != b.productID ";
            sql += "WHERE b.prId = '" + prId + "'";
            DataTable dtListAuthorize = con.executeReader(sql);
            foreach (DataRow dr in dtListAuthorize.Rows)
            {
                foreach(var b in db.transVendorProducts.Where(x=>x.id_vendor==vendorID).ToList() as IEnumerable<transVendorProduct>)
                {
                    if (b.productID == int.Parse(dr["productID"].ToString()))
                    {
                        var editor = new purchaseRequestHeader.purchaseRequestDetail()
                        {
                            productId = int.Parse(dr["productID"].ToString()),
                            description = dr["description"].ToString(),
                            qty = int.Parse(dr["qty"].ToString()),
                            unit = dr["unit"].ToString()
                        };
                        model.detailPurchaseRequest.Add(editor);
                        break;
                    }
                }
            }
            #endregion
            ViewData["dataDetail"] = model.detailPurchaseRequest.ToList();

            ViewBag.product = db.products.ToList();
            return PartialView("_PartialPagePR-PO");
        }

        [HttpGet]
        public async Task<ActionResult> LoadRefreshVendor(string prId)
        {

            ViewBag.ddlVendor = DDLVendorByPRId(prId);

            return PartialView("_PartialPageVendor");
        }

        [HttpGet]
        public async Task<ActionResult> saveMultipleTOP(string dataDetail, string poId)
        {
            string[] data = dataDetail.Split('|');

            var model = new purchaseOrderHeader();
            for (int i = 0; i < data.Count(); i++)
            {
                string[] value = data[i].Split(';');

                if (value[1].ToString() == "")
                    break;

                DateTime t_BuyDate = Convert.ToDateTime(value[1].ToString());
                decimal t_BuyPercent =Convert.ToDecimal(Convert.ToDouble(value[2].ToString().Replace('.', ',')));

                var editor = new purchaseOrderHeader.purchaseOrderTop()
                {
                    id = (i+1),
                    poId = poId,
                    buyDate = t_BuyDate,
                    buyPercent = t_BuyPercent
                };
                model.detailPurchaseTop.Add(editor);
            }
            ViewData["dataDetailMultiple"] = model.detailPurchaseTop.ToList();
            Session["topMultiple"] = model.detailPurchaseTop.ToList();
            return PartialView("_PartialPageMultipleTOP");
        }

        // GET: /PurchaseOrder/
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

            ViewBag.rcvHasCreated = db.purchaseReceives.ToList();
            ViewBag.vendor = db.vendors.ToList();

            string sql = "SELECT d.poId ";
            sql += "FROM [dbo].[AccountPayableDetails] a ";
            sql += "INNER JOIN [dbo].[PurchaseInvoices] b ON b.purchaseInvoiceNo = a.invoiceNo ";
            sql += "INNER JOIN [dbo].[PurchaseReceives] c ON c.receiveNo = b.receiveNo  AND c.receivedStatus = 1 ";
            sql += "INNER JOIN [dbo].[PurchaseOrderHeaders] d ON d.poId = c.poId ";

            DataTable dataAll = con.executeReader(sql);
            ViewBag.donePayment = dataAll;
            
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

            var data = from s in db.purchaseOrderHeaders select s;
            if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate) && !String.IsNullOrEmpty(searchString))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.poDate >= sDate && s.poDate <= eDate && s.poId.Contains(searchString));
            }
            else if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.poDate >= sDate && s.poDate <= eDate);
            }
            else if (!String.IsNullOrEmpty(searchString))
            {
                data = data.Where(s => s.poId.Contains(searchString));
            }
            

            if (lvm.isAdmin == false && ViewBag.ApprovalAuth == false)
                data = data.Where(x => x.createdUser == lvm.userID);

            switch (sortOrder)
            {
                case "date_":
                    data = data.OrderByDescending(s => s.poDate);
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

            #region output Index
            //if (lvm.isAdmin == false && ViewBag.ApprovalAuth == false)
            //    return View(db.purchaseOrderHeaders.Where(x => x.createdUser == lvm.userID).ToList());
            //else if (lvm.isAdmin == false && ViewBag.ApprovalAuth == true)
            //    return View(db.purchaseOrderHeaders.ToList());
            //else
            //    return View(db.purchaseOrderHeaders.ToList());
            #endregion
        }

        // GET: /PurchaseOrder/Details/5
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
            purchaseOrderHeader purchaseorderheader = db.purchaseOrderHeaders.Find(id);
            if (purchaseorderheader == null)
            {
                return HttpNotFound();
            }

            loadPO(purchaseorderheader.poId);
            ViewBag.rcvHasCreated = db.purchaseReceives.ToList();

            return View(purchaseorderheader);
        }

        // GET: /PurchaseOrder/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            ViewBag.ddlVendor = DDLVendor(string.Empty);

            #region oldProcess getListPR
            //var popUp = db.purchaseRequestHeaders.AsNoTracking().Where(x=> !db.purchaseOrderHeaders.Any(y=>y.prId==x.prId) && x.proposedStatus == true && x.approvedStatus == true)
            //                    //.Join(db.departments, a => a.requestDeptId, b => b.deptID, (a, b) => new { a, b }) ~old process ~sweeping department kebalik dgn divisi
            //                    .Join(db.divisis, a => a.requestDeptId, b => b.divisiID, (a, b) => new {a,b})
            //                    .Select(c => new { c.a.prId, c.a.requestDeptId, deptName = c.b.divisiName, c.a.typeOrder, c.a.proposalInclude, c.a.specialInstruction, c.a.projectTimeDelivery }).ToList();

            //var model = new purchaseOrderHeader();
            //for(int i=0; i<popUp.Count; i++)
            //{ 
            //    var editor = new purchaseOrderHeader.prPopUp()
            //    {
            //        prId = popUp[i].prId,
            //        requestDeptId = popUp[i].requestDeptId,
            //        deptName = popUp[i].deptName,
            //        typeOrder = popUp[i].typeOrder,
            //        proposalInclude = popUp[i].proposalInclude,
            //        specialInstruction = popUp[i].specialInstruction,
            //        projectTimeDelivery = popUp[i].projectTimeDelivery
            //    };
            //    model.PopUpPR.Add(editor);
            //}
            //ViewBag.PRPopUp = model.PopUpPR.ToList();
            //ViewBag.totalRows = popUp.Count;
            #endregion
            #region newProcess getListPR
                getListPR(null, null, maxRows, pageStarted, true);
            #endregion

            Session["topMultiple"] = null;
            return View();
        }

        // POST: /PurchaseOrder/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(purchaseOrderHeader purchaseorderheader)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            //ViewBag.ddlVendor = DDLVendor(string.Empty);
            ViewBag.ddlVendor = DDLVendorByPRId(purchaseorderheader.prId);
            var xz = db.purchaseRequestHeaders.AsNoTracking().Where(x => !db.purchaseOrderHeaders.Any(y => y.prId == x.prId) && x.proposedStatus == true && x.approvedStatus == true)
                                //.Join(db.departments, a => a.requestDeptId, b => b.deptID, (a, b) => new { a, b }) ~old process ~sweeping department kebalik dgn divisi
                                .Join(db.divisis, a => a.requestDeptId, b => b.divisiID, (a, b) => new { a, b })
                                .Select(c => new { c.a.prId, c.a.requestDeptId, deptName = c.b.divisiName, c.a.typeOrder, c.a.proposalInclude, c.a.specialInstruction, c.a.projectTimeDelivery }).ToList();

            ViewData["PRPopUp"] = xz;

            var model = new purchaseOrderHeader();
            var editor = new purchaseOrderHeader.prPopUp()
            {
                prId = xz[0].prId,
                requestDeptId = xz[0].requestDeptId,
                deptName = xz[0].deptName,
                typeOrder = xz[0].typeOrder,
                proposalInclude = xz[0].proposalInclude,
                specialInstruction = xz[0].specialInstruction,
                projectTimeDelivery = xz[0].projectTimeDelivery
            };
            model.PopUpPR.Add(editor);
            ViewBag.PRPopUp = model.PopUpPR.ToList();
            
            var modelx = new purchaseRequestHeader();
            foreach (var a in db.purchaseRequestDetails.Where(x => x.prId == purchaseorderheader.prId).ToList() as IEnumerable<purchaseRequestHeader.purchaseRequestDetail>)
            {
                var editorx = new purchaseRequestHeader.purchaseRequestDetail()
                {
                    productId = a.productId,
                    description = a.description,
                    qty = a.qty,
                    unit = a.unit
                };
                modelx.detailPurchaseRequest.Add(editorx);
            }
            ViewData["dataDetail"] = modelx.detailPurchaseRequest.ToList();

            if (ModelState.IsValid)
            {
                var countChk = 0;
                var countChkMultiple = 0;
                bool isProposed = false;
                for (int i = 0; i < Request.Form.Count; i++)
                {
                    if (Request.Form.AllKeys.ToList()[i].Contains("txtProductID_"))
                        countChk++;
                    else if (Request.Form.AllKeys.ToList()[i].Contains("hdnTypeSubmit"))
                        if (Request.Form["hdnTypeSubmit"].ToString() == "requested")
                            isProposed = true;

                    if (purchaseorderheader.topType == "Multiple")
                        if (Request.Form.AllKeys.ToList()[i].Contains("txtBuyDate_"))
                            countChkMultiple++;
                }

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        string generateID = prefix + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("d2");
                        var prIDCreated = db.purchaseOrderHeaders.Where(x => x.poId.Contains(generateID)).OrderByDescending(x => x.createdDate).Select(x => x.poId).ToList();
                        if (prIDCreated.Count == 0)
                        {
                            generateID = generateID + "0001";
                        }
                        else
                        {
                            generateID = generateID + (Convert.ToInt32(prIDCreated[0].Substring((prIDCreated[0].Length - 4))) + 1).ToString().PadLeft(4, '0');
                        }

                        #region ubah vendorID ke vendorCode
                        int idVendor = 0;
                        try{
                            idVendor = int.Parse(purchaseorderheader.vendorId) * 1;
                        }
                        catch
                        {

                        }

                        if(idVendor > 0 )
                        {
                            var ven = db.vendors.AsNoTracking().Where(x => x.id == idVendor).ToList();
                            purchaseorderheader.vendorId = ven[0].vendorID;
                        }
                        #endregion
                        purchaseorderheader.poId = generateID;
                        lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                        purchaseorderheader.createdDate = DateTime.Now;
                        purchaseorderheader.createdUser = lvm.userID;
                        purchaseorderheader.proposedBy = lvm.userID;
                        purchaseorderheader.proposedDate = DateTime.Now;
                        purchaseorderheader.proposedStatus = isProposed;
                        db.purchaseOrderHeaders.Add(purchaseorderheader);

                        #region insertDetail
                        for (int i = 1; i <= countChk; i++)
                        {
                            var colVal = Request.Form["txtProductID_" + i];
                            if (colVal == "")
                                break;

                            int t_productID = int.Parse(Request.Form["txtProductID_" + i].ToString());
                            int t_qty = Request.Form["txtQty_" + i].ToString() != "" ? Convert.ToInt32(Request.Form["txtQty_" + i].ToString().Replace('.', ',')) : 0;
                            decimal t_price = Request.Form["txtPrice_" + i].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(Request.Form["txtPrice_" + i].ToString().Replace('.', ','))) : 0;
                            decimal t_disc = Request.Form["txtDisc_" + i].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(Request.Form["txtDisc_" + i].ToString().Replace('.', ','))) : 0;

                            var editorx = new purchaseOrderHeader.purchaseOrderDetail()
                            {
                                poId = purchaseorderheader.poId,
                                productID = t_productID,
                                qty = t_qty,
                                price = t_price,
                                disc = t_disc
                            };
                            db.purchaseOrderDetails.Add(editorx);
                        }
                        #endregion

                        #region insertTOPMultiple
                        if(purchaseorderheader.topType == "Multiple")
                        { 
                            for (int i = 0; i < countChkMultiple; i++)
                            {
                                var colVal = Request.Form["txtBuyDate_" + i];
                                if (colVal == "")
                                    break;

                                DateTime t_BuyDate = Convert.ToDateTime(Request.Form["txtBuyDate_" + i].ToString());
                                decimal t_BuyPercent = Request.Form["txtBuyPercent_" + i].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(Request.Form["txtBuyPercent_" + i].ToString().Replace('.', ','))) : 0;

                                var editort = new purchaseOrderHeader.purchaseOrderTop()
                                {
                                    poId = purchaseorderheader.poId,
                                    buyDate = t_BuyDate,
                                    buyPercent = t_BuyPercent
                                };
                                db.purchaseOrderTOPs.Add(editort);
                            }
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
            return View(purchaseorderheader);
        }

        // GET: /PurchaseOrder/Edit/5
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
            purchaseOrderHeader purchaseorderheader = db.purchaseOrderHeaders.Find(id);
            if (purchaseorderheader == null)
            {
                return HttpNotFound();
            }

            //ViewBag.ddlVendor = DDLVendor(purchaseorderheader.vendorId);
            ViewBag.ddlVendor = DDLVendorByPOId(purchaseorderheader.poId);
            loadPO(purchaseorderheader.poId);

            return View(purchaseorderheader);
        }

        // POST: /PurchaseOrder/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(purchaseOrderHeader purchaseorderheader)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            #region 
            //ViewBag.ddlVendor = DDLVendor(purchaseorderheader.vendorId);
            ViewBag.ddlVendor = DDLVendorByPRId(purchaseorderheader.prId);
            loadPO(purchaseorderheader.poId);
            #endregion

            if (ModelState.IsValid)
            {
                var countChk = 0;
                var countChkMultiple = 0;
                bool isProposed = false;
                for (int i = 0; i < Request.Form.Count; i++)
                {
                    if (Request.Form.AllKeys.ToList()[i].Contains("txtProductID_"))
                        countChk++;
                    else if (Request.Form.AllKeys.ToList()[i].Contains("hdnTypeSubmit"))
                        if (Request.Form["hdnTypeSubmit"].ToString() == "requested")
                            isProposed = true;

                    if (purchaseorderheader.topType == "Multiple")
                        if (Request.Form.AllKeys.ToList()[i].Contains("txtBuyDate_"))
                            countChkMultiple++;
                }

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                        purchaseorderheader.proposedStatus = isProposed;
                        purchaseorderheader.modifiedDate = DateTime.Now;
                        purchaseorderheader.modifiedUser = lvm.userID;
                        db.Entry(purchaseorderheader).State = EntityState.Modified;

                        #region insertDetail
                        bool runFirst = true;
                        for (int i = 1; i <= countChk; i++)
                        {
                            if (runFirst == true)
                            {
                                db.purchaseOrderDetails.RemoveRange(db.purchaseOrderDetails.Where(x => x.poId == purchaseorderheader.poId));
                                runFirst = false;
                            }

                            var colVal = Request.Form["txtProductID_" + i];
                            if (colVal == "")
                                break;

                            int t_productID = int.Parse(Request.Form["txtProductID_" + i].ToString());
                            int t_qty = Request.Form["txtQty_" + i].ToString() != "" ? Convert.ToInt32(Request.Form["txtQty_" + i].ToString().Replace('.', ',')) : 0;
                            decimal t_price = Request.Form["txtPrice_" + i].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(Request.Form["txtPrice_" + i].ToString().Replace('.', ','))) : 0;
                            decimal t_disc = Request.Form["txtDisc_" + i].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(Request.Form["txtDisc_" + i].ToString().Replace('.', ','))) : 0;

                            var editorx = new purchaseOrderHeader.purchaseOrderDetail()
                            {
                                poId = purchaseorderheader.poId,
                                productID = t_productID,
                                qty = t_qty,
                                price = t_price,
                                disc = t_disc
                            };
                            db.purchaseOrderDetails.Add(editorx);
                        }
                        #endregion

                        #region insertTOPMultiple
                        if (purchaseorderheader.topType == "Multiple")
                        { 
                            runFirst = true;
                            for (int i = 0; i <= countChkMultiple; i++)
                            {
                                if (runFirst == true)
                                {
                                    db.purchaseOrderTOPs.RemoveRange(db.purchaseOrderTOPs.Where(x => x.poId == purchaseorderheader.poId));
                                    runFirst = false;
                                }
                                var colVal = Request.Form["txtBuyDate_" + i];
                                if (colVal == "")
                                    break;

                                DateTime t_BuyDate = Convert.ToDateTime(Request.Form["txtBuyDate_" + i].ToString());
                                decimal t_BuyPercent = Request.Form["txtBuyPercent_" + i].ToString() != "" ? Convert.ToDecimal(Convert.ToDouble(Request.Form["txtBuyPercent_" + i].ToString().Replace('.', ','))) : 0;

                                var editort = new purchaseOrderHeader.purchaseOrderTop()
                                {
                                    poId = purchaseorderheader.poId,
                                    buyDate = t_BuyDate,
                                    buyPercent = t_BuyPercent
                                };
                                db.purchaseOrderTOPs.Add(editort);
                            }
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
            return View(purchaseorderheader);
        }

        // GET: /PurchaseOrder/Delete/5
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
            purchaseOrderHeader purchaseorderheader = db.purchaseOrderHeaders.Find(id);
            if (purchaseorderheader == null)
            {
                return HttpNotFound();
            }

            loadPO(purchaseorderheader.poId);
            return View(purchaseorderheader);
        }

        // POST: /PurchaseOrder/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            purchaseOrderHeader purchaseorderheader = db.purchaseOrderHeaders.Find(id);
            db.purchaseOrderHeaders.Remove(purchaseorderheader);
            db.purchaseOrderDetails.RemoveRange(db.purchaseOrderDetails.Where(x => x.poId==id));
            db.purchaseOrderTOPs.RemoveRange(db.purchaseOrderTOPs.Where(x => x.poId == id));
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
        
        public SelectList DDLVendor(string currSelection)
        {
            var ven = db.vendors.ToList();

            List<SelectListItem> list = new List<SelectListItem>();
            foreach(var a in ven as IEnumerable<Models.vendor>)
            {
                //if(currSelection == a.vendorID)
                //    list.Add(new SelectListItem { Value = a.vendorID, Text = a.vendorName, Selected = true });
                //else
                    list.Add(new SelectListItem { Value = a.vendorID, Text = a.vendorName});
            }

            ViewBag.VendorDetail = ven;
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList DDLVendorByPRId(string prId)
        {
            string currSelection = string.Empty;
            List<SelectListItem> list = new List<SelectListItem>();

            #region oldProcess
            //var ven = db.transVendorProducts
            //          .Join(db.purchaseRequestDetails.Where(x => x.prId == prId), a => a.productID, b => b.productId, (a, b) => new { a, b })
            //          .Join(db.vendors, c => c.a.id_vendor, d => d.id, (c, d) => new { c, d })
            //          .Select(x=> new {vendorID = x.c.a.id_vendor, vendorName = x.d.vendorName})
            //          .Distinct()
            //          .ToList();

            //for (int i = 0; i < ven.Count(); i++ )
            //{
            //    list.Add(new SelectListItem { Value = ven[i].vendorID.ToString(), Text = ven[i].vendorName });
            //}
            //ViewBag.VendorDetail = ven;
            #endregion

            #region new Process
                string sql = "SELECT DISTINCT id_vendor as vendorID, vendorName ";
                        sql += "FROM [dbo].[transVendorProducts] a ";
                        sql += "INNER JOIN [dbo].[PurchaseRequestDetails] b ON b.productID = a.productID ";
                        sql += "INNER JOIN [dbo].[Vendors] c ON c.id = a.id_vendor ";
                        sql += "LEFT JOIN (SELECT x.*, y.prId ";
		                        sql += "	FROM [dbo].[PurchaseOrderDetails] x ";
                                sql += "	INNER JOIN [dbo].[PurchaseOrderHeaders] y ON y.poId = x.poId) d ON d.prId = b.prId AND d.productID != b.productID ";
                        sql += "WHERE b.prId = '"+prId+"'";
            DataTable dtListAuthorize = con.executeReader(sql);
            foreach (DataRow dr in dtListAuthorize.Rows)
            {
                list.Add(new SelectListItem { Value = dr["vendorID"].ToString(), Text = dr["vendorName"].ToString() });
            }
            //ViewBag.VendorDetail = ven;
            #endregion
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public SelectList DDLVendorByPOId(string poId)
        {
            string currSelection = string.Empty;

            var po = db.purchaseOrderHeaders.AsNoTracking().Where(x => x.poId == poId)
                    .Join(db.vendors,a=>a.vendorId,b=>b.vendorID, (a,b)=>new{a,b})
                    .ToList();
            if (po.Count() > 0)
            {
                currSelection = po[0].b.id.ToString();
            }
            List<SelectListItem> list = new List<SelectListItem>();

            #region new Process
            string sql = "SELECT DISTINCT id_vendor as vendorID, vendorName ";
            sql += "FROM [dbo].[transVendorProducts] a ";
            sql += "INNER JOIN [dbo].[PurchaseOrderDetails] b ON b.productID = a.productID ";
            sql += "INNER JOIN [dbo].[Vendors] c ON c.id = a.id_vendor ";
            sql += "WHERE b.poId = '" + poId + "'";
            DataTable dtListAuthorize = con.executeReader(sql);
            foreach (DataRow dr in dtListAuthorize.Rows)
            {
                list.Add(new SelectListItem { Value = dr["vendorID"].ToString(), Text = dr["vendorName"].ToString() });
            }
            //ViewBag.VendorDetail = ven;
            #endregion
            return new SelectList(list, "Value", "Text", currSelection);
        }

        public void loadPO(string poId)
        {
            var detailPR = db.purchaseRequestHeaders.Where(x => x.proposedStatus == true && x.approvedStatus == true).AsNoTracking()
                               //.Join(db.departments, a => a.requestDeptId, b => b.deptID, (a, b) => new { a, b }) ~old process ~sweeping department kebalik dgn divisi
                               .Join(db.divisis, a => a.requestDeptId, b => b.divisiID, (a, b) => new { a, b })
                               .Join(db.purchaseOrderHeaders.Where(po=>po.poId == poId), e=>e.a.prId, d=>d.prId,(e,d) => new{d,e})
                               .Select(c => new { c.e.a.prId, c.e.a.requestDeptId, deptName = c.e.b.divisiName, c.e.a.typeOrder, c.e.a.proposalInclude, c.e.a.specialInstruction, c.e.a.projectTimeDelivery, c.d.vendorId }).ToList();
            var vendorId = "";
            //var model = new purchaseOrderHeader();
            for (int i = 0; i < detailPR.Count; i++)
            {
                //var editor = new purchaseOrderHeader.prPopUp()
                //{
                ViewBag.From = detailPR[i].deptName;
                ViewBag.poUrgent = detailPR[i].typeOrder;
                ViewBag.Proposal = detailPR[i].proposalInclude;
                ViewBag.Instruction = detailPR[i].specialInstruction;
                ViewBag.Delivery = detailPR[i].projectTimeDelivery;
                vendorId = detailPR[i].vendorId;
                //};
                //model.PopUpPR.Add(editor);
            }
            //ViewBag.PRPopUp = model.PopUpPR.ToList();

            var topMultipleSaved = db.purchaseOrderTOPs.AsNoTracking().Where(x => x.poId == poId).ToList();
            Session["topMultiple"] = topMultipleSaved;

            var poH = db.purchaseOrderHeaders.AsNoTracking().Where(x => x.poId == poId)
                                .Join(db.vendors, a => a.vendorId, b => b.vendorID, (a, b) => new { a, b }).ToList();
            
            string userId = string.Empty;
            string prId = string.Empty;
            for (int i = 0; i < poH.Count; i++)
            {
                ViewBag.vendorName = poH[i].b.vendorName;
                ViewBag.Address = poH[i].b.address1;
                ViewBag.Cp = poH[i].b.contactPerson;
                ViewBag.Telp = poH[i].b.telp;
                ViewData["topTypes"] = poH[i].a.topType;
                ViewBag.Disc = poH[i].a.disc;
                ViewBag.Ppn = poH[i].a.ppn;
                userId = poH[i].a.proposedBy;
                prId = poH[i].a.prId;
            }
            ViewBag.userName = ccm.getUserName(userId);

            #region oldProcess
            //var model = new purchaseRequestHeader();
            //foreach (var a in db.purchaseRequestDetails.Where(x => x.prId == prId).ToList() as IEnumerable<purchaseRequestHeader.purchaseRequestDetail>)
            //{
            //    var editor = new purchaseRequestHeader.purchaseRequestDetail()
            //    {
            //        productId = a.productId,
            //        description = a.description,
            //        qty = a.qty,
            //        unit = a.unit
            //    };
            //    model.detailPurchaseRequest.Add(editor);
            //}
            //ViewData["dataDetail"] = model.detailPurchaseRequest.ToList();
            #endregion
            #region newProcess
            int id_vendor = 0;
            var ven = db.vendors.Where(x => x.vendorID == vendorId).ToList();
            if (ven.Count() > 0)
                id_vendor = ven[0].id;

            var model = new purchaseRequestHeader();
            string sql = "SELECT DISTINCT b.* ";
            sql += "FROM [dbo].[transVendorProducts] a ";
            sql += "INNER JOIN [dbo].[PurchaseRequestDetails] b ON b.productID = a.productID ";
            sql += "INNER JOIN [dbo].[Vendors] c ON c.id = a.id_vendor ";
            sql += "INNER JOIN (SELECT x.*, y.prId ";
            sql += "	FROM [dbo].[PurchaseOrderDetails] x ";
            sql += "	INNER JOIN [dbo].[PurchaseOrderHeaders] y ON y.poId = x.poId) d ON d.prId = b.prId AND d.productID = b.productID ";
            sql += "WHERE d.poId = '" + poId + "'";
            DataTable dtListAuthorize = con.executeReader(sql);
            foreach (DataRow dr in dtListAuthorize.Rows)
            {
                foreach (var b in db.transVendorProducts.Where(x => x.id_vendor == id_vendor).ToList() as IEnumerable<transVendorProduct>)
                {
                    if (b.productID == int.Parse(dr["productID"].ToString()))
                    {
                        var editor = new purchaseRequestHeader.purchaseRequestDetail()
                        {
                            productId = int.Parse(dr["productID"].ToString()),
                            description = dr["description"].ToString(),
                            qty = int.Parse(dr["qty"].ToString()),
                            unit = dr["unit"].ToString()
                        };
                        model.detailPurchaseRequest.Add(editor);
                        break;
                    }
                }
            }
            ViewData["dataDetail"] = model.detailPurchaseRequest.ToList();
            #endregion
            ViewData["dataDetailSaved"] = db.purchaseOrderDetails.Where(x => x.poId == poId).ToList();
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
            purchaseOrderHeader purchaseorderheader = db.purchaseOrderHeaders.Find(id);
            if (purchaseorderheader == null)
            {
                return HttpNotFound();
            }

            loadPO(purchaseorderheader.poId);
            return View(purchaseorderheader);
        }

        public void getListPR(string keywordId, string keywordSearch, int? maxRows, int? pageStart, bool isResultAll)
        {
            DataTable dtPRforPO = new DataTable();
            DataTable dtPRforPOAll = new DataTable();
            using (SqlConnection conn = new SqlConnection(con.conString))
            {
                SqlCommand cmd = new SqlCommand();
                SqlDataAdapter da = new SqlDataAdapter();


                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[sp_pr_for_po]";
                cmd.Parameters.AddWithValue("@keywordId",string.IsNullOrEmpty(keywordId) ? (object)DBNull.Value : keywordId);
                cmd.Parameters.AddWithValue("@keywordSearch",string.IsNullOrEmpty(keywordSearch) ? (object)DBNull.Value : keywordSearch);
                cmd.Parameters.AddWithValue("@maxRows", maxRows == null ? (object)DBNull.Value : maxRows);
                cmd.Parameters.AddWithValue("@pageStart", pageStart == null ? (object)DBNull.Value : pageStart);
                cmd.Parameters.AddWithValue("@isResultAll", false);

                conn.Open();
                da.SelectCommand = cmd;
                da.Fill(dtPRforPO);
            }

            using (SqlConnection conn = new SqlConnection(con.conString))
            {
                SqlCommand cmd = new SqlCommand();
                SqlDataAdapter da = new SqlDataAdapter();


                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[sp_pr_for_po]";
                cmd.Parameters.AddWithValue("@keywordId", string.IsNullOrEmpty(keywordId) ? (object)DBNull.Value : keywordId);
                cmd.Parameters.AddWithValue("@keywordSearch", string.IsNullOrEmpty(keywordSearch) ? (object)DBNull.Value : keywordSearch);
                cmd.Parameters.AddWithValue("@maxRows", maxRows == null ? (object)DBNull.Value : maxRows);
                cmd.Parameters.AddWithValue("@pageStart", pageStart == null ? (object)DBNull.Value : pageStart);
                cmd.Parameters.AddWithValue("@isResultAll", true);

                conn.Open();
                da.SelectCommand = cmd;
                da.Fill(dtPRforPOAll);
            }

            var model = new purchaseOrderHeader();
            foreach(DataRow dr in dtPRforPO.Rows)
            {
                var editor = new purchaseOrderHeader.prPopUp()
                {
                    prId = dr["prId"].ToString(),
                    requestDeptId = int.Parse(dr["requestDeptId"].ToString()),
                    deptName = dr["deptName"].ToString(),
                    typeOrder = dr["typeOrder"].ToString(),
                    proposalInclude = bool.Parse(dr["proposalInclude"].ToString()),
                    specialInstruction = dr["specialInstruction"].ToString(),
                    projectTimeDelivery = dr["projectTimeDelivery"].ToString()
                };
                model.PopUpPR.Add(editor);
            }
            ViewBag.PRPopUp = model.PopUpPR.ToList();
            ViewBag.totalRows = dtPRforPOAll.Rows.Count; 
            ViewBag.currentPage = pageStart;
        }
    }
}
