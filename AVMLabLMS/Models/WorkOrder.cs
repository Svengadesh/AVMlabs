using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVMLabLMS.Models
{
    public class WorkOrder
    {
        [Key]
        public int WOId { get; set; }

        public int ClientId { get; set; }
        [ForeignKey("ClientId")]
        public Client Client { get; set; }

        public DateTime WODate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } // Pending, Processing, Reported, Billed

        public decimal TotalAmount { get; set; }

        [MaxLength(50)]
        public string CreatedBy { get; set; }

        public ICollection<WorkOrderItem> Items { get; set; }
    }
}
