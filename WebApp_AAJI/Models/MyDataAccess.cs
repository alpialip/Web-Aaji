using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class MyDataAccess : DbContext
    {
        public DbSet<WebApp_AAJI.Models.user> Users { get; set; }
        public DbSet<WebApp_AAJI.Models.menu> Menus { get; set; }
        public DbSet<WebApp_AAJI.Models.userGroupH> UserGroupHs { get; set; }
        public DbSet<WebApp_AAJI.Models.userGroupH.userGroupD> UserGroupDs { get; set; }
        public DbSet<WebApp_AAJI.Models.menuValidation> MenuValidations { get; set; }
        public DbSet<WebApp_AAJI.Models.menuValidation.menuValidationDetail> MenuValidationDetails { get; set; }
        public DbSet<WebApp_AAJI.Models.chartOfAccount> chartOfAccounts { get; set; }
        public DbSet<WebApp_AAJI.Models.typeFinanceTransaction> typeFinanceTransactions { get; set; }
        public DbSet<WebApp_AAJI.Models.setupEmail> setupEmails { get; set; }
        public DbSet<WebApp_AAJI.Models.email> emails { get; set; }
        public DbSet<WebApp_AAJI.Models.groupEmail> groupEmails { get; set; }
        public DbSet<WebApp_AAJI.Models.journalHeader> journalHeaders { get; set; }
        public DbSet<WebApp_AAJI.Models.journalHeader.journalDetail> journalDetails { get; set; }
        public DbSet<WebApp_AAJI.Models.financeTransactionHeader> financeTransactionHeaders { get; set; }
        public DbSet<WebApp_AAJI.Models.financeTransactionHeader.financeTransactionDetail> financeTransactionDetails { get; set; }
        public DbSet<WebApp_AAJI.Models.purchaseRequestHeader> purchaseRequestHeaders { get; set; }
        public DbSet<WebApp_AAJI.Models.purchaseRequestHeader.purchaseRequestDetail> purchaseRequestDetails { get; set; }
        public DbSet<WebApp_AAJI.Models.purchaseOrderHeader> purchaseOrderHeaders { get; set; }
        public DbSet<WebApp_AAJI.Models.purchaseOrderHeader.purchaseOrderDetail> purchaseOrderDetails { get; set; }
        public DbSet<WebApp_AAJI.Models.purchaseOrderHeader.purchaseOrderTop> purchaseOrderTOPs { get; set; }
        public DbSet<WebApp_AAJI.Models.bank> banks { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.vendor> vendors { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.penghasilan> penghasilans { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.pengurangPenghasilan> pengurangPenghasilans { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.potongan> potongans { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.department> departments { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.divisi> divisis { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.typeAbsensi> typeAbsensis { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.product> products { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.fixedAssetType> fixedAssetTypes { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.level> levels { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.purchaseInvoice> purchaseInvoices { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.transReminder> transReminders { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.advancePaymentVoucher> advancePaymentVouchers { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.advancePaymentVoucher.advancePaymentVoucherDetail> advancePaymentVoucherDetails { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.bankCashPaymentVoucher> bankCashPaymentVouchers { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.bankCashPaymentVoucher.bankCashPaymentVoucherDetail> bankCashPaymentVoucherDetails { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.employee> employees { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.employee.employeePosition> employeePositions { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.employee.employeeResign> employeeResigns { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.employee.employeeMate> employeeMates { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.employee.employeeChild> employeeChilds { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.employee.employeeCV> employeeCVs { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.employee.employeeEducation> employeeEducations { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.employee.employeeOccupation> employeeOccupations { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.absensiRekap> absensiRekaps { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.absensiEmployee> absensiEmployees { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.employeeLoan> employeeLoans { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.employeeClaimMedical> employeeClaimMedicals { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.employeeClaimMedical.employeeClaimMedicalDetail> employeeClaimMedicalDetails { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.sisaSaldoCuti> sisaSaldoCutis { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.fixedAsset> fixedAssets { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.fixedAsset.fixedAssetPerson> fixedAssetPersons { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.fixedAsset.fixedAssetMaintenance> fixedAssetMaintenances { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.purchaseReceive> purchaseReceives { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.purchaseReceive.purchaseReceiveDetail> purchaseReceiveDetails { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.purchaseReceive.purchaseReturnDetail> purchaseReturnDetails { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.cashOnHandAndAdvanceRequest> cashOnHandAndAdvanceRequests { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.cashOnHandAndAdvanceRequest.cashOnHandAndAdvanceRequestDetail> cashOnHandAndAdvanceRequestDetails { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.accountPayableHeader> accountPayableHeaders { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.accountPayableHeader.accountPayableDetail> accountPayableDetails { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.transPenghasilan> transPenghasilans { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.transPengurangPenghasilan> transPengurangPenghasilans { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.transMasterPenghasilan> transMasterPenghasilans { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.transMasterPengurangPenghasilan> transMasterPengurangPenghasilans { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.transVendorProduct> transVendorProducts { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.transLeaveRequest> transLeaveRequests { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.transReportPayment> transReportPayments { get; set; }
        public System.Data.Entity.DbSet<WebApp_AAJI.Models.holiday> holidays { get; set; }
    }
}