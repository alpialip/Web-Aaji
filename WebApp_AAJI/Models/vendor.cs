using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class vendor
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [DisplayName("Vendor ID")]
        [StringLength(50)]
        public string vendorID { get; set; }

        [Required]
        [DisplayName("Vendor Name")]
        [StringLength(100)]
        public string vendorName { get; set; }

        [DisplayName("NPWP")]
        [StringLength(50)]
        public string npwp { get; set; }
                
        [DisplayName("Address 1")]
        [StringLength(250)]
        public string address1 { get; set; }

        [DisplayName("Address 2")]
        [StringLength(250)]
        public string address2 { get; set; }

        [DisplayName("Telephone")]
        [StringLength(50)]
        public string telp { get; set; }

        [DisplayName("Fax")]
        [StringLength(50)]
        public string fax { get; set; }

        [DisplayName("Contact Person")]
        [StringLength(50)]
        public string contactPerson { get; set; }

        [DisplayName("PPN")]
        public bool ppn { get; set; }

        [DisplayName("Term Of Payment")]
        public int top { get; set; }

        [DisplayName("Bank Account")]
        public int? bankRekening { get; set; }

        [DisplayName("Account No.")]
        public string noRekening { get; set; }

        [DisplayName("Account Name")]
        public string namaRekening { get; set; }


        
        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }        

    }
}