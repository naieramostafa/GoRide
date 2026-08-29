using Microsoft.EntityFrameworkCore;
using RideSharing.Core.Entities;

namespace RideSharing.Infrastructure.Persistence;

public class RideSharingDbContext : DbContext
{
    public RideSharingDbContext(DbContextOptions<RideSharingDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Passenger> Passengers => Set<Passenger>();
    public DbSet<Ride> Rides => Set<Ride>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            e.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Phone).HasMaxLength(20);
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.PasswordHash).HasMaxLength(200);
        });

        modelBuilder.Entity<Driver>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.Property(x => x.LicenseNumber).IsRequired().HasMaxLength(50);
            e.Property(x => x.Rating).HasPrecision(3, 2);
            e.OwnsOne(x => x.CurrentLocation, loc =>
            {
                loc.Property(l => l.Latitude).HasColumnName("Latitude");
                loc.Property(l => l.Longitude).HasColumnName("Longitude");
                loc.Property(l => l.Address).HasColumnName("Address").HasMaxLength(500);
            });
        });

        modelBuilder.Entity<Passenger>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.Property(x => x.Rating).HasPrecision(3, 2);
        });

        modelBuilder.Entity<Vehicle>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Make).IsRequired().HasMaxLength(50);
            e.Property(x => x.Model).IsRequired().HasMaxLength(50);
            e.Property(x => x.LicensePlate).IsRequired().HasMaxLength(20);
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Ride>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Passenger).WithMany().HasForeignKey(x => x.PassengerId);
            e.HasOne(x => x.Driver).WithMany().HasForeignKey(x => x.DriverId).IsRequired(false);
            e.OwnsOne(x => x.PickupLocation, loc =>
            {
                loc.Property(l => l.Latitude).HasColumnName("PickupLatitude");
                loc.Property(l => l.Longitude).HasColumnName("PickupLongitude");
                loc.Property(l => l.Address).HasColumnName("PickupAddress").HasMaxLength(500);
            });
            e.OwnsOne(x => x.DropoffLocation, loc =>
            {
                loc.Property(l => l.Latitude).HasColumnName("DropoffLatitude");
                loc.Property(l => l.Longitude).HasColumnName("DropoffLongitude");
                loc.Property(l => l.Address).HasColumnName("DropoffAddress").HasMaxLength(500);
            });
            e.OwnsOne(x => x.Fare, f => f.Property(m => m.Amount).HasColumnName("FareAmount").HasPrecision(18, 2));
            e.OwnsOne(x => x.FinalFare, f => f.Property(m => m.Amount).HasColumnName("FinalFareAmount").HasPrecision(18, 2));
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Ride).WithMany().HasForeignKey(x => x.RideId);
            e.OwnsOne(x => x.Amount, a => a.Property(m => m.Amount).HasColumnName("Amount").HasPrecision(18, 2));
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.StripePaymentIntentId).HasMaxLength(200);
            e.Property(x => x.StripeChargeId).HasMaxLength(200);
        });

        modelBuilder.Entity<TaskItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.AssignedTo).WithMany().HasForeignKey(x => x.AssignedToUserId).IsRequired(false);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Rating>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Ride).WithMany().HasForeignKey(x => x.RideId);
            e.Property(x => x.Comment).HasMaxLength(500);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Token).IsUnique();
            e.Property(x => x.Token).IsRequired().HasMaxLength(500);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });
    }
}
