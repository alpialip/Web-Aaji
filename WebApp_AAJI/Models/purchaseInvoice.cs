using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class purchaseInvoice
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("Invoice No.")]
        public string purchaseInvoiceNo { get; set; }

        [DisplayName("Invoice Date")]
        [Required]
        public DateTime invoiceDate { get; set; }

        [DisplayName("Due Date")]
        [Required]
        public DateTime dueDate { get; set; }

        [DisplayName("Receive No.")]
        [Required]
        [StringLength(50)]
        public string receiveNo { get; set; }

        [DisplayName("Payment Info")]
        [StringLength(250)]
        public string paymentInfo { get; set; }

        [DisplayName("Remarks")]
        [StringLength(250)]
        public string remarks { get; set; }

        [DisplayName("Total")]
        [Required]
        public decimal total { get; set; }

        [DisplayName("Invoice Reference")]
        public string invoiceReference { get; set; }

        [DisplayName("Check Document")]
        public bool? isCheckDocument { get; set; }
        public DateTime? isCheckDocumentDate { get; set; }
        public string isCheckDocumentUser { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}