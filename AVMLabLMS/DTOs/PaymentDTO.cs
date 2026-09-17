using System.ComponentModel.DataAnnotations;

namespace AVMLabLMS.DTOs
{
    public class RecordPaymentDTO
    {
        [Required]
        public int ClientId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public string Mode { get; set; } // Cash, Bank Transfer, Online
    }

    public class PaymentResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal GatewayFee { get; set; }
        public decimal NetAmount { get; set; }
    }
}
