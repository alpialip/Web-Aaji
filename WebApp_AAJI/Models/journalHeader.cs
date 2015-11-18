using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApp_AAJI.Models
{
    public class journalHeader
    {
        public journalHeader()
        {
            this.detailJournal = new List<journalDetail>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long id { get; set; }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(13)]
        [DisplayName("Voucher No.")]
        public string voucherNo { get; set; }

        [Required]
        [DisplayName("Voucher Date")]
        public DateTime voucherDate { get; set; }

        [Required]
        public string journalType { get; set; }

        [StringLength(250)]
        [DisplayName("Remark")]
        public string remark { get; set; }

        public DateTime createdDate { get; set; }
        public string createdUser { get; set; }
        public DateTime? modifiedDate { get; set; }
        public string modifiedUser { get; set; }

        public List<journalDetail> detailJournal { get; set; }

        public class journalDetail
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long sId { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            [StringLength(13)]
            public string voucherNo { get; set; }

            public int coaId { get; set; }

            [StringLength(100)]
            public string accountNameOther { get; set; }
            public string remarks { get; set; }
            public decimal debit { get; set; }
            public decimal credit { get; set; }

        }
    }
}