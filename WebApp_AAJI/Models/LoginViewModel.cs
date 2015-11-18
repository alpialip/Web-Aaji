using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class LoginViewModel
    {
        public LoginViewModel()
        {
            this.detailUserLogin = new List<userLogin>();
        }


        [Required(ErrorMessage = "User ID Required")]
        public string UserID { get; set; }

        [Required(ErrorMessage = "Password Required")]
        [StringLength(20, MinimumLength = 1)]
        public string Password { get; set; }

        public List<userLogin> detailUserLogin { get; set; }


        public class userLogin
        {
            public userLogin()
            {
                this.listAuthorize = new List<string>();
            }

            public string userID { get; set; }
            public string password { get; set; }
            public string userName { get; set; }
            public int deptID { get; set; }
            public int divID { get; set; }
            public bool isAdmin { get; set; }
            public int employeeId { get; set; }
            public string employeeName { get; set; }
            public string employeeNIK { get; set; }
            public int employeePositionId { get; set; }
            public string employeeEmail { get; set; }
            public DateTime datePosition { get; set; }
            public int levelID { get; set; }
            public List<string> listAuthorize { get; set; }
        }
    }

    
}