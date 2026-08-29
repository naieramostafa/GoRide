namespace RideSharing.Core.Entities;

public class Rating
{
    public Guid Id { get; private set; }
    public Guid RideId { get; private set; }
    public Ride Ride { get; private set; } = null!;
    public Guid RatedByUserId { get; private set; }
    public Guid RatedUserId { get; private set; }
    public int Score { get; private set; }
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Rating() { }

    public Rating(Ride ride, Guid ratedByUserId, Guid ratedUserId, int score, string? comment = null)
    {
        if (score < 1 || score > 5)
            throw new ArgumentException("Score must be between 1 and 5");
        Id = Guid.NewGuid();
        RideId = ride.Id;
        Ride = ride;
        RatedByUserId = ratedByUserId;
        RatedUserId = ratedUserId;
        Score = score;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }
}
