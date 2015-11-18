using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class typeFinanceTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [StringLength(10)]
        [DisplayName("Transaction Type")]
        public string tftID { get; set; }

        [Required]
        [StringLength(100)]
        [DisplayName("Type Name")]
        public string tftName { get; set; }

        [Required]
        [StringLength(8)]
        [DisplayName("Account No.")]
        public string accountNo { get; set; }

        public DateTime createdDate { get; set; }
        public string createdUser { get; set; }
        public DateTime? modifiedDate { get; set; }
        public string modifiedUser { get; set; }
    }
}