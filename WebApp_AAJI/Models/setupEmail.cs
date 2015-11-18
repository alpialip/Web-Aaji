using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class setupEmail
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        [Required]
        [StringLength(250)]
        [DisplayName("Host")]
        public string host { get; set; }
        [Required]
        [StringLength(10)]
        [DisplayName("Port")]
        public string port { get; set; }
        [StringLength(250)]
        [DisplayName("Email Name")]
        public string emailName { get; set; }
        [EmailAddress]
        [StringLength(250)]
        [DisplayName("Email Address")]
        public string emailAddress { get; set; }
        public DateTime modifiedDate { get; set; }
        public string modifiedUser { get; set; }


    }
}