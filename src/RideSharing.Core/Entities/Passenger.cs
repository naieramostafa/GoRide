namespace RideSharing.Core.Entities;

public class Passenger
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public double Rating { get; private set; } = 5.0;
    public int TotalRides { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Passenger() { }

    public Passenger(User user)
    {
        Id = Guid.NewGuid();
        UserId = user.Id;
        User = user;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateRating(double newRating)
    {
        Rating = ((Rating * TotalRides) + newRating) / (TotalRides + 1);
        TotalRides++;
    }
}
