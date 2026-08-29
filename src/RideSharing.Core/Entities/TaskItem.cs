using RideSharing.Core.Enums;

namespace RideSharing.Core.Entities;

public class TaskItem
{
    public Guid Id { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public User? AssignedTo { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TaskItemStatus Status { get; private set; }
    public TaskPriority Priority { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Guid? RelatedEntityId { get; private set; }

    private TaskItem() { }

    public TaskItem(string title, string description, TaskPriority priority,
                    Guid? assignedTo = null, DateTime? dueDate = null)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Priority = priority;
        AssignedToUserId = assignedTo;
        DueDate = dueDate;
        Status = TaskItemStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Assign(Guid userId) => AssignedToUserId = userId;
    public void Start() => Status = TaskItemStatus.InProgress;
    public void Complete() { Status = TaskItemStatus.Completed; CompletedAt = DateTime.UtcNow; }
    public void Cancel() => Status = TaskItemStatus.Cancelled;
}
