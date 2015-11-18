using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class transVendorProduct
    {
        [Required]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 0)]
        [DisplayName("Vendor")]
        public int id_vendor { get; set; }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        [Column(Order = 1)]
        [DisplayName("Product")]
        public int productID { get; set; }

        [Required]
        public DateTime modifiedDate { get; set; }

        [Required]
        public string modifiedUser { get; set; }
    }
}