namespace RideSharing.Application.DTOs;

public class RideCancelDto
{
    public Guid RideId { get; set; }
    public string? Reason { get; set; }
}
