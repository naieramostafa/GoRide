namespace RideSharing.Application.DTOs;

public class CreateRatingDto
{
    public Guid RideId { get; set; }
    public Guid RatedByUserId { get; set; }
    public Guid RatedUserId { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
}

public class RatingResponseDto
{
    public Guid Id { get; set; }
    public Guid RideId { get; set; }
    public Guid RatedByUserId { get; set; }
    public Guid RatedUserId { get; set; }
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
