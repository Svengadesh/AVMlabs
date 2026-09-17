using System.ComponentModel.DataAnnotations;

namespace AVMLabLMS.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ClientName { get; set; }

        [MaxLength(100)]
        public string ContactPerson { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(100)]
        public string Country { get; set; }

        public decimal CreditLimit { get; set; }

        public bool IsActive { get; set; }

        // Navigation properties
        public ICollection<WorkOrder> WorkOrders { get; set; }
        public ICollection<Invoice> Invoices { get; set; }
    }
}
