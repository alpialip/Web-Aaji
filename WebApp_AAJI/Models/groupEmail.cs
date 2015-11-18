using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class groupEmail
    {
        public groupEmail()
        {
            this.emailDistributed = new List<SelectEmailDistributed>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int groupEmailID { get; set; }
        [Required]
        [StringLength(100)]
        [DisplayName("Group Email Name")]
        public string groupEmailName { get; set; }
        [StringLength(250)]
        [DisplayName("Distributed Email")]
        public string emailAddress { get; set; }
        public DateTime modifiedDate { get; set; }
        public string modifiedUser { get; set; }

        public List<SelectEmailDistributed> emailDistributed { get; set; }

        public class SelectEmailDistributed
        {
            public bool Selected { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}