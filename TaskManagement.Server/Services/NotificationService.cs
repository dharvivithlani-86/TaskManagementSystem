using Microsoft.EntityFrameworkCore;
using TaskManagement.Server.Common;
using TaskManagement.Server.Data;
using TaskManagement.Server.Entities;

namespace TaskManagement.Server.Services;

public interface INotificationService
{
    Task NotifyTaskAssignedAsync(TaskItem task, User assignee, User assignedBy);
    Task NotifyTaskStatusChangedAsync(TaskItem task, TaskItemStatus oldStatus, TaskItemStatus newStatus, User updatedBy);
    Task NotifyCommentAddedAsync(TaskItem task, Comment comment, User author);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ApplicationDbContext dbContext, ILogger<NotificationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task NotifyTaskAssignedAsync(TaskItem task, User assignee, User assignedBy)
    {
        var title = AppMessages.Notifications.TaskAssignedTitle;
        var message = AppMessages.Notifications.TaskAssignedBody(task.Title, assignedBy.FullName);

        var notification = new NotificationLog
        {
            UserId = assignee.Id,
            TaskItemId = task.Id,
            Title = title,
            Message = message,
            Type = NotificationType.TaskAssigned,
            IsRead = false,
            EmailSent = true,
            EmailSentAt = DateTime.UtcNow
        };

        _dbContext.NotificationLogs.Add(notification);
        await _dbContext.SaveChangesAsync();

        // Simulate sending email notification
        SendMockEmail(
            recipientEmail: assignee.Email,
            recipientName: assignee.FullName,
            subject: title,
            body: $"Hello {assignee.FullName},\n\n{message}\n\nTask Priority: {task.Priority}\nDue Date: {task.DueDate:yyyy-MM-dd}\n\nBest regards,\nTeam Task Management System"
        );
    }

    public async Task NotifyTaskStatusChangedAsync(TaskItem task, TaskItemStatus oldStatus, TaskItemStatus newStatus, User updatedBy)
    {
        var title = AppMessages.Notifications.TaskStatusChangedTitle;
        var message = AppMessages.Notifications.TaskStatusChangedBody(
            task.Title, 
            oldStatus.ToString(), 
            newStatus.ToString(), 
            updatedBy.FullName
        );

        // Determine who to notify:
        // If the updater is the assignee, notify creator.
        // If the updater is the creator/manager, notify assignee.
        // If neither, notify both if applicable.
        var recipientUserIds = new HashSet<int>();
        if (task.AssignedToUserId.HasValue && task.AssignedToUserId.Value != updatedBy.Id)
        {
            recipientUserIds.Add(task.AssignedToUserId.Value);
        }
        if (task.CreatedByUserId != updatedBy.Id)
        {
            recipientUserIds.Add(task.CreatedByUserId);
        }

        var recipients = await _dbContext.Users
            .Where(u => recipientUserIds.Contains(u.Id))
            .ToListAsync();

        foreach (var recipient in recipients)
        {
            var notification = new NotificationLog
            {
                UserId = recipient.Id,
                TaskItemId = task.Id,
                Title = title,
                Message = message,
                Type = NotificationType.StatusChanged,
                IsRead = false,
                EmailSent = true,
                EmailSentAt = DateTime.UtcNow
            };

            _dbContext.NotificationLogs.Add(notification);

            SendMockEmail(
                recipientEmail: recipient.Email,
                recipientName: recipient.FullName,
                subject: title,
                body: $"Hello {recipient.FullName},\n\n{message}\n\nCurrent Status: {newStatus}\n\nBest regards,\nTeam Task Management System"
            );
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task NotifyCommentAddedAsync(TaskItem task, Comment comment, User author)
    {
        var title = AppMessages.Notifications.CommentAddedTitle;
        var message = AppMessages.Notifications.CommentAddedBody(task.Title, author.FullName);

        var recipientUserIds = new HashSet<int>();
        if (task.AssignedToUserId.HasValue && task.AssignedToUserId.Value != author.Id)
        {
            recipientUserIds.Add(task.AssignedToUserId.Value);
        }
        if (task.CreatedByUserId != author.Id)
        {
            recipientUserIds.Add(task.CreatedByUserId);
        }

        var recipients = await _dbContext.Users
            .Where(u => recipientUserIds.Contains(u.Id))
            .ToListAsync();

        foreach (var recipient in recipients)
        {
            var notification = new NotificationLog
            {
                UserId = recipient.Id,
                TaskItemId = task.Id,
                Title = title,
                Message = message,
                Type = NotificationType.CommentAdded,
                IsRead = false,
                EmailSent = true,
                EmailSentAt = DateTime.UtcNow
            };

            _dbContext.NotificationLogs.Add(notification);
        }

        await _dbContext.SaveChangesAsync();
    }

    private void SendMockEmail(string recipientEmail, string recipientName, string subject, string body)
    {
        _logger.LogInformation(
            "\n==================== [EMAIL DISPATCH NOTIFICATION] ====================\n" +
            "To: {RecipientName} <{RecipientEmail}>\n" +
            "Subject: {Subject}\n" +
            "Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss UTC}\n" +
            "Content:\n{Body}\n" +
            "=======================================================================",
            recipientName, recipientEmail, subject, DateTime.UtcNow, body
        );
    }
}
