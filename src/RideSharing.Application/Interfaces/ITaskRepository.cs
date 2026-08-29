using RideSharing.Core.Entities;
using RideSharing.Core.Enums;

namespace RideSharing.Application.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskItem>> GetByAssigneeAsync(Guid userId);
    Task<IEnumerable<TaskItem>> GetByStatusAsync(TaskItemStatus status);
    Task<TaskItem> AddAsync(TaskItem task);
    Task UpdateAsync(TaskItem task);
    Task<IEnumerable<TaskItem>> GetAllAsync();
}
