using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagement.Server.Common;
using TaskManagement.Server.Data;
using TaskManagement.Server.Entities;
using TaskManagement.Server.Services;
using Xunit;

namespace TaskManagement.Tests;

public class TaskWorkflowTests
{
    [Fact]
    public async Task NotificationService_ShouldRecordNotification_WhenTaskAssigned()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var logger = new NullLogger<NotificationService>();
        var notificationService = new NotificationService(context, logger);

        var assigner = new User
        {
            Id = 1,
            FullName = "Manager Mike",
            Email = "mike@tms.com",
            Role = UserRole.Manager
        };

        var assignee = new User
        {
            Id = 2,
            FullName = "Dev Dave",
            Email = "dave@tms.com",
            Role = UserRole.User
        };

        var task = new TaskItem
        {
            Id = 101,
            Title = "Implement Unit Tests",
            Description = "Write xUnit tests",
            Status = TaskItemStatus.ToDo,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(2),
            CreatedByUserId = assigner.Id,
            AssignedToUserId = assignee.Id
        };

        context.Users.AddRange(assigner, assignee);
        context.TaskItems.Add(task);
        await context.SaveChangesAsync();

        // Act
        await notificationService.NotifyTaskAssignedAsync(task, assignee, assigner);

        // Assert
        var notification = await context.NotificationLogs.FirstOrDefaultAsync(n => n.UserId == assignee.Id);
        notification.Should().NotBeNull();
        notification!.Type.Should().Be(NotificationType.TaskAssigned);
        notification.Message.Should().Contain(task.Title);
        notification.Message.Should().Contain(assigner.FullName);
        notification.EmailSent.Should().BeTrue();
    }

    [Fact]
    public async Task NotificationService_ShouldRecordNotification_WhenStatusChanged()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options);
        var logger = new NullLogger<NotificationService>();
        var notificationService = new NotificationService(context, logger);

        var creator = new User
        {
            Id = 1,
            FullName = "Manager Mike",
            Email = "mike@tms.com",
            Role = UserRole.Manager
        };

        var assignee = new User
        {
            Id = 2,
            FullName = "Dev Dave",
            Email = "dave@tms.com",
            Role = UserRole.User
        };

        var task = new TaskItem
        {
            Id = 102,
            Title = "Deploy to Production",
            Description = "Publish release pipeline",
            Status = TaskItemStatus.ToDo,
            Priority = TaskPriority.Urgent,
            DueDate = DateTime.UtcNow.AddDays(1),
            CreatedByUserId = creator.Id,
            AssignedToUserId = assignee.Id
        };

        context.Users.AddRange(creator, assignee);
        context.TaskItems.Add(task);
        await context.SaveChangesAsync();

        // Act: Assignee updates status from ToDo to InProgress
        await notificationService.NotifyTaskStatusChangedAsync(task, TaskItemStatus.ToDo, TaskItemStatus.InProgress, assignee);

        // Assert: Creator should be notified
        var notification = await context.NotificationLogs.FirstOrDefaultAsync(n => n.UserId == creator.Id);
        notification.Should().NotBeNull();
        notification!.Type.Should().Be(NotificationType.StatusChanged);
        notification.Message.Should().Contain("ToDo");
        notification.Message.Should().Contain("InProgress");
    }
}
