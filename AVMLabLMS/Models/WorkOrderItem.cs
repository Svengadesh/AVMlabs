using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVMLabLMS.Models
{
    public class WorkOrderItem
    {
        [Key]
        public int WOItemId { get; set; }

        public int WOId { get; set; }
        [ForeignKey("WOId")]
        public WorkOrder WorkOrder { get; set; }

        public int TestId { get; set; }
        [ForeignKey("TestId")]
        public Test Test { get; set; }

        public int Quantity { get; set; }

        public decimal Rate { get; set; }

        public decimal Amount { get; set; }

        [MaxLength(20)]
        public string SampleStatus { get; set; } // e.g., Received, In Transit, Tested
    }
}
