using System;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
	public class ViewModelCuti
	{
		public int leaveRequestID { get; set; }

		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Column(Order = 1)]
		[DisplayName("Nama Karyawan")]
		public int employeeID { get; set; }

		[Required]
		[DisplayName("Nama Karyawan")]
		public string employeeName { get; set; }

		[DisplayName("Tanggal Awal Cuti")]
		public DateTime leaveStartDate { get; set; }

		[DisplayName("Tanggal Akhir Cuti")]
		public DateTime leaveEndDate { get; set; }

		[Required]
		[DisplayName("Kategori")]
		public string category { get; set; }

		[Required]
		[DisplayName("Keterangan")]
		public string leaveDescription { get; set; }

		[DisplayName("Cuti di Hari Libut")]
		public int cutiInHoliday { get; set; }

		public int level { get; set; }

		[DisplayName("Jatah Cuti Tahunan")]
		public int saldoCuti { get; set; }
		[DisplayName("Jatah Cuti Persalinan")]
		public int saldoCuti2 { get; set; }
		[DisplayName("Jatah Cuti Kemalangan")]
		public int saldoCuti3 { get; set; }
		[DisplayName("Jatah Cuti Lain-lain")]
		public int saldoCuti4 { get; set; }
		[DisplayName("Sisa Cuti Tahunan")]
		public int amount { get; set; }
		[DisplayName("Sisa Cuti Persalinan")]
		public int amount2 { get; set; }
		[DisplayName("Sisa Cuti Kemalangan")]
		public int amount3 { get; set; }
		[DisplayName("Sisa Cuti Lain-lain")]
		public int amount4 { get; set; }
		[DisplayName("Cuti Tahunan Disetujui")]
		public int countedApproved { get; set; }
		[DisplayName("Cuti Persalinan Disetujui")]
		public int countedApproved2 { get; set; }
		[DisplayName("Cuti Kemalangan Disetujui")]
		public int countedApproved3 { get; set; }
		[DisplayName("Cuti Lain-lain Disetujui")]
		public int countedApproved4 { get; set; }


		//public level masterLevel { get; set; }
		//public sisaSaldoCuti sisaCuti { get; set; }
	}
}