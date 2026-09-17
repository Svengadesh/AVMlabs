using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVMLabLMS.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }

        public int InvoiceId { get; set; }
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; }

        public DateTime PaymentDate { get; set; }

        public decimal Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Mode { get; set; } // Cash, Bank Transfer, Online

        public decimal GatewayFee { get; set; }

        public decimal NetAmount { get; set; }
    }
}
