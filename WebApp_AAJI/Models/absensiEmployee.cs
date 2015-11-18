using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class absensiEmployee
    {
        public absensiEmployee()
        {
            this.detailEmployeeAbsensi = new List<absensiEmployeeDetail>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public int employeeID { get; set; }
        public DateTime date { get; set; }
        public TimeSpan? checkIn { get; set; }
        public TimeSpan? checkOut { get; set; }
        public int? typeAbsensiID { get; set; }
        public string remarks { get; set; }
        public List<absensiEmployeeDetail> detailEmployeeAbsensi {get;set;}

        public class absensiEmployeeDetail
        {
            [Key]
            public int id { get; set; }
             public int employeeID { get; set; }
            public DateTime date { get; set; }
            public TimeSpan? checkIn { get; set; }
            public TimeSpan? checkOut { get; set; }
            public int? typeAbsensiID { get; set; }
            public string remarks { get; set; }
        }
    }
}