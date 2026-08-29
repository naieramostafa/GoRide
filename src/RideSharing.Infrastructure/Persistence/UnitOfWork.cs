using Microsoft.EntityFrameworkCore.Storage;
using RideSharing.Application.Interfaces;
using RideSharing.Infrastructure.Persistence.Repositories;

namespace RideSharing.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly RideSharingDbContext _context;
    private IDbContextTransaction? _transaction;
    private IRideRepository? _rides;
    private IUserRepository? _users;
    private IDriverRepository? _drivers;
    private IPassengerRepository? _passengers;
    private IPaymentRepository? _payments;
    private ITaskRepository? _tasks;
    private IRatingRepository? _ratings;
    private IRefreshTokenRepository? _refreshTokens;

    public UnitOfWork(RideSharingDbContext context)
    {
        _context = context;
    }

    public IRideRepository Rides => _rides ??= new RideRepository(_context);
    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IDriverRepository Drivers => _drivers ??= new DriverRepository(_context);
    public IPassengerRepository Passengers => _passengers ??= new PassengerRepository(_context);
    public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
    public ITaskRepository Tasks => _tasks ??= new TaskRepository(_context);
    public IRatingRepository Ratings => _ratings ??= new RatingRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);

    public async Task<int> CommitAsync()
    {
        if (_transaction == null)
            _transaction = await _context.Database.BeginTransactionAsync();

        var result = await _context.SaveChangesAsync();
        await _transaction.CommitAsync();
        _transaction.Dispose();
        _transaction = null;
        return result;
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            _transaction.Dispose();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
