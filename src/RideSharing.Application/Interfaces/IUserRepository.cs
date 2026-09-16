using RideSharing.Core.Entities;

namespace RideSharing.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> AddAsync(User user);

    /// <summary>
    /// Updates the user entity. Relies on EF Core change tracking —
    /// call this after modifying properties on a tracked entity.
    /// No explicit update call is needed on the DbContext.
    /// </summary>
    Task UpdateAsync(User user);
}
