using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class transReminder
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [Required]
        [DisplayName("Subject")]
        [StringLength(250)]
        public string subject { get; set; }

        [Required]
        [DisplayName("Email")]
        [StringLength(250)]
        public string email { get; set; }

        [Required]
        [DisplayName("Reminder Start Date")]
        public DateTime reminderDate { get; set; }

        [Required]
        [DisplayName("Reminder Time")]
        public TimeSpan reminderTime { get; set; }


        [Required]
        [DisplayName("Repetition")]
        public string repeatitionID { get; set; }


        [Required]
        [DisplayName("Reminder")]
        public string reminderID { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}