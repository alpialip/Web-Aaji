using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class employeeLoan
    {
        public employeeLoan()
        {
            this.popUpEmployee = new List<employeePOPUp>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int pinjamanID { get; set; }

        [Required]
        [DisplayName("Employee")]
        public int employeeID { get; set; }

        [Required]
        [DisplayName("Amount")]
        public decimal amount { get; set; }

        [Required]
        [DisplayName("Installment")]
        public decimal installment { get; set; }

        [Required]
        [DisplayName("Installment Duration")]
        public int installmentDuration { get; set; }

        [DisplayName("Collateral")]
        public string collateral { get; set; }

        [DisplayName("Remark")]
        [StringLength(250)]
        [Required]
        public string remarks { get; set; }

        [DisplayName("Loan Category")]
        [StringLength(50)]
        [Required]
        public string loanCategory { get; set; }

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

        public List<employeePOPUp> popUpEmployee { get; set; }

        public class employeePOPUp
        {
            [Key]
            public int employeeID {get;set;}
            public string employeeNIK {get;set;}
            public string employeeName {get;set;}
            public int deptID {get;set;}
            public string deptName { get; set; }
            public decimal gapok { get; set; }
        }
    }
}