using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class typeAbsensi
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int typeAbsensiID { get; set; }

        [Required]
        [DisplayName("Name")]
        [StringLength(250)]
        public string typeAbsensiName { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}