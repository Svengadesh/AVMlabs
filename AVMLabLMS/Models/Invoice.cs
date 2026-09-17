using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVMLabLMS.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        public int ClientId { get; set; }
        [ForeignKey("ClientId")]
        public Client Client { get; set; }

        public DateTime InvoiceDate { get; set; }
        
        public DateTime DueDate { get; set; }

        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } // e.g., Unpaid, Partial, Paid

        public ICollection<Payment> Payments { get; set; }
    }
}
