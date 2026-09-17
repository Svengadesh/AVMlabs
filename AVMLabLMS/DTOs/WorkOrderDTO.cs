using System.ComponentModel.DataAnnotations;

namespace AVMLabLMS.DTOs
{
    public class CreateWorkOrderDTO
    {
        [Required]
        public int ClientId { get; set; }

        [Required]
        public List<WorkOrderItemDTO> Items { get; set; } = new List<WorkOrderItemDTO>();
    }

    public class WorkOrderItemDTO
    {
        [Required]
        public int TestId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; }
    }

    public class WorkOrderResponseDTO
    {
        public int WOId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public DateTime WODate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
