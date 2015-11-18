using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class transPengurangPenghasilan
    {
        [Required]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order=0)]
        [DisplayName("NIK")]
        public string employeeNIK { get; set; }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 1)]
        [Required]
        [DisplayName("Subject")]
        public int idPengurangPenghasilan { get; set; }

        [Required]
        [DisplayName("Amount")]
        public decimal amount { get; set; }

        [Required]
        public DateTime modifiedDate { get; set; }

        [Required]
        public string modifiedUser { get; set; }
    }
}