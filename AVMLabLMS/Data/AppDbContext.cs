using AVMLabLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace AVMLabLMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<WorkOrderItem> WorkOrderItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimals
            modelBuilder.Entity<Client>()
                .Property(c => c.CreditLimit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Test>()
                .Property(t => t.Rate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<WorkOrder>()
                .Property(w => w.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<WorkOrderItem>()
                .Property(wi => wi.Rate)
                .HasPrecision(18, 2);

            modelBuilder.Entity<WorkOrderItem>()
                .Property(wi => wi.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.GatewayFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.NetAmount)
                .HasPrecision(18, 2);
        }
    }
}
