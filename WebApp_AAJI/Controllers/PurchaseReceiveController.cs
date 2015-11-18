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
    public class PurchaseReceiveController : Controller
    {
        private MyDataAccess db = new MyDataAccess();
        LoginViewModel.userLogin lvm = new LoginViewModel.userLogin();
        private AccountController acm = new AccountController();
        private CommonController ccm = new CommonController();
        private ConnectionController con = new ConnectionController();
        string prefix = "RCV";

        [HttpGet]
        public async Task<ActionResult> ApprovalProcess(string act, string id)
        {
            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            purchaseReceive model = db.purchaseReceives.Find(id);
            model.receivedStatus = act == "approved" ? true : false;
            model.receivedDate = DateTime.Now;
            model.receivedBy = lvm.userID;
            db.Entry(model).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> popUpSearch(string keywordId, string keywordSearch, int maxRows, int pageStart)
        {
            string sql = "WITH cte AS ( ";
            sql += "SELECT ROW_NUMBER() OVER(ORDER BY poId) AS rn, ";
            sql += "a.poId, a.poDate, b.vendorID, b.vendorName, a.poUrgent ";
            sql += "FROM [dbo].[PurchaseOrderHeaders] a WITH(NOLOCK) ";
            sql += "INNER JOIN [dbo].[Vendors] b ON b.vendorID = a.vendorId ";
            sql += "WHERE a.poId NOT IN (SELECT poId FROM [dbo].[PurchaseReceives])	AND a.proposedStatus = 1";
            if (keywordId != string.Empty && keywordSearch != string.Empty && !keywordId.Contains("a.poDate"))
            {
                sql += "AND " + keywordId + " LIKE  '%" + keywordSearch + "%' ";
            }
            else
            {
                sql += "AND " + keywordId + keywordSearch;
            }
            sql += ") ";
            sql += "SELECT * FROM cte ";
            string sqlAll = sql;
            sql += "WHERE rn BETWEEN " + ((pageStart * maxRows) - (maxRows - 1)) + " AND " + (pageStart * maxRows);

            DataTable dataAll = con.executeReader(sqlAll);
            ViewBag.totalRows = dataAll.Rows.Count;
            ViewBag.currentPage = pageStart;

            DataTable popUp = con.executeReader(sql);
            var model = new purchaseOrderHeader();
            if (popUp != null)
            {
                foreach (DataRow dr in popUp.Rows)
                {
                    var editor = new purchaseOrderHeader.POpopUp()
                    {
                        poId = dr["poId"].ToString(),
                        poDate = Convert.ToDateTime(dr["poDate"].ToString()),
                        vendorId = dr["vendorId"].ToString(),
                        vendorName = dr["vendorName"].ToString(),
                        poUrgent = dr["poUrgent"].ToString(),
                    };
                    model.PopUpPO.Add(editor);
                }
                ViewBag.POPopUp = model.PopUpPO.ToList();
            }

            return PartialView("_PartialPagePopUpPOSearchSub");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDetailPurchaseReceive(string dataDetail, string act)
        {
            string[] data = dataDetail.Split('|');

            var model = new purchaseReceive();
            for (int i = 0; i < data.Count(); i++)
            {
                string[] value = data[i].Split(';');

                if (value[1].ToString() == "")
                    break;

                int t_productID = int.Parse(value[1].ToString());
                int t_qty = value[2].ToString() != "" ? Convert.ToInt32(value[2].ToString()) : 0;
                string t_remarks = value[3].ToString();

                var editor = new purchaseReceive.purchaseReceiveDetail()
                {
                    receiveNo = "",
                    productId = t_productID,
                    qty = t_qty,
                    remarks= t_remarks
                };
                model.detailPurchaseReceive.Add(editor);
            }
            ViewData["dataDetail"] = model.detailPurchaseReceive.ToList();

            return PartialView("_PartialPageRCV1");
        }

        [HttpGet]
        public async Task<ActionResult> LoadDetailPO(string poId)
        {
            var z = db.purchaseOrderDetails.Where(x => x.poId == poId)
                    .Join(db.purchaseOrderHeaders, a => a.poId, b => b.poId, (a, b) => new { a, b })
                    .Join(db.purchaseRequestDetails, c => c.b.prId, d => d.prId, (c, d) => new { c, d })
                    .Where(e=>e.c.a.productID == e.d.productId)
                    .Select(y => new { y.c.a.poId, y.c.a.productID, y.d.description, y.c.a.qty, y.d.unit }).ToList();
            
            var model = new purchaseReceive();
            for (int i = 0; i < z.Count; i++ )
            {
                var editor = new purchaseReceive.poReceive()
                {
                    productId = z[i].productID,
                    productDesc = z[i].description,
                    qtyOrder = z[i].qty,
                    unit = z[i].unit
                };
                model.detailPoReceive.Add(editor);
            }
            ViewData["dataDetail"] = model.detailPoReceive.ToList();

            return PartialView("_PartialPageRCV-PO1");
        }

        // GET: /PurchaseReceive/
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

            invoiceWasCreated();
            
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

            var data = from s in db.purchaseReceives select s;
            if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate) && !String.IsNullOrEmpty(searchString))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.receiveDate >= sDate && s.receiveDate <= eDate && s.receiveNo.Contains(searchString));
            }
            else if (!String.IsNullOrEmpty(startDate) && !String.IsNullOrEmpty(endDate))
            {
                DateTime sDate = DateTime.Parse(startDate);
                DateTime eDate = DateTime.Parse(endDate);
                data = data.Where(s => s.receiveDate >= sDate && s.receiveDate <= eDate);
            }
            else if (!String.IsNullOrEmpty(searchString))
            {
                data = data.Where(s => s.receiveNo.Contains(searchString));
            }


            if (lvm.isAdmin == false && ViewBag.ApprovalAuth == false)
                data = data.Where(x => x.createdUser == lvm.userID);

            switch (sortOrder)
            {
                case "date_":
                    data = data.OrderByDescending(s => s.receiveDate);
                    break;
                case "voucherNo":
                    data = data.OrderBy(s => new { s.receiveNo });
                    break;
                case "voucherNo_":
                    data = data.OrderByDescending(s => s.receiveNo);
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
            //    return View(db.purchaseReceives.Where(x => x.createdUser == lvm.userID).ToList());
            //else if (lvm.isAdmin == false && ViewBag.ApprovalAuth == true)
            //    return View(db.purchaseReceives.ToList());
            //else
            //    return View(db.purchaseReceives.ToList());
            #endregion
        }

        // GET: /PurchaseReceive/Details/5
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
            purchaseReceive purchasereceive = db.purchaseReceives.Find(id);
            if (purchasereceive == null)
            {
                return HttpNotFound();
            }

            var popUp = db.purchaseOrderHeaders.AsNoTracking().Where(x => !db.purchaseReceives.Any(y => y.poId == x.poId))
                                .Join(db.vendors, a => a.vendorId, b => b.vendorID, (a, b) => new { a, b })
                                .Select(c => new { c.a.poId, c.a.poDate, c.b.vendorID, c.b.vendorName, c.a.poUrgent }).ToList();
            var aa = db.purchaseReceives.Where(x => x.receiveNo == "RCV").ToList();
            var model = new purchaseOrderHeader();
            for (int i = 0; i < popUp.Count; i++)
            {
                var editor = new purchaseOrderHeader.POpopUp()
                {
                    poId = popUp[i].poId,
                    poDate = popUp[i].poDate,
                    vendorId = popUp[i].vendorID,
                    vendorName = popUp[i].vendorName,
                    poUrgent = popUp[i].poUrgent
                };
                model.PopUpPO.Add(editor);
            }
            ViewBag.POPopUp = model.PopUpPO.ToList();

            loadRCV(purchasereceive.receiveNo);
            invoiceWasCreated();
            return View(purchasereceive);
        }

        // GET: /PurchaseReceive/Create
        public ActionResult Create()
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            var popUp = db.purchaseOrderHeaders.AsNoTracking().Where(x => !db.purchaseReceives.Any(y => y.poId == x.poId) && x.proposedStatus == true && x.approvedStatus == true)
                                .Join(db.vendors, a => a.vendorId, b => b.vendorID, (a, b) => new { a, b })
                                .Select(c => new { c.a.poId, c.a.poDate, c.b.vendorID, c.b.vendorName, c.a.poUrgent}).ToList();

            var aa = db.purchaseReceives.Where(x => x.receiveNo == "RCV").ToList();

            var model = new purchaseOrderHeader();
            for (int i = 0; i < popUp.Count; i++)
            {
                var editor = new purchaseOrderHeader.POpopUp()
                {
                    poId = popUp[i].poId,
                    poDate = popUp[i].poDate,
                    vendorId = popUp[i].vendorID,
                    vendorName = popUp[i].vendorName,
                    poUrgent = popUp[i].poUrgent
                };
                model.PopUpPO.Add(editor);
            }
            ViewBag.POPopUp = model.PopUpPO.ToList();
            ViewBag.totalRows = model.PopUpPO.Count;

            return View();
        }

        // POST: /PurchaseReceive/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(purchaseReceive purchasereceive)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            var z = db.purchaseOrderDetails.Where(x => x.poId == purchasereceive.poId)
                    .Join(db.purchaseOrderHeaders, a => a.poId, b => b.poId, (a, b) => new { a, b })
                    .Join(db.purchaseRequestDetails, c => c.b.prId, d => d.prId, (c, d) => new { c, d })
                    .Where(e => e.c.a.productID == e.d.productId)
                    .Select(y => new { y.c.a.poId, y.c.a.productID, y.d.description, y.c.a.qty, y.d.unit }).ToList();

            var model = new purchaseReceive();
            for (int i = 0; i < z.Count; i++)
            {
                var editor = new purchaseReceive.poReceive()
                {
                    productId = z[i].productID,
                    productDesc = z[i].description,
                    qtyOrder = z[i].qty,
                    unit = z[i].unit
                };
                model.detailPoReceive.Add(editor);
            }
            ViewData["dataDetail"] = model.detailPoReceive.ToList();

            if (ModelState.IsValid)
            {
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

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        string generateID = prefix + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("d2");
                        var prIDCreated = db.purchaseReceives.Where(x => x.receiveNo.Contains(generateID)).OrderByDescending(x => x.createdDate).Select(x => x.poId).ToList();
                        if (prIDCreated.Count == 0)
                        {
                            generateID = generateID + "0001";
                        }
                        else
                        {
                            generateID = generateID + (Convert.ToInt32(prIDCreated[0].Substring((prIDCreated[0].Length - 4))) + 1).ToString().PadLeft(4, '0');
                        }

                        purchasereceive.receiveNo = generateID;
                        lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                        purchasereceive.createdUser = lvm.userID;
                        purchasereceive.createdDate = DateTime.Now;
                        purchasereceive.proposedBy = lvm.userID;
                        purchasereceive.proposedDate = DateTime.Now;
                        purchasereceive.proposedStatus = isProposed;
                        db.purchaseReceives.Add(purchasereceive);

                        List<string> listProductId = new List<string>();
                        List<int> listReceive = new List<int>();
                        #region insertDetail
                        for (int i = 1; i <= countChk; i++)
                        {
                            var colVal = Request.Form["txtProductID_" + i];
                            if (colVal == "")
                                break;

                            int t_productID = int.Parse(Request.Form["txtProductID_" + i].ToString());
                            int t_qtyReceive = Request.Form["txtQtyReceive_" + i].ToString() != "" ? Convert.ToInt32(Request.Form["txtQtyReceive_" + i].ToString().Replace('.', ',')) : 0;
                            int t_qtyReturn = Request.Form["txtQtyReturn_" + i].ToString() != "" ? Convert.ToInt32(Request.Form["txtQtyReturn_" + i].ToString().Replace('.', ',')) : 0;
                            listProductId.Add(t_productID.ToString());
                            listReceive.Add(t_qtyReceive);

                            if (t_qtyReceive > 0)
                            {
                                var editorx = new purchaseReceive.purchaseReceiveDetail()
                                {
                                    receiveNo = purchasereceive.receiveNo,
                                    productId = t_productID,
                                    qty = t_qtyReceive
                                };
                                db.purchaseReceiveDetails.Add(editorx);
                            }

                            if (t_qtyReturn > 0)
                            {
                                var editory = new purchaseReceive.purchaseReturnDetail()
                                {
                                    receiveNo = purchasereceive.receiveNo,
                                    productId = t_productID,
                                    qty = t_qtyReturn
                                };
                                db.purchaseReturnDetails.Add(editory);
                            }
                        }
                        #endregion

                        if(isProposed == true)
                        { 
                            #region createInvoice
                            PurchaseInvoiceController pIv = new PurchaseInvoiceController();
                            string prefixInvoice = pIv.prefix;
                            string generateIDInvoice = prefixInvoice + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("d2");
                            string generateIDInvoiceSequence = "";
                            var invIDCreated = db.purchaseInvoices.Where(x => x.purchaseInvoiceNo.Contains(generateIDInvoice)).OrderByDescending(x => x.createdDate).Select(x => x.purchaseInvoiceNo).ToList();
                            if (invIDCreated.Count == 0)
                            {
                                generateIDInvoiceSequence = generateIDInvoice + "0001";
                            }
                            else
                            {
                                generateIDInvoiceSequence = generateIDInvoice + (Convert.ToInt32(invIDCreated[0].Substring((invIDCreated[0].Length - 4))) + 1).ToString().PadLeft(4, '0');
                            }

                            var headerPO = db.purchaseOrderHeaders.Where(x => x.poId == purchasereceive.poId).ToList();
                            var topPOMultiple = db.purchaseOrderTOPs.Where(x => x.poId == purchasereceive.poId).ToList();

                            purchaseInvoice purchaseinvoice = new purchaseInvoice();
                            purchaseinvoice.invoiceDate = purchasereceive.proposedDate;
                            purchaseinvoice.receiveNo = purchasereceive.receiveNo;
                            purchaseinvoice.createdUser = purchasereceive.proposedBy;
                            purchaseinvoice.createdDate = purchasereceive.proposedDate;
                            purchaseinvoice.remarks = purchasereceive.remarks;

                            if (headerPO[0].topType == "Single")
                            {
                                purchaseinvoice.purchaseInvoiceNo = generateIDInvoiceSequence;
                                purchaseinvoice.dueDate = purchasereceive.proposedDate.AddDays(headerPO[0].topAmount);

                                decimal total = 0;
                                for (int i = 1; i <= countChk; i++)
                                {
                                    var colVal = Request.Form["txtProductID_" + i];
                                    if (colVal == "")
                                        break;

                                    int t_productID = int.Parse(Request.Form["txtProductID_" + i].ToString());
                                    int t_qtyReceive = Request.Form["txtQtyReceive_" + i].ToString() != "" ? Convert.ToInt32(Request.Form["txtQtyReceive_" + i].ToString().Replace('.', ',')) : 0;
                                    int t_qtyReturn = Request.Form["txtQtyReturn_" + i].ToString() != "" ? Convert.ToInt32(Request.Form["txtQtyReturn_" + i].ToString().Replace('.', ',')) : 0;
                            
                                    var poInfo = db.purchaseOrderDetails.Where(x => x.poId == purchasereceive.poId && x.productID == t_productID).ToList();
                                    foreach (var ax in poInfo as IEnumerable<purchaseOrderHeader.purchaseOrderDetail>)
                                    {
                                        total += ((t_qtyReceive * ax.price) - (((t_qtyReceive * ax.price) * ax.disc) / 100));
                                    }
                                }

                                var poInfoH = db.purchaseOrderHeaders.Where(x => x.poId == purchasereceive.poId).ToList();
                                foreach (var axH in poInfoH as IEnumerable<purchaseOrderHeader>)
                                {
                                    total = (total - ((total * axH.disc) / 100));
                                    total = (total + ((total * axH.ppn) / 100));
                                }
                                purchaseinvoice.total = total;

                                db.purchaseInvoices.Add(purchaseinvoice);
                            }
                            else if (headerPO[0].topType == "Multiple")
                            {
                                for (int a = 0; a < topPOMultiple.Count; a++)
                                {
                                    purchaseinvoice.purchaseInvoiceNo = generateIDInvoiceSequence;
                                    purchaseinvoice.dueDate = topPOMultiple[a].buyDate;
                                    int prodId = int.Parse(listProductId[0]);
                                    var poInfo = db.purchaseOrderDetails.Where(x => x.poId == purchasereceive.poId && x.productID == prodId).ToList();
                                    decimal total = ((poInfo[0].qty * poInfo[0].price) - ((poInfo[0].qty * poInfo[0].price) / 100));
                                    purchaseinvoice.total = total;
                                    db.purchaseInvoices.Add(purchaseinvoice);

                                    generateIDInvoiceSequence = generateIDInvoice + (Convert.ToInt32(generateIDInvoiceSequence.Substring((generateIDInvoiceSequence.Length - 4))) + 1).ToString().PadLeft(4, '0');
                                }
                                db.purchaseInvoices.Add(purchaseinvoice);
                            }
                            #endregion
                        }

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

            return View(purchasereceive);
        }

        // GET: /PurchaseReceive/Edit/5
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
            purchaseReceive purchasereceive = db.purchaseReceives.Find(id);
            if (purchasereceive == null)
            {
                return HttpNotFound();
            }

            var popUp = db.purchaseOrderHeaders.AsNoTracking().Where(x => !db.purchaseReceives.Any(y => y.poId == x.poId) && x.proposedStatus == true && x.approvedStatus == true)
                                .Join(db.vendors, a => a.vendorId, b => b.vendorID, (a, b) => new { a, b })
                                .Select(c => new { c.a.poId, c.a.poDate, c.b.vendorID, c.b.vendorName, c.a.poUrgent }).ToList();
            var aa = db.purchaseReceives.Where(x => x.receiveNo == "RCV").ToList();
            var model = new purchaseOrderHeader();
            for (int i = 0; i < popUp.Count; i++)
            {
                var editor = new purchaseOrderHeader.POpopUp()
                {
                    poId = popUp[i].poId,
                    poDate = popUp[i].poDate,
                    vendorId = popUp[i].vendorID,
                    vendorName = popUp[i].vendorName,
                    poUrgent = popUp[i].poUrgent
                };
                model.PopUpPO.Add(editor);
            }
            ViewBag.POPopUp = model.PopUpPO.ToList();

            loadRCV(purchasereceive.receiveNo);

            return View(purchasereceive);
        }

        // POST: /PurchaseReceive/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(purchaseReceive purchasereceive)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            if (ModelState.IsValid)
            {
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

                try
                {
                    using (TransactionScope ts = new TransactionScope())
                    {
                        lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
                        purchasereceive.modifiedUser = lvm.userID;
                        purchasereceive.modifiedDate = DateTime.Now;
                        purchasereceive.proposedStatus = isProposed;
                        db.Entry(purchasereceive).State = EntityState.Modified;

                        List<string> listProductId = new List<string>();
                        #region insertDetail
                        bool runFirst = true;
                        for (int i = 1; i <= countChk; i++)
                        {
                            if (runFirst == true)
                            {
                                db.purchaseReceiveDetails.RemoveRange(db.purchaseReceiveDetails.Where(x => x.receiveNo == purchasereceive.receiveNo));
                                db.purchaseReturnDetails.RemoveRange(db.purchaseReturnDetails.Where(x => x.receiveNo == purchasereceive.receiveNo));
                                runFirst = false;
                            }

                            var colVal = Request.Form["txtProductID_" + i];
                            if (colVal == "")
                                break;

                            int t_productID = int.Parse(Request.Form["txtProductID_" + i].ToString());
                            int t_qtyReceive = Request.Form["txtQtyReceive_" + i].ToString() != "" ? Convert.ToInt32(Request.Form["txtQtyReceive_" + i].ToString().Replace('.', ',')) : 0;
                            int t_qtyReturn = Request.Form["txtQtyReturn_" + i].ToString() != "" ? Convert.ToInt32(Request.Form["txtQtyReturn_" + i].ToString().Replace('.', ',')) : 0; 
                            listProductId.Add(t_productID.ToString());

                            if (t_qtyReceive > 0)
                            {
                                var editorx = new purchaseReceive.purchaseReceiveDetail()
                                {
                                    receiveNo = purchasereceive.receiveNo,
                                    productId = t_productID,
                                    qty = t_qtyReceive
                                };
                                db.purchaseReceiveDetails.Add(editorx);
                            }

                            if (t_qtyReturn > 0)
                            {
                                var editory = new purchaseReceive.purchaseReturnDetail()
                                {
                                    receiveNo = purchasereceive.receiveNo,
                                    productId = t_productID,
                                    qty = t_qtyReturn
                                };
                                db.purchaseReturnDetails.Add(editory);
                            }
                        }
                        #endregion

                        if (isProposed == true)
                        {
                            #region createInvoice
                            PurchaseInvoiceController pIv = new PurchaseInvoiceController();
                            string prefixInvoice = pIv.prefix;
                            string generateIDInvoice = prefixInvoice + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("d2");
                            string generateIDInvoiceSequence = "";
                            var invIDCreated = db.purchaseInvoices.Where(x => x.purchaseInvoiceNo.Contains(generateIDInvoice)).OrderByDescending(x => x.createdDate).Select(x => x.purchaseInvoiceNo).ToList();
                            if (invIDCreated.Count == 0)
                            {
                                generateIDInvoiceSequence = generateIDInvoice + "0001";
                            }
                            else
                            {
                                generateIDInvoiceSequence = generateIDInvoice + (Convert.ToInt32(invIDCreated[0].Substring((invIDCreated[0].Length - 4))) + 1).ToString().PadLeft(4, '0');
                            }

                            var headerPO = db.purchaseOrderHeaders.Where(x => x.poId == purchasereceive.poId).ToList();
                            var topPOMultiple = db.purchaseOrderTOPs.Where(x => x.poId == purchasereceive.poId).ToList();

                            purchaseInvoice purchaseinvoice = new purchaseInvoice();
                            purchaseinvoice.invoiceDate = purchasereceive.proposedDate;
                            purchaseinvoice.receiveNo = purchasereceive.receiveNo;
                            purchaseinvoice.createdUser = purchasereceive.proposedBy;
                            purchaseinvoice.createdDate = purchasereceive.proposedDate;

                            if (headerPO[0].topType == "Single")
                            {
                                purchaseinvoice.purchaseInvoiceNo = generateIDInvoiceSequence;
                                purchaseinvoice.dueDate = purchasereceive.proposedDate.AddDays(headerPO[0].topAmount);
                                db.purchaseInvoices.Add(purchaseinvoice);
                            }
                            else if (headerPO[0].topType == "Multiple")
                            {
                                for (int a = 0; a < topPOMultiple.Count; a++)
                                {
                                    purchaseinvoice.purchaseInvoiceNo = generateIDInvoiceSequence;
                                    purchaseinvoice.dueDate = topPOMultiple[a].buyDate;
                                    int prodId = int.Parse(listProductId[0]);
                                    var poInfo = db.purchaseOrderDetails.Where(x => x.poId == purchasereceive.poId && x.productID == prodId).ToList();
                                    decimal total = ((poInfo[0].qty * poInfo[0].price) - ((poInfo[0].qty * poInfo[0].price) / 100));
                                    purchaseinvoice.total = total;
                                    db.purchaseInvoices.Add(purchaseinvoice);

                                    generateIDInvoiceSequence = generateIDInvoice + (Convert.ToInt32(generateIDInvoiceSequence.Substring((generateIDInvoiceSequence.Length - 4))) + 1).ToString().PadLeft(4, '0');
                                }
                                db.purchaseInvoices.Add(purchaseinvoice);
                            }
                            #endregion
                        }

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
            return View(purchasereceive);
        }

        // GET: /PurchaseReceive/Delete/5
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
            purchaseReceive purchasereceive = db.purchaseReceives.Find(id);
            if (purchasereceive == null)
            {
                return HttpNotFound();
            }

            var popUp = db.purchaseOrderHeaders.AsNoTracking().Where(x => !db.purchaseReceives.Any(y => y.poId == x.poId))
                                .Join(db.vendors, a => a.vendorId, b => b.vendorID, (a, b) => new { a, b })
                                .Select(c => new { c.a.poId, c.a.poDate, c.b.vendorID, c.b.vendorName, c.a.poUrgent }).ToList();
            var aa = db.purchaseReceives.Where(x => x.receiveNo == "RCV").ToList();
            var model = new purchaseOrderHeader();
            for (int i = 0; i < popUp.Count; i++)
            {
                var editor = new purchaseOrderHeader.POpopUp()
                {
                    poId = popUp[i].poId,
                    poDate = popUp[i].poDate,
                    vendorId = popUp[i].vendorID,
                    vendorName = popUp[i].vendorName,
                    poUrgent = popUp[i].poUrgent
                };
                model.PopUpPO.Add(editor);
            }
            ViewBag.POPopUp = model.PopUpPO.ToList();

            loadRCV(purchasereceive.receiveNo);
            return View(purchasereceive);
        }

        // POST: /PurchaseReceive/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            if (acm.cekSession() == false)
                return RedirectToAction("Logout", "Account");

            lvm = Session["sessionUserLogin"] as LoginViewModel.userLogin;
            if (acm.cekValidation(Url.Action().ToString()) == false && lvm.isAdmin == false)
                return RedirectToAction("NotAuthorized", "Account", new { menu = Url.Action().ToString() });

            purchaseReceive purchasereceive = db.purchaseReceives.Find(id);
            db.purchaseReceives.Remove(purchasereceive);
            db.purchaseReceiveDetails.RemoveRange(db.purchaseReceiveDetails.Where(x => x.receiveNo == id));
            db.purchaseReturnDetails.RemoveRange(db.purchaseReturnDetails.Where(x => x.receiveNo == id));
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

        public void loadRCV(string receiveNo)
        {
            var detailRCV = db.purchaseOrderHeaders.AsNoTracking()
                                .Join(db.vendors, a => a.vendorId, b => b.vendorID, (a, b) => new { a, b })
                                .Join(db.purchaseReceives.Where(x=>x.receiveNo == receiveNo), c => c.a.poId, d => d.poId, (c, d) => new { c, d})
                                .Select(e => new { e.c.a.poId, e.c.a.poDate, e.c.b.vendorID, e.c.b.vendorName, e.c.a.poUrgent, e.d.proposedBy }).ToList();
            var userId = "";
            var poId = "";
            for (int i = 0; i < detailRCV.Count; i++)
            {
                ViewBag.vendorId = detailRCV[i].vendorID;
                ViewBag.vendorName = detailRCV[i].vendorName;
                userId = detailRCV[i].proposedBy;
                poId = detailRCV[i].poId;
            }


            var aa = db.purchaseReceives.Where(x => x.receiveNo == "RCV").ToList();
            ViewBag.dataDetailSaved = db.purchaseReceiveDetails.Where(x => x.receiveNo == receiveNo).ToList();
            ViewBag.dataDetailReturnSaved = db.purchaseReturnDetails.Where(x => x.receiveNo == receiveNo).ToList();
            ViewBag.userName = ccm.getUserName(userId);

            var z = db.purchaseOrderDetails.Where(x => x.poId == poId)
                    .Join(db.purchaseOrderHeaders, a => a.poId, b => b.poId, (a, b) => new { a, b })
                    .Join(db.purchaseRequestDetails, c => c.b.prId, d => d.prId, (c, d) => new { c, d })
                    .Where(e => e.c.a.productID == e.d.productId)
                    .Select(y => new { y.c.a.poId, y.c.a.productID, y.d.description, y.c.a.qty, y.d.unit }).ToList();

            var model = new purchaseReceive();
            for (int i = 0; i < z.Count; i++)
            {
                var editor = new purchaseReceive.poReceive()
                {
                    productId = z[i].productID,
                    productDesc = z[i].description,
                    qtyOrder = z[i].qty,
                    unit = z[i].unit
                };
                model.detailPoReceive.Add(editor);
            }
            ViewData["dataDetail"] = model.detailPoReceive.ToList();
        }

        public void invoiceWasCreated()
        {
            var invoiceWasCreated = db.purchaseInvoices
                                    .Join(db.purchaseReceives, a => a.receiveNo, b => b.receiveNo, (a, b) => new { a,b }).ToList();
            List<string> listPR = new List<string>();
            for (int i = 0; i < invoiceWasCreated.Count; i++)
            {
                listPR.Add(invoiceWasCreated[i].a.receiveNo);
            }
            ViewBag.invoiceWasCreated = listPR.ToList();
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
            purchaseReceive purchasereceive = db.purchaseReceives.Find(id);
            if (purchasereceive == null)
            {
                return HttpNotFound();
            }

            var popUp = db.purchaseOrderHeaders.AsNoTracking().Where(x => !db.purchaseReceives.Any(y => y.poId == x.poId))
                                .Join(db.vendors, a => a.vendorId, b => b.vendorID, (a, b) => new { a, b })
                                .Select(c => new { c.a.poId, c.a.poDate, c.b.vendorID, c.b.vendorName, c.a.poUrgent }).ToList();
            var aa = db.purchaseReceives.Where(x => x.receiveNo == "RCV").ToList();
            var model = new purchaseOrderHeader();
            for (int i = 0; i < popUp.Count; i++)
            {
                var editor = new purchaseOrderHeader.POpopUp()
                {
                    poId = popUp[i].poId,
                    poDate = popUp[i].poDate,
                    vendorId = popUp[i].vendorID,
                    vendorName = popUp[i].vendorName,
                    poUrgent = popUp[i].poUrgent
                };
                model.PopUpPO.Add(editor);
            }
            ViewBag.POPopUp = model.PopUpPO.ToList();

            loadRCV(purchasereceive.receiveNo);
            return View(purchasereceive);
        }
    }
}
