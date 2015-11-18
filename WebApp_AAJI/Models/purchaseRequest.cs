using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class purchaseRequestHeader
    {
        public purchaseRequestHeader()
        {
            this.detailPurchaseRequest = new List<purchaseRequestDetail>();
            this.popUpPR = new List<PRPopUp>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(13)]
        [DisplayName("PR No.")]
        public string prId { get; set; }

        [DisplayName("PR Date")]
        [Required]
        public DateTime prDate { get; set; }

        [DisplayName("Request By")]
        [Required]
        public int requestDeptId { get; set; }

        [DisplayName("Type Order")]
        [Required]
        public string typeOrder { get; set; }
        
        [DisplayName("Proposal Include")]
        [Required]
        public bool proposalInclude { get; set; }

        [DisplayName("Special Instruction")]
        [StringLength(250)]
        public string specialInstruction { get; set; }

        [DisplayName("Project Time Delivery")]
        [StringLength(250)]
        public string projectTimeDelivery { get; set; }

        public string proposedBy { get; set; }
        public DateTime proposedDate { get; set; }
        [DisplayName("Proposed")]
        public bool proposedStatus { get; set; }

        public string approvedBy { get; set; }
        public DateTime? approvedDate { get; set; }
        [DisplayName("Approved")]
        public bool? approvedStatus { get; set; }

        public string acknowledgeBy { get; set; }
        public DateTime? acknowledgeDate { get; set; }
        public bool? acknowledgeStatus { get; set; }

        public string receivedBy { get; set; }
        public DateTime? receivedDate { get; set; }
        public bool? receivedStatus { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }
        public List<purchaseRequestDetail> detailPurchaseRequest { get; set; }
        public List<PRPopUp> popUpPR { get; set; }

        public class purchaseRequestDetail
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 0)]
            public string prId { get; set; }
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order=1)]
            public int productId { get; set; }
            public string description { get; set; }
            public int qty { get; set; }
            public string unit { get; set; }

            public int? vendorId { get; set; }
        }

        public class PRPopUp
        {
            [Key]
            public string prId { get; set; }
            public int deptId { get; set; }
            public string deptName { get; set; }
            public string typeOrder { get; set; }
            public bool proposalInclude { get; set; }
            public string specialInstruction { get; set; }
            public string projectTimeDelivery { get; set; }
        }
    }
}