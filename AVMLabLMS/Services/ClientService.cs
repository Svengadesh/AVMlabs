using AVMLabLMS.Data;
using AVMLabLMS.DTOs;
using AVMLabLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace AVMLabLMS.Services
{
    public class ClientService
    {
        private readonly AppDbContext _context;

        public ClientService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ClientDTO>> GetClientsAsync(string search, string country, int page = 1, int pageSize = 10)
        {
            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.ClientName.Contains(search));
            }

            if (!string.IsNullOrEmpty(country))
            {
                query = query.Where(c => c.Country == country);
            }

            var clients = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = new List<ClientDTO>();
            foreach (var client in clients)
            {
                var outstanding = await CalculateOutstandingBalance(client.ClientId);
                dtos.Add(new ClientDTO
                {
                    ClientId = client.ClientId,
                    ClientName = client.ClientName,
                    City = client.City,
                    Country = client.Country,
                    CreditLimit = client.CreditLimit,
                    IsActive = client.IsActive,
                    OutstandingBalance = outstanding,
                    NblStatus = outstanding == 0 ? "NBL" : "Non-NBL"
                });
            }

            return dtos;
        }

        public async Task<decimal> CalculateOutstandingBalance(int clientId)
        {
            // Pending Invoice Amount + In-Transit Work Order Amount
            var pendingInvoices = await _context.Invoices
                .Where(i => i.ClientId == clientId && i.Status != "Paid")
                .SumAsync(i => i.TotalAmount);

            var totalPaymentsForPendingInvoices = await _context.Payments
                .Where(p => p.Invoice.ClientId == clientId && p.Invoice.Status != "Paid")
                .SumAsync(p => p.Amount);

            var netPendingInvoices = pendingInvoices - totalPaymentsForPendingInvoices;

            var inTransitWorkOrders = await _context.WorkOrders
                .Where(w => w.ClientId == clientId && w.Status != "Billed" && w.Status != "Reported")
                .SumAsync(w => w.TotalAmount);

            return netPendingInvoices + inTransitWorkOrders;
        }

        public async Task<string> GetNblStatusAsync(int clientId)
        {
            var outstanding = await CalculateOutstandingBalance(clientId);
            return outstanding == 0 ? "NBL" : "Non-NBL";
        }

        public async Task<ClientLedgerDTO> GetClientLedgerAsync(int clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null) return null;

            var outstanding = await CalculateOutstandingBalance(clientId);
            
            var ledger = new ClientLedgerDTO
            {
                ClientName = client.ClientName,
                CreditLimit = client.CreditLimit,
                TotalOutstanding = outstanding,
                NblStatus = outstanding == 0 ? "NBL" : "Non-NBL"
            };

            // Get Invoices (Debits)
            var invoices = await _context.Invoices
                .Where(i => i.ClientId == clientId)
                .Select(i => new LedgerRecordDTO
                {
                    Date = i.InvoiceDate,
                    Description = $"Invoice #{i.InvoiceId}",
                    Debit = i.TotalAmount,
                    Credit = 0
                }).ToListAsync();

            // Get Payments (Credits) and Gateway Fees (Debits)
            var payments = await _context.Payments
                .Include(p => p.Invoice)
                .Where(p => p.Invoice.ClientId == clientId)
                .ToListAsync();

            var paymentRecords = new List<LedgerRecordDTO>();
            foreach (var payment in payments)
            {
                paymentRecords.Add(new LedgerRecordDTO
                {
                    Date = payment.PaymentDate,
                    Description = $"Payment #{payment.PaymentId} ({payment.Mode})",
                    Debit = 0,
                    Credit = payment.Amount
                });

                if (payment.GatewayFee > 0)
                {
                    paymentRecords.Add(new LedgerRecordDTO
                    {
                        Date = payment.PaymentDate,
                        Description = $"Gateway Fee for Payment #{payment.PaymentId}",
                        Debit = payment.GatewayFee,
                        Credit = 0
                    });
                }
            }

            ledger.Records.AddRange(invoices);
            ledger.Records.AddRange(paymentRecords);

            ledger.Records = ledger.Records.OrderBy(r => r.Date).ToList();

            decimal runningBalance = 0;
            foreach (var record in ledger.Records)
            {
                runningBalance += record.Debit - record.Credit;
                record.RunningBalance = runningBalance;
            }

            ledger.TotalInvoiced = invoices.Sum(i => i.Debit);
            ledger.TotalPaid = paymentRecords.Where(p => p.Credit > 0).Sum(p => p.Credit);
            ledger.TotalGatewayFees = paymentRecords.Where(p => p.Debit > 0).Sum(p => p.Debit);

            return ledger;
        }

        public async Task<bool> ToggleStatusAsync(int clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null) return false;

            client.IsActive = !client.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
