using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class purchaseReceive
    {
        public purchaseReceive()
        {
            this.detailPurchaseReceive = new List<purchaseReceiveDetail>();
            this.detailPurchaseReturn = new List<purchaseReturnDetail>();
            this.detailPoReceive = new List<poReceive>();
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("Receive No.")]
        public string receiveNo { get; set; }

        [DisplayName("Receive Date")]
        public DateTime receiveDate { get; set; }

        [DisplayName("PO No.")]
        public string poId { get; set; }

        [DisplayName("Remarks")]
        public string remarks { get; set; }

        [DisplayName("Delivery Order")] //surat jalan
        public string deliveryOrder { get; set; }

        [DisplayName("Proposed By")]
        public string proposedBy { get; set; }
        [DisplayName("Proposed Date")]
        public DateTime proposedDate { get; set; }
        public bool proposedStatus { get; set; }

        public string receivedBy { get; set; }
        public DateTime? receivedDate { get; set; }
        [DisplayName("Received Status")]
        public bool? receivedStatus { get; set; }

        public string createdUser { get; set; }
        public DateTime createdDate { get; set; }
        public string modifiedUser { get; set; }
        public DateTime? modifiedDate { get; set; }

        public List<purchaseReceiveDetail> detailPurchaseReceive { get; set; }
        public List<purchaseReturnDetail> detailPurchaseReturn { get; set; }
        public List<poReceive> detailPoReceive { get; set; }

        //public class receiveDetail
        //{
        //    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        //    public string receiveNo { get; set; }
        //    public string productId { get; set; }
        //    public int qty { get; set; }
        //    public string remarks { get; set; }
            
        //}
        public class purchaseReceiveDetail
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 0)]
            public string receiveNo { get; set; }

            [Key]
            [Column(Order = 1)]
            public int productId { get; set; }
            public int qty { get; set; }
            public string remarks { get; set; }
        }

        public class purchaseReturnDetail
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            [Column(Order = 0)]
            public string receiveNo { get; set; }

            [Key]
            [Column(Order = 1)]
            public int productId { get; set; }
            public int qty { get; set; }
            public string remarks { get; set; }
        }

        public class poReceive
        {
            [Key]
            public int productId { get; set; }
            public string productDesc { get; set; }
            public int qtyOrder { get; set; }
            public string unit { get; set; }
        }
    }
}