using RideSharing.Core.Entities;

namespace RideSharing.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<Payment?> GetByRideIdAsync(Guid rideId);
    Task<Payment> AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
}
