using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class chartOfAccount
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order=0)]
        public int id { get; set; }

        [Key]
        [Column(Order = 1)]
        public string accountNo { get; set; }

        [Required]
        [DisplayName("Account Name")]
        [StringLength(100)]
        public string accountName { get; set; }

        [Required]
        [DisplayName("Level")]
        public int levelID { get; set; }

        [Required]
        [DisplayName("Parent COA")]
        public int? parentCOAId { get; set; }

        public DateTime createdDate { get; set; }
        public string createdUser { get; set; }
        public DateTime? modifiedDate { get; set; }
        public string modifiedUser { get; set; }

    }
}