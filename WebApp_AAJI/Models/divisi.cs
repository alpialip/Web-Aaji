﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class divisi
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int divisiID { get; set; }

        [Required]
        [DisplayName("Department Name")]
        public int deptID { get; set; }
        
        [Required]
        [DisplayName("Division Name")]
        [StringLength(50)]
        public string divisiName { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}