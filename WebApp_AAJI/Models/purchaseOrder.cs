using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class purchaseOrderHeader
    {
        public purchaseOrderHeader()
        {
            this.detailPurchaseOrder = new List<purchaseOrderDetail>();
            this.detailPurchaseTop = new List<purchaseOrderTop>();
            this.PopUpPR = new List<prPopUp>();
            this.PopUpPO = new List<POpopUp>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("PO No.")]
        public string poId { get; set; }

        [DisplayName("PO Date")]
        [Required]
        public DateTime poDate { get; set; }

        [DisplayName("PR No.")]
        [Required]
        public string prId { get; set; }

        [DisplayName("Vendor")]
        [Required]
        public string vendorId { get; set; }

        [StringLength(7)]
        public string poUrgent { get; set; }

        [DisplayName("TOP")]
        [Required]
        [StringLength(10)]
        public string topType { get; set; }

        public int topAmount { get; set; }

        [DisplayName("PPN")]
        [Required]
        public int ppn { get; set; }

        [DisplayName("Disc")]
        [Required]
        public decimal disc { get; set; }

        [DisplayName("Proposed By")]
        public string proposedBy { get; set; }
        [DisplayName("Proposed Date")]
        public DateTime proposedDate { get; set; }
        [DisplayName("Proposed")]
        public bool proposedStatus { get; set; }

        public string approvedBy { get; set; }
        public DateTime? approvedDate { get; set; }
        [DisplayName("Approved")]
        public bool? approvedStatus { get; set; }

        public string acknowledgeBy { get; set; }
        public DateTime? acknowledgeDate { get; set; }
        [DisplayName("Acknowledge")]
        public bool? acknowledgeStatus { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }

        public List<purchaseOrderDetail> detailPurchaseOrder { get; set; }
        public List<purchaseOrderTop> detailPurchaseTop { get; set; }
        public List<prPopUp> PopUpPR { get; set; }

        public List<POpopUp> PopUpPO { get; set; }
        public class purchaseOrderDetail
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 0)]
            public string poId { get; set; }

            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 1)]
            public int productID { get; set; }
            public int qty { get; set; }
            public decimal price { get; set; }
            public decimal disc { get; set; }
        }

        public class purchaseOrderTop
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Column(Order = 0)]
            public int id { get; set; }

            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 1)]
            public string poId { get; set; }
            public DateTime buyDate { get; set; }
            public decimal buyPercent { get; set; }
        }

        public class prPopUp
        {
            [Key]
            public string prId { get; set; }
            public int requestDeptId { get; set; }
            public string deptName { get; set; }
            public string typeOrder { get; set; }
            public bool proposalInclude { get; set; }
            public string specialInstruction { get; set; }
            public string projectTimeDelivery { get; set; }
        }

        public class POpopUp
        {
            [Key]
            public string poId { get; set; }
            public DateTime poDate { get; set; }
            public string vendorId { get; set; }
            public string vendorName { get; set; }
            public string poUrgent { get; set; }
        }

    }
}