using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaskManagement.Server.Common;
using TaskManagement.Server.Data;
using TaskManagement.Server.Entities;
using TaskManagement.Server.Services;
using Xunit;

namespace TaskManagement.Tests;

public class AuditEntityTests
{
    [Fact]
    public async Task SaveChangesAsync_ShouldAutomaticallyPopulateAuditFields_OnCreation()
    {
        // Arrange
        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(u => u.Email).Returns("auditor@tms.com");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options, mockUserService.Object);

        var task = new TaskItem
        {
            Title = "Automated Audit Test Task",
            Description = "Verifying CreatedBy and CreatedAt audit trail properties",
            Status = TaskItemStatus.ToDo,
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(3),
            CreatedByUserId = 1
        };

        // Act
        context.TaskItems.Add(task);
        await context.SaveChangesAsync();

        // Assert
        task.Id.Should().BeGreaterThan(0);
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        task.CreatedBy.Should().Be("auditor@tms.com");
        task.IsDeleted.Should().BeFalse();
        task.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task DeleteEntity_ShouldPerformSoftDelete_AndPopulateUpdatedAtAndUpdatedBy()
    {
        // Arrange
        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(u => u.Email).Returns("deleter@tms.com");

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ApplicationDbContext(options, mockUserService.Object);

        var task = new TaskItem
        {
            Title = "Task to be soft deleted",
            Description = "Testing soft delete behavior",
            Status = TaskItemStatus.ToDo,
            Priority = TaskPriority.Low,
            DueDate = DateTime.UtcNow.AddDays(5),
            CreatedByUserId = 1
        };

        context.TaskItems.Add(task);
        await context.SaveChangesAsync();

        // Act
        context.TaskItems.Remove(task);
        await context.SaveChangesAsync();

        // Assert
        task.IsDeleted.Should().BeTrue();
        task.UpdatedAt.Should().NotBeNull();
        task.UpdatedBy.Should().Be("deleter@tms.com");

        // Query with default filter should not return soft deleted record
        var activeTasks = await context.TaskItems.ToListAsync();
        activeTasks.Should().NotContain(t => t.Id == task.Id);

        // Query ignoring filters should return it
        var deletedTasks = await context.TaskItems.IgnoreQueryFilters().ToListAsync();
        deletedTasks.Should().Contain(t => t.Id == task.Id && t.IsDeleted);
    }
}
