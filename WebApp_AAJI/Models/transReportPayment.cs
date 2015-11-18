using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class transReportPayment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string voucherNo { get; set; }
        public string paymentType { get; set; }
        public string modifiedUser { get; set; }
        public DateTime modifiedDate { get; set; }
    }
}