namespace RideSharing.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRideRepository Rides { get; }
    IUserRepository Users { get; }
    IDriverRepository Drivers { get; }
    IPassengerRepository Passengers { get; }
    IPaymentRepository Payments { get; }
    ITaskRepository Tasks { get; }
    IRatingRepository Ratings { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    Task<int> CommitAsync();
    Task BeginTransactionAsync();
    Task RollbackAsync();
}
