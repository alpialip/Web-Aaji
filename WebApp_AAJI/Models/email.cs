using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class email
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int emailID { get; set; }
        [Required]
        [StringLength(50)]
        [DisplayName("Email Name")]
        public string emailName { get; set; }
        [EmailAddress]
        [StringLength(100)]
        [DisplayName("Email Address")]
        public string emailAddress { get; set; }
        public DateTime createdDate { get; set; }
        public string createdUser { get; set; }
        public DateTime? modifiedDate { get; set; }
        public string modifiedUser { get; set; }

    }
}