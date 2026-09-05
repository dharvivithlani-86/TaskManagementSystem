using TaskManagement.Server.Common;

namespace TaskManagement.Server.Entities;

public class NotificationLog : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public bool EmailSent { get; set; } = false;
    public DateTime? EmailSentAt { get; set; }
}
