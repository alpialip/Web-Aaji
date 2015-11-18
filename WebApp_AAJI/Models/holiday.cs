using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class holiday
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int holidayID { get; set; }

        [Required]
        [Display(Name="Holiday Date")]
        public DateTime holidayDate { get; set; }

        [Required]
        [Display(Name = "Remarks")]
        public string remarks { get; set; }
    }
}