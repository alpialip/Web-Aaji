using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
        public class menu
        {
            [Key]
            public int menuID { get; set; }
            [Required]
            [StringLength(50)]
            [DisplayName("Menu Name")]
            public string menuName { get; set; }
            [StringLength(50)]
            [DisplayName("Link")]
            public string menuLink { get; set; }

            [DisplayName("Parent Menu")]
            public int menuParent { get; set; }
            [StringLength(200)]
            [DisplayName("Description")]
            public string menuDescription { get; set; }
            [DisplayName("Is Parent")]
            public bool menuIsParent { get; set; }
            [DisplayName("Is Active")]
            public bool menuIsActive { get; set; }
            public DateTime? createdDate { get; set; }
            public string createdUser { get; set; }
            public DateTime? modifiedDate { get; set; }
            public string modifiedUser { get; set; }
        }
}