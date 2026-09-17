namespace AVMLabLMS.DTOs
{
    public class DailySummaryDTO
    {
        public DateTime Date { get; set; }
        public int TotalWorkOrders { get; set; }
        public int TotalTests { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class OutstandingClientDTO
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal OutstandingBalance { get; set; }
        public bool IsOverLimit => OutstandingBalance > CreditLimit;
    }

    public class DashboardReportDTO
    {
        public List<DailySummaryDTO> DailySummaries { get; set; } = new List<DailySummaryDTO>();
        public List<OutstandingClientDTO> TopOutstandingClients { get; set; } = new List<OutstandingClientDTO>();
        public List<OutstandingClientDTO> ClientsAboveLimit { get; set; } = new List<OutstandingClientDTO>();
        public decimal TotalOutstanding { get; set; }
    }
}
