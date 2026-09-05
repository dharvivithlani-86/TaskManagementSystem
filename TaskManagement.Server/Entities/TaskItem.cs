using TaskManagement.Server.Common;

namespace TaskManagement.Server.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime DueDate { get; set; }

    // Relationship to Team
    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    // Relationship to Creator
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    // Relationship to Assignee
    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    // Navigation properties
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<NotificationLog> Notifications { get; set; } = new List<NotificationLog>();
}
