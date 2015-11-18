using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class financeTransactionHeader
    {
        public financeTransactionHeader()
        {
            this.detailFinanceTransaction = new List<collectTransactionDetail>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(13)]
        [DisplayName("Transaction No.")]
        public string voucherNo { get; set; }

        [DisplayName("Transaction Date")]
        public DateTime voucherDate { get; set; }

        [StringLength(10)]
        [DisplayName("Transaction Type")]
        public string transactionType { get; set; }
        public int tftID { get; set; }

        [StringLength(10)]
        [DisplayName("Billing Type")]
        public string billingType { get; set; }

        [DisplayName("Billing No.")]
        public int billingNo { get; set; }

        [DisplayName("Clearing Date")]
        public DateTime? clearingDate { get; set; }

        [DisplayName("Amount")]
        public decimal amount { get; set; }

        [DisplayName("Remark")]
        [StringLength(250)]
        public string remarks { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }

        public List<collectTransactionDetail> detailFinanceTransaction { get; set; }

        public class collectTransactionDetail
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            public string voucherNo { get; set; }

            public string invoiceNo { get; set; }
            public DateTime invoiceDate { get; set; }
            public string supplierName { get; set; }
            public decimal debt { get; set; }
            public decimal remains { get; set; }
            public decimal amount { get; set; }
        }

        public class financeTransactionDetail
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Column(Order = 0)]
            public long sId { get; set; }

            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order=1)]
            [StringLength(13)]
            [DisplayName("Transaction No.")]
            public string voucherNo { get; set; }

            public string invoiceNo { get; set; }
            public decimal amount { get; set; }
        }
    }
}