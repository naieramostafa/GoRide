using RideSharing.Core.Enums;
using RideSharing.Core.Exceptions;
using RideSharing.Core.ValueObjects;

namespace RideSharing.Core.Entities;

public class Ride
{
    public Guid Id { get; private set; }
    public Guid PassengerId { get; private set; }
    public Passenger Passenger { get; private set; } = null!;
    public Guid? DriverId { get; private set; }
    public Driver? Driver { get; private set; }
    public Location PickupLocation { get; private set; } = null!;
    public Location DropoffLocation { get; private set; } = null!;
    public RideStatus Status { get; private set; }
    public Money Fare { get; private set; } = Money.Zero;
    public Money? FinalFare { get; private set; }
    public double? DistanceKm { get; private set; }
    public int? DurationMinutes { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    private Ride() { }

    public Ride(Passenger passenger, Location pickup, Location dropoff, Money fare, double? distanceKm = null, int? durationMinutes = null)
    {
        Id = Guid.NewGuid();
        PassengerId = passenger.Id;
        Passenger = passenger;
        PickupLocation = pickup;
        DropoffLocation = dropoff;
        Fare = fare;
        Status = RideStatus.Pending;
        RequestedAt = DateTime.UtcNow;
        DistanceKm = distanceKm;
        DurationMinutes = durationMinutes;
    }

    public void MarkSearching()
    {
        if (Status != RideStatus.Pending)
            throw new RideStateException("Only pending rides can be marked as searching");
        Status = RideStatus.Searching;
    }

    public void AssignDriver(Driver driver)
    {
        if (driver == null)
            throw new ArgumentNullException(nameof(driver));
        DriverId = driver.Id;
        Driver = driver;
        Status = RideStatus.DriverAccepted;
        AcceptedAt = DateTime.UtcNow;
    }

    public void StartRide()
    {
        if (Status != RideStatus.DriverAccepted)
            throw new RideStateException("Ride must be accepted before starting");
        Status = RideStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    public void CompleteRide(double distanceKm, int durationMinutes, Money finalFare)
    {
        if (Status != RideStatus.InProgress)
            throw new RideStateException("Ride must be in progress to complete");
        Status = RideStatus.Completed;
        DistanceKm = distanceKm;
        DurationMinutes = durationMinutes;
        FinalFare = finalFare;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel(string? reason = null)
    {
        if (Status is RideStatus.Completed or RideStatus.Cancelled)
            throw new RideStateException("Ride already ended");
        Status = RideStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
    }

    public void SetDriverArriving()
    {
        if (Status != RideStatus.DriverAccepted)
            throw new RideStateException("Driver must be accepted first");
        Status = RideStatus.DriverArriving;
    }
}
