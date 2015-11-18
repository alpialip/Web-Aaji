using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class level
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int levelID { get; set; }

        [Required]
        [DisplayName("Nama Level")]
        public string levelName { get; set; }

        [Required]
        [DisplayName("Jatah Cuti Tahunan")]
        public int saldoCuti { get; set; }

	 [Required]
	 [DisplayName("Jatah Cuti Persalinan")]
	 public int saldoCuti2 { get; set; }

	 [Required]
	 [DisplayName("Jatah Cuti Kemalangan")]
	 public int saldoCuti3 { get; set; }

	 [Required]
	 [DisplayName("Jatah Cuti Lain-lain")]
	 public int saldoCuti4 { get; set; }

	 [Required]
	 [DisplayName("Gaji Pokok (Minimum)")]
	 public int GajiTerendah { get; set; }

	 [Required]
	 [DisplayName("Gaji Pokok (Maksimum)")]
	 public int GajiTertinggi { get; set; }
        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }
    }
}