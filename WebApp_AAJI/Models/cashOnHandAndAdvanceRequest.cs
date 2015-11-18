using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class cashOnHandAndAdvanceRequest
    {
        public cashOnHandAndAdvanceRequest()
        {
            this.detailCashOnHandAndAdvanceRequest = new List<cashOnHandAndAdvanceRequestDetail>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("Req.No")]
        [Column(Order=0)]
        [StringLength(13)]
        public string reqNo { get; set; }

        [DisplayName("Date")]
        [Required]
        public DateTime date { get; set; }

        [DisplayName("From")]
        [StringLength(12)]
        [Required]
        public string reqFrom { get; set; }

        [DisplayName("Activity")]
        [StringLength(50)]
        [Required]
        public string activity { get; set; }

        public int coaActivity { get; set; }

        [DisplayName("Clearing Date")]
        [Required]
        public DateTime clearanceDate { get; set; }

        [DisplayName("Req.Type")]
        [Required]
        public bool reqIsCashOnHand { get; set; }

        [DisplayName("Incl.Proposal")]
        public bool includeProposal { get; set; }

        [DisplayName("Reimbursement")]
        public bool isReimbursement { get; set; }

        [DisplayName("Created Date")]
        public DateTime createdDate { get; set; }

        [DisplayName("Created User")]
        public string createdUser { get; set; }
        public DateTime? modifiedDate { get; set; }
        public string modifiedUser { get; set; }

        public string approvedBy { get; set; }
        public DateTime? approvedDate { get; set; }
        [DisplayName("Approved")]
        public bool? approvedStatus { get; set; }

        public List<cashOnHandAndAdvanceRequestDetail> detailCashOnHandAndAdvanceRequest { get; set; }

        public class cashOnHandAndAdvanceRequestDetail
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order=0)]
            public int seqId { get; set; }

            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 1)]
            public string reqNo { get; set; }
            public string activityDescription { get; set; }
            public int currency { get; set; }
            public decimal amount { get; set; }
            public string fileReimbursement { get; set; }
        }


    }
}