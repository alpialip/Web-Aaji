using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
	public class sisaSaldoCuti
	{
		public sisaSaldoCuti()
		{
			this.detailSaldoCuti = new List<saldoCutiDetail>();
		}

		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int id { get; set; }

		[Required]
		[DisplayName("Tahun")]
		public int year { get; set; }
		public int employeeID { get; set; }

		[DisplayName("Cuti Tahunan Disetujui")]
		public int countedApproved { get; set; }

		[DisplayName("Sisa Cuti Tahunan")]
		public int amount { get; set; }

		[DisplayName("Cuti Persalinan Disetujui")]
		public int countedApproved2 { get; set; }

		[DisplayName("Sisa Cuti Persalinan")]
		public int amount2 { get; set; }

		[DisplayName("Cuti Kemalangan Disetujui")]
		public int countedApproved3 { get; set; }

		[DisplayName("Sisa Cuti Kemalangan")]
		public int amount3 { get; set; }

		[DisplayName("Cuti Lain-lain Disetujui")]
		public int countedApproved4 { get; set; }

		[DisplayName("Sisa Cuti Lain-lain")]
		public int amount4 { get; set; }

		public List<saldoCutiDetail> detailSaldoCuti { get; set; }
		public class saldoCutiDetail
		{
			[Key]
			public int employeeID { get; set; }
			public string employeeNIK { get; set; }
			public string employeeName { get; set; }
			public string deptName { get; set; }
			public int amount { get; set; }
		}
	}
}