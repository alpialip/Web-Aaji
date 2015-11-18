using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class employee
    {
        public employee()
        {
            this.resignEmployee = new List<employeeResign>();
            this.positionEmployee = new List<employeePosition>();
            this.mateEmployee = new List<employeeMate>();
            this.childEmployee = new List<employeeChild>();
            this.cvEmployee = new List<employeeCV>();
            this.educationEmployee = new List<employeeEducation>();
            this.occupationEmployee = new List<employeeOccupation>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int employeeID { get; set; }

        [DisplayName("Nama Karyawan")]
        [Required]
        [StringLength(50)]
        public string employeeName { get; set; }

        [DisplayName("ID Karyawan")]
        [Required]
        [StringLength(10)]
        public string employeeNIK { get; set; }

        [DisplayName("Email")]
        //[Required]
        [StringLength(100)]
        [EmailAddress]
        public string email { get; set; }
        
        [DisplayName("NPWP")]
        //[Required]
        [StringLength(50)]
        public string npwp { get; set; }

        [DisplayName("KTP")]
        [Required]
        [StringLength(50)]
        public string ktp { get; set; }

        [DisplayName("Alamat")]
        [Required]
        [StringLength(200)]
        public string address { get; set; }

        [DisplayName("Kota")]
        [Required]
        [StringLength(50)]
        public string city { get; set; }

        [DisplayName("Bank Account")]
        //[Required]
        [StringLength(100)]
        public string bankName { get; set; }

        [DisplayName("Account No.")]
        //[Required]
        [StringLength(30)]
        public string rekening { get; set; }

        [DisplayName("Tempat Tanggal Lahir")]
        [Required]
        [StringLength(100)]
        public string tempatLahir { get; set; }

	 [DisplayName("Tempat Tanggal Lahir")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]  
        [Required]
        public DateTime tanggalLahir { get; set; }

        [DisplayName("Foto Profil")]
        [StringLength(250)]
        public string linkFilePhoto { get; set; }


        [DisplayName("Gaji Pokok")]
        public decimal gapok { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }
        public string virtualSign { get; set; }

        [DisplayName("Jenis Kelamin")]
        [StringLength(1)]
        public string jenisKelamin { get; set; }

        [DisplayName("RT/RW")]
        [StringLength(3)]
        public string rt { get; set; }

        [StringLength(3)]
        public string rw { get; set; }

        [DisplayName("Kelurahan")]
        public string kelurahan { get; set; }

        [DisplayName("Kecamatan")]
        public string kecamatan { get; set; }

        [DisplayName("Agama")]
        public string agama { get; set; }

        [DisplayName("Status Pernikahan")]
        public string statusNikah { get; set; }

        [DisplayName("Pekerjaan")]
        public string pekerjaan { get; set; }

        [DisplayName("Kewarganegaraan")]
        public string kewarganegaraan { get; set; }

        public List<employeePosition> positionEmployee { get; set; }
        public List<employeeResign> resignEmployee { get; set; }
        public List<employeeMate> mateEmployee { get; set; }
        public List<employeeChild> childEmployee { get; set; }
        public List<employeeOccupation> occupationEmployee { get; set; }
        public List<employeeEducation> educationEmployee { get; set; }
        public List<employeeCV> cvEmployee { get; set; }

        public class employeePosition
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int positionId { get; set; }
            public DateTime positionDate { get; set; }
            public int employeeID { get; set; }
            public string jobTitle { get; set; }
            public int deptID { get; set; }
            public int divisiID { get; set; }
            public int levelID { get; set; }
            public string employeeStatus { get; set; }
        }

        public class employeeResign
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int id { get; set; }
            public int employeeID { get; set; }
            public DateTime resignDate { get; set; }
            public string remarks { get; set; }
        }

        public class employeeChild
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 0)]
            public int employeeID { get; set; }
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Column(Order=1)]
            public int sId { get; set; }
            public string childName { get; set; }
            public DateTime? childBirthDate { get; set; }
            public string childBirthPlace { get; set; }
            public string lastEducation { get; set; }

            public string gender { get; set; }
        }

        public class employeeMate
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int employeeID { get; set; }
            public string coupleName { get; set; }
            public DateTime? coupleBirthDate { get; set; }
            public string coupleBirthPlace { get; set; }
            public string lastEducation { get; set; }
            public string occupation { get; set; }
            public string status { get; set; }
            public string gender { get; set; }
        }

        public class employeeOccupation
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 0)]
            public int employeeID { get; set; }
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            [Column(Order = 1)]
            public int sId { get; set; }
            public DateTime? periodStart { get; set; }
            public DateTime? periodEnd { get; set; }
            public string company { get; set; }
            public string jobdesc { get; set; }
        }
        public class employeeEducation
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int employeeID { get; set; }

            public string sdNamaSekolah { get; set; }
            public string sdKotaSekolah { get; set; }
            public string sdJurusanSekolah { get; set; }
            public string sdPeriod { get; set; }
            public string sdKeterangan { get; set; }

            public string smpNamaSekolah { get; set; }
            public string smpKotaSekolah { get; set; }
            public string smpJurusanSekolah { get; set; }
            public string smpPeriod { get; set; }
            public string smpKeterangan { get; set; }

            public string smaNamaSekolah { get; set; }
            public string smaKotaSekolah { get; set; }
            public string smaJurusanSekolah { get; set; }
            public string smaPeriod { get; set; }
            public string smaKeterangan { get; set; }

            public string akademiNamaSekolah { get; set; }
            public string akademiKotaSekolah { get; set; }
            public string akademiJurusanSekolah { get; set; }
            public string akademiPeriod { get; set; }
            public string akademiKeterangan { get; set; }

            public string pascaNamaSekolah { get; set; }
            public string pascaKotaSekolah { get; set; }
            public string pascaJurusanSekolah { get; set; }
            public string pascaPeriod { get; set; }
            public string pascaKeterangan { get; set; }
        }
        public class employeeCV
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int employeeID { get; set; }
            public string linkFileCV { get; set; }
        }

    }
}