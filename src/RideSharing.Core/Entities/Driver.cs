using RideSharing.Core.Enums;
using RideSharing.Core.ValueObjects;

namespace RideSharing.Core.Entities;

public class Driver
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string LicenseNumber { get; private set; } = string.Empty;
    public bool IsAvailable { get; private set; } = true;
    public bool IsVerified { get; private set; }
    public double Rating { get; private set; } = 5.0;
    public int TotalRides { get; private set; }
    public Location? CurrentLocation { get; private set; }
    public Vehicle? Vehicle { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Driver() { }

    public Driver(User user, string licenseNumber, Vehicle vehicle)
    {
        Id = Guid.NewGuid();
        UserId = user.Id;
        User = user;
        LicenseNumber = licenseNumber;
        Vehicle = vehicle;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetAvailable(bool available) => IsAvailable = available;
    public void UpdateLocation(Location location) => CurrentLocation = location;
    public void Verify() => IsVerified = true;

    public void UpdateRating(double newRating)
    {
        Rating = ((Rating * TotalRides) + newRating) / (TotalRides + 1);
        TotalRides++;
    }
}
