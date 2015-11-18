using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class accountPayableHeader
    {
        public accountPayableHeader()
        {
            this.detailAccountPayable = new List<accountPayableDetail>();
            this.viewDetailAccountPayable = new List<accountPayableDetailView>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("Voucher No")]
        public string voucherNo { get; set; }

        [Required]
        [DisplayName("Date")]
        public DateTime date { get; set; }

        [Required]
        [DisplayName("Bank")]
        public int bankId { get; set; }

        [Required]
        [DisplayName("Supplier")]
        public string vendorId { get; set; }

        [Required]
        [DisplayName("Payment Type")]
        public string paymentType { get; set; }

        [DisplayName("No.Trans")]
        public string noTrans { get; set; }

        [DisplayName("Remarks")]
        public string remarks { get; set; }


        [DisplayName("Created")]
        public string createdUser { get; set; }

        [DisplayName("Date")]
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }

        public List<accountPayableDetail> detailAccountPayable { get; set; }
        public List<accountPayableDetailView> viewDetailAccountPayable { get; set; }

        public class accountPayableDetail
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 0)]
            public string voucherNo { get; set; }

            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 1)]
            public string invoiceNo { get; set; }

            public decimal amountPayment { get; set; }
        }

        public class accountPayableDetailView
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            public string invoiceNo { get; set; }

            public decimal amountDebt { get; set; }

            public decimal amountPayment { get; set; }
        }
    }
}