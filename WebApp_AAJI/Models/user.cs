using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApp_AAJI.Models
{
    public class user
    {
        public user()
        {
            this.userCheckBox = new List<SelectActionUser>();
        }

        [Key]
        [Required]
        [StringLength(20)]
        [DisplayName("User ID")]
        public string userID { get; set; }

        [Required]
        [StringLength(50)]
        [DisplayName("User Name")]
        public string userName { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [DisplayName("Email")]
        public string email { get; set; }

        [DisplayName("Department")]
        public int deptID { get; set; }

        [PasswordPropertyText(true)]
        [DisplayName("Password")]
        public string password { get; set; }

        [DisplayName("Is Active")]
        public bool isActive { get; set; }

        //[Display(AutoGenerateField = false)]
        //[HiddenInput(DisplayValue = false)]
        [DisplayName("Last Login")]
        public DateTime? lastLogin { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }
        public string employeeNIK { get; set; }
        public bool isAdmin { get; set; }
        public List<SelectActionUser> userCheckBox { get; set; }

        public class SelectActionUser
        {
            public bool userSelected { get; set; }
            [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
            public string userId { get; set; }
            public string userName { get; set; }
        }
    }
}