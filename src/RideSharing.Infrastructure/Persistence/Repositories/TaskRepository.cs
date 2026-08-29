using Microsoft.EntityFrameworkCore;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Entities;
using RideSharing.Core.Enums;

namespace RideSharing.Infrastructure.Persistence.Repositories;

public class TaskRepository : BaseRepository<TaskItem>, ITaskRepository
{
    public TaskRepository(RideSharingDbContext context) : base(context) { }

    public async Task<TaskItem?> GetByIdAsync(Guid id) =>
        await DbSet.Include(t => t.AssignedTo).FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<TaskItem>> GetByAssigneeAsync(Guid userId) =>
        await DbSet.Where(t => t.AssignedToUserId == userId).Include(t => t.AssignedTo).ToListAsync();

    public async Task<IEnumerable<TaskItem>> GetByStatusAsync(TaskItemStatus status) =>
        await DbSet.Where(t => t.Status == status).Include(t => t.AssignedTo).ToListAsync();

    public async Task<IEnumerable<TaskItem>> GetAllAsync() =>
        await DbSet.Include(t => t.AssignedTo).ToListAsync();

    public async Task<TaskItem> AddAsync(TaskItem task) { await DbSet.AddAsync(task); return task; }
    public Task UpdateAsync(TaskItem task) => Task.CompletedTask;
}
