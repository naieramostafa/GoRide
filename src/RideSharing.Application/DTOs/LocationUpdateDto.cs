namespace RideSharing.Application.DTOs;

public class LocationUpdateDto
{
    public string RideId { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
}

public class StatusUpdateDto
{
    public string RideId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
