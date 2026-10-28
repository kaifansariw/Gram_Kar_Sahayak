using Microsoft.EntityFrameworkCore;
using GramKar.Models.Entities;

namespace GramKar.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Household> Households => Set<Household>();
        public DbSet<TaxType> TaxTypes => Set<TaxType>();
        public DbSet<Demand> Demands => Set<Demand>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
        public DbSet<PanchayatSetting> PanchayatSettings => Set<PanchayatSetting>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Household constraints
            modelBuilder.Entity<Household>()
                .HasIndex(h => h.HouseNo)
                .IsUnique();

            // Demand unique composite constraint: HouseId + TaxTypeId + FinancialYear
            modelBuilder.Entity<Demand>()
                .HasIndex(d => new { d.HouseId, d.TaxTypeId, d.FinancialYear })
                .IsUnique();

            // Payment unique receipt number
            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.ReceiptNo)
                .IsUnique();

            // Relationships and delete behaviors
            modelBuilder.Entity<Demand>()
                .HasOne(d => d.Household)
                .WithMany(h => h.Demands)
                .HasForeignKey(d => d.HouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Demand>()
                .HasOne(d => d.TaxType)
                .WithMany(t => t.Demands)
                .HasForeignKey(d => d.TaxTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Household)
                .WithMany(h => h.Payments)
                .HasForeignKey(p => p.HouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaymentAllocation>()
                .HasOne(pa => pa.Payment)
                .WithMany(p => p.PaymentAllocations)
                .HasForeignKey(pa => pa.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentAllocation>()
                .HasOne(pa => pa.Demand)
                .WithMany(d => d.PaymentAllocations)
                .HasForeignKey(pa => pa.DemandId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
