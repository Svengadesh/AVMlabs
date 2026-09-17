using AVMLabLMS.Data;
using AVMLabLMS.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AVMLabLMS.Services
{
    public class ReportService
    {
        private readonly AppDbContext _context;
        private readonly ClientService _clientService;

        public ReportService(AppDbContext context, ClientService clientService)
        {
            _context = context;
            _clientService = clientService;
        }

        public async Task<List<DailySummaryDTO>> GetDailySummaryAsync(DateTime from, DateTime to)
        {
            // Daily work order summary for last 30 days including zero-WO days
            var dates = Enumerable.Range(0, 1 + to.Subtract(from).Days)
                                  .Select(offset => from.AddDays(offset).Date)
                                  .ToList();

            var wos = await _context.WorkOrders
                .Include(w => w.Items)
                .Where(w => w.WODate.Date >= from.Date && w.WODate.Date <= to.Date)
                .GroupBy(w => w.WODate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Revenue = g.Sum(w => w.TotalAmount),
                    Tests = g.SelectMany(w => w.Items).Sum(i => i.Quantity)
                })
                .ToDictionaryAsync(k => k.Date, v => v);

            var summary = new List<DailySummaryDTO>();
            foreach (var date in dates)
            {
                summary.Add(new DailySummaryDTO
                {
                    Date = date,
                    TotalWorkOrders = wos.ContainsKey(date) ? wos[date].Count : 0,
                    TotalTests = wos.ContainsKey(date) ? wos[date].Tests : 0,
                    TotalRevenue = wos.ContainsKey(date) ? wos[date].Revenue : 0
                });
            }

            return summary.OrderByDescending(s => s.Date).ToList();
        }

        public async Task<DashboardReportDTO> GetDashboardReportAsync()
        {
            var report = new DashboardReportDTO();
            var clients = await _context.Clients.ToListAsync();
            
            var clientBalances = new List<OutstandingClientDTO>();
            decimal totalOutstanding = 0;

            foreach (var client in clients)
            {
                var outstanding = await _clientService.CalculateOutstandingBalance(client.ClientId);
                totalOutstanding += outstanding;

                clientBalances.Add(new OutstandingClientDTO
                {
                    ClientId = client.ClientId,
                    ClientName = client.ClientName,
                    CreditLimit = client.CreditLimit,
                    OutstandingBalance = outstanding
                });
            }

            report.TotalOutstanding = totalOutstanding;
            
            report.TopOutstandingClients = clientBalances
                .OrderByDescending(c => c.OutstandingBalance)
                .Take(5)
                .ToList();

            report.ClientsAboveLimit = clientBalances
                .Where(c => c.IsOverLimit)
                .OrderByDescending(c => c.OutstandingBalance)
                .ToList();

            report.DailySummaries = await GetDailySummaryAsync(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);

            return report;
        }

        public async Task<decimal> GetTotalGatewayFeesAsync(DateTime? from, DateTime? to)
        {
            var query = _context.Payments.AsQueryable();
            if (from.HasValue) query = query.Where(p => p.PaymentDate >= from.Value);
            if (to.HasValue) query = query.Where(p => p.PaymentDate <= to.Value);
            
            return await query.SumAsync(p => p.GatewayFee);
        }
    }
}
