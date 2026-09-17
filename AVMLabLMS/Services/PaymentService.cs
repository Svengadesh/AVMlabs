using AVMLabLMS.Data;
using AVMLabLMS.DTOs;
using AVMLabLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace AVMLabLMS.Services
{
    public class PaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentResponseDTO> RecordPaymentAsync(RecordPaymentDTO dto)
        {
            var unpaidInvoices = await _context.Invoices
                .Where(i => i.ClientId == dto.ClientId && i.Status != "Paid")
                .OrderBy(i => i.InvoiceDate)
                .ToListAsync();

            if (!unpaidInvoices.Any())
                return new PaymentResponseDTO { Success = false, Message = "No unpaid invoices found." };

            decimal gatewayFee = 0;
            if (dto.Mode.Equals("Online", StringComparison.OrdinalIgnoreCase))
            {
                // Gateway fee calculation (2%)
                gatewayFee = dto.Amount * 0.02m;
            }

            decimal remainingAmountToApply = dto.Amount; // apply full payment amount to the invoice balance

            bool feeApplied = false;

            foreach (var invoice in unpaidInvoices)
            {
                if (remainingAmountToApply <= 0)
                    break;

                var paymentsForInvoice = await _context.Payments.Where(p => p.InvoiceId == invoice.InvoiceId).SumAsync(p => p.Amount);
                var invoiceBalance = invoice.TotalAmount - paymentsForInvoice;

                if (invoiceBalance > 0)
                {
                    decimal amountToApply = Math.Min(invoiceBalance, remainingAmountToApply);
                    
                    decimal feeForThisRecord = !feeApplied ? gatewayFee : 0;
                    if (feeForThisRecord > 0) feeApplied = true;

                    var payment = new Payment
                    {
                        InvoiceId = invoice.InvoiceId,
                        PaymentDate = DateTime.UtcNow,
                        Amount = amountToApply,
                        Mode = dto.Mode,
                        GatewayFee = feeForThisRecord,
                        NetAmount = amountToApply - feeForThisRecord
                    };

                    _context.Payments.Add(payment);
                    remainingAmountToApply -= amountToApply;

                    if (amountToApply == invoiceBalance)
                    {
                        invoice.Status = "Paid";
                    }
                    else
                    {
                        invoice.Status = "Partial";
                    }
                }
            }

            await _context.SaveChangesAsync();

            return new PaymentResponseDTO
            {
                Success = true,
                Message = "Payment recorded successfully.",
                GatewayFee = gatewayFee,
                NetAmount = dto.Amount - gatewayFee
            };
        }
    }
}
