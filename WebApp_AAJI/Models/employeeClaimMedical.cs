using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class employeeClaimMedical
    {
        public employeeClaimMedical()
        {
            this.claimMedicalDetail = new List<employeeClaimMedicalDetail>();
            this.listFamily = new List<familyList>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int klaimID { get; set; }

        [Required]
        [DisplayName("Date")]
        public DateTime klaimDate { get; set; }

        [Required]
        [DisplayName("Employee")]
        public int employeeID { get; set; }

        [DisplayName("Document")]
        public string linkFileData { get; set; }

        [Required]
        [DisplayName("Amount")]
        public decimal amount { get; set; }

        [Required]
        [DisplayName("Remarks")]
        public string remarks { get; set; }

        [DisplayName("Proposed By")]
        public string proposedBy { get; set; }
        [DisplayName("Proposed Date")]
        public DateTime proposedDate { get; set; }
        [DisplayName("Proposed")]
        public bool proposedStatus { get; set; }

        [DisplayName("Approved By")]
        public string approvedBy { get; set; }
        [DisplayName("Approved Date")]
        public DateTime? approvedDate { get; set; }
        [DisplayName("Approved")]
        public bool? approvedStatus { get; set; }

        public string acknowledgeBy { get; set; }
        public DateTime? acknowledgeDate { get; set; }
        public bool? acknowledgeStatus { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }


        [DisplayName("Lab")]
        public string lab { get; set; }

        [DisplayName("Medicine")]
        public string medicine { get; set; }

        [DisplayName("Usage")]
        public string usage { get; set; }

        public List<employeeClaimMedicalDetail> claimMedicalDetail { get; set; }
        public List<familyList> listFamily { get; set; }

        public class employeeClaimMedicalDetail
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 0)]
            public int klaimID { get; set; }

            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 1)]
            public int seqID { get; set; }

            public string claimType { get; set; }
            public string relationStatus { get; set; }
            public DateTime happeningDate { get; set; }

            public int relationSeqID { get; set; }
            public decimal amount { get; set; }
            public string remarks { get; set; }
        }

        public class familyList
        {
            [Key]
            public int employeeID { get; set; }
            public int sId { get; set; }
            public string name { get; set; }
            public DateTime birthdate { get; set; }
            public string birthplace { get; set; }
            public string education { get; set; }
            public string statusRelation { get; set; }
            public string gender { get; set; }
        }


    }
}