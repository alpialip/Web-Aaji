using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class product
    {
        public product()
        {
            this.productList = new List<listProduct>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int productID { get; set; }

        [Required]
        [DisplayName("Code")]
        [StringLength(50)]
        public string productCode { get; set; }

        [Required]
        [DisplayName("Name")]
        [StringLength(250)]
        public string productName { get; set; }

        [Required]
        [DisplayName("Unit")]
        public string unit { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }

        public List<listProduct> productList { get; set; }
        public class listProduct
        {
            [Key]
            public int productID { get; set; }
            public string productCode { get; set; }

            public string productName { get; set; }
            public string unit { get; set; }

            public string createdUser { get; set; }
            public DateTime createdDate { get; set; }
            public string modifiedUser { get; set; }
            public DateTime? modifiedDate { get; set; }
        }
    }
}