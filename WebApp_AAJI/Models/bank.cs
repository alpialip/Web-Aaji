using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class bank
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int bankID { get; set; }

        [Required]
        [DisplayName("Bank Name")]
        [StringLength(100)]
        public string bankName { get; set; }

        [Required]
        [DisplayName("Account No.")]
        [StringLength(50)]
        public string accountNo { get; set; }

        [Required]
        [DisplayName("COA")]
        public int coaID { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}