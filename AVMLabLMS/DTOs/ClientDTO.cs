using System.ComponentModel.DataAnnotations;

namespace AVMLabLMS.DTOs
{
    public class ClientDTO
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal OutstandingBalance { get; set; }
        public string NblStatus { get; set; } // "NBL" or "Non-NBL"
        public bool IsActive { get; set; }
    }

    public class ClientLedgerDTO
    {
        public string ClientName { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal TotalOutstanding { get; set; }
        public string NblStatus { get; set; }
        public List<LedgerRecordDTO> Records { get; set; } = new List<LedgerRecordDTO>();
        public decimal TotalInvoiced { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalGatewayFees { get; set; }
    }

    public class LedgerRecordDTO
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal RunningBalance { get; set; }
    }

    public class CreateClientDTO
    {
        [Required]
        public string ClientName { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Credit Limit must be greater than 0.")]
        public decimal CreditLimit { get; set; }
    }
}
