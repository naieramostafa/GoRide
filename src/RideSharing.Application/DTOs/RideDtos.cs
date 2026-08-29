using RideSharing.Core.Enums;
using RideSharing.Core.ValueObjects;

namespace RideSharing.Application.DTOs;

public class RideRequestDto
{
    public Guid UserId { get; set; }
    public double PickupLatitude { get; set; }
    public double PickupLongitude { get; set; }
    public string? PickupAddress { get; set; }
    public double DropoffLatitude { get; set; }
    public double DropoffLongitude { get; set; }
    public string? DropoffAddress { get; set; }
}

public class RideResponseDto
{
    public Guid Id { get; set; }
    public Guid PassengerId { get; set; }
    public string? PassengerName { get; set; }
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public Location? PickupLocation { get; set; }
    public Location? DropoffLocation { get; set; }
    public RideStatus Status { get; set; }
    public decimal Fare { get; set; }
    public decimal? FinalFare { get; set; }
    public double? DistanceKm { get; set; }
    public int? DurationMinutes { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class RideAcceptDto
{
    public Guid RideId { get; set; }
    public Guid DriverId { get; set; }
}

public class RideLocationUpdateDto
{
    public Guid RideId { get; set; }
    public Guid DriverId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class RideCompleteDto
{
    public Guid RideId { get; set; }
    public Guid DriverId { get; set; }
    public double DistanceKm { get; set; }
    public int DurationMinutes { get; set; }
    public decimal FinalFare { get; set; }
}

public class PriceEstimateDto
{
    public double DistanceKm { get; set; }
    public int DurationMinutes { get; set; }
    public decimal EstimatedFare { get; set; }
    public string Currency { get; set; } = "USD";
}
