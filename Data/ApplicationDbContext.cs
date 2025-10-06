using Microsoft.EntityFrameworkCore;
using CarDealershipAPI.Models;

namespace CarDealershipAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Purchase> Purchases { get; set; }
    public DbSet<OtpCode> OtpCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Purchase>()
            .HasOne(p => p.User)
            .WithMany(u => u.Purchases)
            .HasForeignKey(p => p.UserId);

        modelBuilder.Entity<Purchase>()
            .HasOne(p => p.Vehicle)
            .WithMany(v => v.Purchases)
            .HasForeignKey(p => p.VehicleId);

        // Configure decimal precision
        modelBuilder.Entity<Vehicle>()
            .Property(v => v.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Purchase>()
            .Property(p => p.PurchasePrice)
            .HasPrecision(18, 2);
    }
}
