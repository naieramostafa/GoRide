using Microsoft.EntityFrameworkCore;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Entities;

namespace RideSharing.Infrastructure.Persistence.Repositories;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(RideSharingDbContext context) : base(context) { }

    public async Task<Payment?> GetByIdAsync(Guid id) => await DbSet.FindAsync(id);
    public async Task<Payment?> GetByRideIdAsync(Guid rideId) =>
        await DbSet.FirstOrDefaultAsync(p => p.RideId == rideId);
    public async Task<Payment> AddAsync(Payment payment) { await DbSet.AddAsync(payment); return payment; }
    public Task UpdateAsync(Payment payment) => Task.CompletedTask;
}
