using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class absensiRekap
    {
        public absensiRekap()
        {
            this.detailRekapAbsensi = new List<RekapAbsensiDetail>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        [DisplayName("Date")]
        [Required]
        public DateTime absensiDate { get; set; }

        [DisplayName("Employee ID")]
        public int employeeID { get; set; }

        [DisplayName("Absensi")]
        public int typeAbsensiID { get; set; }

        [DisplayName("Remarks")]
        public string remarks { get; set; }

        public List<RekapAbsensiDetail> detailRekapAbsensi{get;set;}

        public class RekapAbsensiDetail
        {
            [Key]
            public int employeeID{get;set;}
            public string employeeNIK{get;set;}
            public string employeeName{get;set;}
            public string deptName{get;set;}
            public int typeAbsensiId {get;set;}
            public string remarks{get;set;}
        }
    }
}