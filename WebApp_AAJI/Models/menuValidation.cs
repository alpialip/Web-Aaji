using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApp_AAJI.Models
{
    public class menuValidation
    {
        public menuValidation()
        {
            this.ActionAuth = new List<SelectActionAuthorize>();
            this.MenuAuth = new List<SelectMenuAuthorize>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(Order=0)]
        public int menuValId { get; set; }

        public bool validationTypeIsGeneral { get; set; }
        [DisplayName("Group Name")]
        [StringLength(250)]
        [Required]
        public string validationName { get; set; }

        //[Key]
        //[Column(Order = 2)]
        [Required]
        [DisplayName("Assign User")]
        public string userID { get; set; }

        [DisplayName("Is Active")]
        public bool isActive { get; set; }
        public DateTime createdDate { get; set; }
        public string createdUser { get; set; }
        public DateTime? modifiedDate { get; set; }
        public string modifiedUser { get; set; }

        public List<SelectActionAuthorize> ActionAuth { get; set; }
        public List<SelectMenuAuthorize> MenuAuth { get; set; }

#region unused
        //public class menuValidationD
        //{
        //    [Key]
        //    [Column(Order=0)]
        //    public int menuValIdH { get; set; }

        //    [Key]
        //    [Column(Order = 1)]
        //    public string userID { get; set; }

        //    [Required]
        //    public string validationAccess { get; set; }
        //    public DateTime modifiedDate { get; set; }
        //    public string modifiedUser { get; set; }

        //}
#endregion

        public class menuValidationDetail
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 0)]
            public int menuValIdH { get; set; }

            [Key]
            [Column(Order = 1)]
            public int menuID { get; set; }

            [DisplayName("Action Authorized")]
            public string validationAccess { get; set; }
        }

        public class menuValidationView
        {
            [Key]
            public int menuValId { get; set; }
            [Key]
            public int menuID { get; set; }
            [DisplayName("Assign User")]
            public string userName { get; set; }
            [DisplayName("Assign Menu")]
            public int menuName { get; set; }
            [DisplayName("Validation Access")]
            public string validationAccess { get; set; }
            [DisplayName("Is Active")]
            public bool isActive { get; set; }
        }

        public class SelectActionAuthorize
        {
            public bool Selected { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
        }

        public class SelectMenuAuthorize
        {
            public bool menuSelected { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}