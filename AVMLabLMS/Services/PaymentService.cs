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
                // Simple gateway fee calculation (e.g., 2%)
                gatewayFee = dto.Amount * 0.02m;
            }

            decimal netAmount = dto.Amount - gatewayFee;
            decimal remainingAmountToApply = netAmount; // or apply full amount? Business rule says record payment, gateway fee separate debit.
            
            // Wait, standard practice: Amount is credited, Gateway Fee is debited separately.
            // So the payment to invoices is the full Amount, but the net received by the lab is NetAmount.
            // Let's allocate the full Amount to invoices to clear the client's balance.

            foreach (var invoice in unpaidInvoices)
            {
                if (remainingAmountToApply <= 0)
                    break;

                var paymentsForInvoice = await _context.Payments.Where(p => p.InvoiceId == invoice.InvoiceId).SumAsync(p => p.Amount);
                var invoiceBalance = invoice.TotalAmount - paymentsForInvoice;

                if (invoiceBalance > 0)
                {
                    decimal amountToApply = Math.Min(invoiceBalance, remainingAmountToApply);

                    var payment = new Payment
                    {
                        InvoiceId = invoice.InvoiceId,
                        PaymentDate = DateTime.UtcNow,
                        Amount = amountToApply,
                        Mode = dto.Mode,
                        GatewayFee = amountToApply == dto.Amount ? gatewayFee : 0, // apply fee to first record
                        NetAmount = amountToApply - (amountToApply == dto.Amount ? gatewayFee : 0)
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
                NetAmount = netAmount
            };
        }
    }
}
