using AVMLabLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace AVMLabLMS.Data
{
    public static class DbSeeder
    {
        public static void SeedData(AppDbContext context)
        {
            context.Database.Migrate();

            if (!context.Clients.Any())
            {
                var clients = new List<Client>
                {
                    new Client { ClientName = "Acme Corp", City = "New York", Country = "USA", CreditLimit = 5000, IsActive = true },
                    new Client { ClientName = "Global Tech", City = "London", Country = "UK", CreditLimit = 10000, IsActive = true },
                    new Client { ClientName = "Local Med", City = "Sydney", Country = "Australia", CreditLimit = 2000, IsActive = true }
                };
                context.Clients.AddRange(clients);
                context.SaveChanges();
            }

            if (!context.Tests.Any())
            {
                var tests = new List<Test>
                {
                    new Test { TestCode = "T01", TestName = "Blood Sugar", SampleType = "Blood", Rate = 50, TATHours = 24, IsActive = true },
                    new Test { TestCode = "T02", TestName = "Lipid Profile", SampleType = "Blood", Rate = 150, TATHours = 48, IsActive = true },
                    new Test { TestCode = "T03", TestName = "CBC", SampleType = "Blood", Rate = 80, TATHours = 12, IsActive = true },
                    new Test { TestCode = "T04", TestName = "Urine Routine", SampleType = "Urine", Rate = 40, TATHours = 24, IsActive = true },
                    new Test { TestCode = "T05", TestName = "Vitamin D", SampleType = "Blood", Rate = 200, TATHours = 48, IsActive = true },
                    new Test { TestCode = "T06", TestName = "Thyroid Profile", SampleType = "Blood", Rate = 120, TATHours = 24, IsActive = true },
                    new Test { TestCode = "T07", TestName = "LFT", SampleType = "Blood", Rate = 180, TATHours = 24, IsActive = true },
                    new Test { TestCode = "T08", TestName = "KFT", SampleType = "Blood", Rate = 160, TATHours = 24, IsActive = true },
                    new Test { TestCode = "T09", TestName = "HbA1c", SampleType = "Blood", Rate = 90, TATHours = 24, IsActive = true },
                    new Test { TestCode = "T10", TestName = "CRP", SampleType = "Blood", Rate = 110, TATHours = 24, IsActive = true }
                };
                context.Tests.AddRange(tests);
                context.SaveChanges();
            }

            if (!context.WorkOrders.Any())
            {
                var acme = context.Clients.First(c => c.ClientName == "Acme Corp");
                var globalTech = context.Clients.First(c => c.ClientName == "Global Tech");
                var test1 = context.Tests.First(t => t.TestCode == "T01");
                var test2 = context.Tests.First(t => t.TestCode == "T02");

                // In-transit Work Order for Acme (creates outstanding balance)
                var wo1 = new WorkOrder
                {
                    ClientId = acme.ClientId,
                    WODate = DateTime.UtcNow.AddDays(-1),
                    Status = "Processing",
                    TotalAmount = test1.Rate * 2,
                    CreatedBy = "System",
                    Items = new List<WorkOrderItem>
                    {
                        new WorkOrderItem { TestId = test1.TestId, Quantity = 2, Rate = test1.Rate, Amount = test1.Rate * 2, SampleStatus = "Received" }
                    }
                };

                // Billed Work Order for Global Tech
                var wo2 = new WorkOrder
                {
                    ClientId = globalTech.ClientId,
                    WODate = DateTime.UtcNow.AddDays(-5),
                    Status = "Billed",
                    TotalAmount = test2.Rate * 1,
                    CreatedBy = "System",
                    Items = new List<WorkOrderItem>
                    {
                        new WorkOrderItem { TestId = test2.TestId, Quantity = 1, Rate = test2.Rate, Amount = test2.Rate * 1, SampleStatus = "Tested" }
                    }
                };

                context.WorkOrders.AddRange(wo1, wo2);
                context.SaveChanges();

                // Create Invoice for Billed Work Order (Global Tech)
                var invoice1 = new Invoice
                {
                    ClientId = globalTech.ClientId,
                    InvoiceDate = DateTime.UtcNow.AddDays(-4),
                    DueDate = DateTime.UtcNow.AddDays(10),
                    TotalAmount = wo2.TotalAmount,
                    Status = "Unpaid"
                };
                context.Invoices.Add(invoice1);
                context.SaveChanges();
            }
        }
    }
}
