using AVMLabLMS.Data;
using AVMLabLMS.DTOs;
using AVMLabLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace AVMLabLMS.Services
{
    public class WorkOrderService
    {
        private readonly AppDbContext _context;
        private readonly ClientService _clientService;

        public WorkOrderService(AppDbContext context, ClientService clientService)
        {
            _context = context;
            _clientService = clientService;
        }

        public async Task<WorkOrderResponseDTO> CreateWorkOrderAsync(CreateWorkOrderDTO dto)
        {
            var client = await _context.Clients.FindAsync(dto.ClientId);
            if (client == null || !client.IsActive)
                throw new Exception("Client not found or is inactive.");

            var nblStatus = await _clientService.GetNblStatusAsync(client.ClientId);
            if (nblStatus != "NBL")
                throw new Exception("Work order can only be created for NBL clients.");

            if (dto.Items == null || !dto.Items.Any())
                throw new Exception("Work order must have at least one item.");

            var workOrder = new WorkOrder
            {
                ClientId = client.ClientId,
                WODate = DateTime.UtcNow,
                Status = "Pending",
                CreatedBy = "System User", // Simulated
                Items = new List<WorkOrderItem>()
            };

            decimal totalAmount = 0;

            foreach (var itemDto in dto.Items)
            {
                var test = await _context.Tests.FindAsync(itemDto.TestId);
                if (test == null || !test.IsActive)
                    throw new Exception($"Test with ID {itemDto.TestId} is invalid.");

                decimal amount = itemDto.Quantity * test.Rate;
                totalAmount += amount;

                workOrder.Items.Add(new WorkOrderItem
                {
                    TestId = test.TestId,
                    Quantity = itemDto.Quantity,
                    Rate = test.Rate,
                    Amount = amount,
                    SampleStatus = "Received"
                });
            }

            workOrder.TotalAmount = totalAmount;

            _context.WorkOrders.Add(workOrder);
            await _context.SaveChangesAsync();

            return new WorkOrderResponseDTO
            {
                WOId = workOrder.WOId,
                ClientId = workOrder.ClientId,
                ClientName = client.ClientName,
                WODate = workOrder.WODate,
                Status = workOrder.Status,
                TotalAmount = workOrder.TotalAmount
            };
        }

        public async Task<bool> UpdateStatusAsync(int woId, string newStatus)
        {
            var validStatuses = new[] { "Pending", "Processing", "Reported", "Billed" };
            if (!validStatuses.Contains(newStatus))
                return false;

            var workOrder = await _context.WorkOrders.FindAsync(woId);
            if (workOrder == null)
                return false;

            // Simple state machine logic can be added here if strict transition is needed
            // e.g. Pending -> Processing -> Reported -> Billed

            workOrder.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<WorkOrderResponseDTO>> GetInTransitWorkOrdersAsync()
        {
            return await _context.WorkOrders
                .Include(w => w.Client)
                .Where(w => w.Status != "Billed" && w.Status != "Reported")
                .Select(w => new WorkOrderResponseDTO
                {
                    WOId = w.WOId,
                    ClientId = w.ClientId,
                    ClientName = w.Client.ClientName,
                    WODate = w.WODate,
                    Status = w.Status,
                    TotalAmount = w.TotalAmount
                })
                .ToListAsync();
        }
    }
}
