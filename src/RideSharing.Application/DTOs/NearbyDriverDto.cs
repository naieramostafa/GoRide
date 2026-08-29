namespace RideSharing.Application.DTOs;

public class NearbyDriverDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string? VehicleType { get; set; }
    public string? VehicleMake { get; set; }
    public string? VehicleModel { get; set; }
    public string? VehicleColor { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
