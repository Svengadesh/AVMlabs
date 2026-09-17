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
                    new Client { ClientName = "Acme Corp", ContactPerson = "John Doe", Phone = "123-456-7890", Email = "john@acme.com", City = "New York", Country = "USA", CreditLimit = 5000, IsActive = true },
                    new Client { ClientName = "Global Tech", ContactPerson = "Jane Smith", Phone = "234-567-8901", Email = "jane@globaltech.com", City = "London", Country = "UK", CreditLimit = 10000, IsActive = true },
                    new Client { ClientName = "Local Med", ContactPerson = "Dr. Brown", Phone = "345-678-9012", Email = "brown@localmed.com", City = "Sydney", Country = "Australia", CreditLimit = 2000, IsActive = true }
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

            if (context.WorkOrders.Count() < 5)
            {
                var acme = context.Clients.First(c => c.ClientName == "Acme Corp");
                var globalTech = context.Clients.First(c => c.ClientName == "Global Tech");
                var test1 = context.Tests.First(t => t.TestCode == "T01");
                var test2 = context.Tests.First(t => t.TestCode == "T02");

                if (!context.WorkOrders.Any(w => w.TotalAmount == test1.Rate * 2 && w.ClientId == acme.ClientId))
                {
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
                    context.WorkOrders.Add(wo1);
                }

                if (!context.WorkOrders.Any(w => w.TotalAmount == test2.Rate * 1 && w.Status == "Billed"))
                {
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
                    context.WorkOrders.Add(wo2);
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
                }

                // Additional Work Order 3 (Local Med - Pending)
                var localMed = context.Clients.First(c => c.ClientName == "Local Med");
                var test3 = context.Tests.First(t => t.TestCode == "T03");
                if (!context.WorkOrders.Any(w => w.ClientId == localMed.ClientId))
                {
                    var wo3 = new WorkOrder
                    {
                        ClientId = localMed.ClientId,
                        WODate = DateTime.UtcNow.AddDays(-3),
                        Status = "Pending",
                        TotalAmount = test3.Rate * 3,
                        CreatedBy = "System",
                        Items = new List<WorkOrderItem>
                        {
                            new WorkOrderItem { TestId = test3.TestId, Quantity = 3, Rate = test3.Rate, Amount = test3.Rate * 3, SampleStatus = "InTransit" }
                        }
                    };
                    context.WorkOrders.Add(wo3);
                }

                // Additional Work Order 4 (Acme Corp - Reported)
                var test4 = context.Tests.First(t => t.TestCode == "T04");
                if (!context.WorkOrders.Any(w => w.ClientId == acme.ClientId && w.Status == "Reported"))
                {
                    var wo4 = new WorkOrder
                    {
                        ClientId = acme.ClientId,
                        WODate = DateTime.UtcNow.AddDays(-3),
                        Status = "Reported",
                        TotalAmount = test4.Rate * 1,
                        CreatedBy = "System",
                        Items = new List<WorkOrderItem>
                        {
                            new WorkOrderItem { TestId = test4.TestId, Quantity = 1, Rate = test4.Rate, Amount = test4.Rate * 1, SampleStatus = "Tested" }
                        }
                    };
                    context.WorkOrders.Add(wo4);
                }

                // Additional Work Order 5 (Global Tech - Processing)
                if (!context.WorkOrders.Any(w => w.ClientId == globalTech.ClientId && w.Status == "Processing"))
                {
                    var wo5 = new WorkOrder
                    {
                        ClientId = globalTech.ClientId,
                        WODate = DateTime.UtcNow.AddDays(-1),
                        Status = "Processing",
                        TotalAmount = test1.Rate * 1,
                        CreatedBy = "System",
                        Items = new List<WorkOrderItem>
                        {
                            new WorkOrderItem { TestId = test1.TestId, Quantity = 1, Rate = test1.Rate, Amount = test1.Rate * 1, SampleStatus = "Received" }
                        }
                    };
                    context.WorkOrders.Add(wo5);
                }

                context.SaveChanges();
            }
        }
    }
}
