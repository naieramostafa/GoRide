using Microsoft.EntityFrameworkCore;
using RideSharing.Infrastructure.Persistence;

namespace RideSharing.Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<T> where T : class
{
    protected readonly RideSharingDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected BaseRepository(RideSharingDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }
}
