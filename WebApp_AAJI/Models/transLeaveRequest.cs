using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
	public class transLeaveRequest
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Column(Order = 0)]
		public int leaveRequestID { get; set; }

		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Column(Order = 1)]
		[DisplayName("Nama Karyawan")]
		public int employeeID { get; set; }

		[DisplayName("Tanggal Awal")]
		public DateTime leaveStartDate { get; set; }

		[DisplayName("Tanggal Akhir")]
		public DateTime leaveEndDate { get; set; }

		[Required]
		[DisplayName("Kategori")]
		public string category { get; set; }

		[Required]
		[DisplayName("Keterangan")]
		public string leaveDescription { get; set; }

		[DisplayName("Cuti di Hari Libur")]
		public int cutiInHoliday { get; set; }

		public string approvedBy { get; set; }
		public DateTime? approvedDate { get; set; }
		[DisplayName("Approved")]
		public bool? approvedStatus { get; set; }

		public string acknowledgeBy { get; set; }
		public DateTime? acknowledgeDate { get; set; }
		[DisplayName("Acknowledge")]
		public bool? acknowledgeStatus { get; set; }

		[DisplayName("Created User")]
		public string createdUser { get; set; }
		public DateTime createdDate { get; set; }
		public string modifiedUser { get; set; }
		public DateTime? modifiedDate { get; set; }
	}
}