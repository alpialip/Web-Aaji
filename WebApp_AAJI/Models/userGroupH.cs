using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class userGroupH
    {
        public userGroupH()
        {
            this.User = new List<SelectPersonEditorViewModel>();
        }

        [Key]
        [Required]
        public int id { get; set; }
        [DisplayName("Group Name")]
        [Required]
        public string groupName { get; set; }
        [DisplayName("Description")]
        public string groupDesc { get; set; }
        [DisplayName("Is Active")]
        [Required]
        public bool isActive { get; set; }
        public DateTime? createdDate { get; set; }
        public string createdUser { get; set; }
        public DateTime? modifiedDate { get; set; }
        public string modifiedUser { get; set; }
        public List<SelectPersonEditorViewModel> User { get; set; }

        public class userGroupD
        {
            [Key]
            [Column(Order=0)]
            [Required]
            public int groupHId { get; set; }

            [Key]
            [Column(Order = 1)]
            [Required]
            public string userID { get; set; }
            public DateTime? modifiedDate { get; set; }
            public string modifiedUser { get; set; }
        }

        public class SelectPersonEditorViewModel
        {
            public bool Selected { get; set; }
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}