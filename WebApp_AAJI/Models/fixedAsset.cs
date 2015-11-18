using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class fixedAsset
    {
        public fixedAsset()
        {
            this.personFixedAsset = new List<fixedAssetPerson>();
            this.maintenanceFixedAsset = new List<fixedAssetMaintenance>();
            this.fixAssetPersonDetail = new List<detailSavedFixedAssetPerson>();
	     //purchaseDate = DateTime.Today;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int fixedAssetID { get; set; }
    
        [DisplayName("Purchase Date")]
	 [DataType(DataType.Date)]
	 [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? purchaseDate { get; set; }

        [DisplayName("Sales Date")]
        public DateTime? salesDate { get; set; }

        [DisplayName("Asset Type")]
        public string fixedAssetType { get; set; }

        [DisplayName("Asset Name")]
        public string fixedAssetName { get; set; }

        [DisplayName("Asset Code")]
        public string fixedAssetCode { get; set; }

        [DisplayName("Asset Status")]
        public string fixedAssetStatus { get; set; }

        [DisplayName("Amount")]
        public decimal amount { get; set; }

        [DisplayName("Depreciation")]
        public decimal depreciationProcentage { get; set; }
        public int depreciationValPeriod { get; set; }
        public string depreciationPeriod { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }

        public List<fixedAssetPerson> personFixedAsset { get; set; }
        public List<fixedAssetMaintenance> maintenanceFixedAsset { get; set; }
        public List<detailSavedFixedAssetPerson> fixAssetPersonDetail { get; set; }

        public class fixedAssetPerson
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int id { get; set; }
            public int employeeID { get; set; }
            public DateTime startDate {get;set;}
            public DateTime endDate { get; set; }
            public string remarks { get; set; }
            public int fixedAssetID { get; set; }
        }

        public class fixedAssetMaintenance
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int id { get; set; }
            public DateTime maintenanceDate { get; set; }
            public string remarks { get; set; }
            public int amount { get; set; }
            public int fixedAssetID { get; set; }
        }

        public class detailSavedFixedAssetPerson
        {
            [Key]
            public DateTime startDate {get;set;}
            public DateTime endDate{get;set;}
            public int employeeID{get;set;}
            public string employeeName {get;set;}
            public string deptName{get;set;}
            public string remarks { get; set; }
        }
    }
}