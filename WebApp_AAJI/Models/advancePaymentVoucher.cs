using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class advancePaymentVoucher
    {
        public advancePaymentVoucher()
        {
            this.detailAdvancePaymentVoucher = new List<advancePaymentVoucherDetail>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("Voucher No.")]
        public string voucherNo { get; set; }

        [DisplayName("Voucher Date")]
        [Required]
        public DateTime voucherDate { get; set; }

        [DisplayName("Bank")]
        [Required]
        public int bankID { get; set; }

        [DisplayName("Remark")]
        [StringLength(250)]
        public string remarks { get; set; }

        [StringLength(250)]
        public string says { get; set; }

        [DisplayName("Prepared By")]
        public string preparedBy { get; set; }
        [DisplayName("Prepared Date")]
        public DateTime preparedDate { get; set; }
        [DisplayName("Status")]
        public bool preparedStatus { get; set; }

        public string checkedBy { get; set; }
        public DateTime? checkedDate { get; set; }
        public bool? checkedStatus { get; set; }

        public string approvedBy { get; set; }
        public DateTime? approvedDate { get; set; }
        [DisplayName("Approved")]
        public bool? approvedStatus { get; set; }

        public string acknowledgeBy { get; set; }
        public DateTime? acknowledgeDate { get; set; }
        public bool? acknowledgeStatus { get; set; }
        
        public int requestDeptId {get;set;}
        public DateTime? received{get;set;}
        public bool settlement{get;set;}

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }

        public List<advancePaymentVoucherDetail> detailAdvancePaymentVoucher { get; set; }

        public class advancePaymentVoucherDetail
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 0)]
            public string voucherNo { get; set; }

            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 1)]
            public string accountNo { get; set; }

            public decimal amount { get; set; }
        }
    }
}