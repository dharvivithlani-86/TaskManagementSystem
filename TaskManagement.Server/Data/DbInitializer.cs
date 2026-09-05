using Microsoft.EntityFrameworkCore;
using TaskManagement.Server.Common;
using TaskManagement.Server.Entities;

namespace TaskManagement.Server.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        try
        {
            await context.Database.EnsureCreatedAsync();

            if (await context.Users.AnyAsync())
            {
                logger.LogInformation("Database already contains data. Skipping initial seeding.");
                return;
            }

            logger.LogInformation("Seeding initial Task Management data...");

            // 1. Seed Users
            var adminUser = new User
            {
                FullName = "System Administrator",
                Email = "admin@tms.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@12345"),
                Role = UserRole.Admin,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            };

            var managerUser = new User
            {
                FullName = "Sarah Jenkins (Lead Manager)",
                Email = "manager@tms.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@12345"),
                Role = UserRole.Manager,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            };

            var memberUser1 = new User
            {
                FullName = "Alex Rivera (Developer)",
                Email = "user@tms.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@12345"),
                Role = UserRole.User,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            };

            var memberUser2 = new User
            {
                FullName = "Elena Rostova (Frontend Engineer)",
                Email = "elena@tms.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@12345"),
                Role = UserRole.User,
                CreatedBy = "System",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(adminUser, managerUser, memberUser1, memberUser2);
            await context.SaveChangesAsync();

            // 2. Seed Teams
            var platformTeam = new Team
            {
                Name = "Core Engineering Team",
                Description = "Cross-functional squad focused on API architecture, microservices, and client applications.",
                LeadManagerId = managerUser.Id,
                CreatedBy = adminUser.Email,
                CreatedAt = DateTime.UtcNow
            };

            var designTeam = new Team
            {
                Name = "Product & UX Team",
                Description = "UI design system, accessibility compliance, and product discovery.",
                LeadManagerId = managerUser.Id,
                CreatedBy = adminUser.Email,
                CreatedAt = DateTime.UtcNow
            };

            context.Teams.AddRange(platformTeam, designTeam);
            await context.SaveChangesAsync();

            // 3. Seed Team Members
            var member1 = new TeamMember
            {
                TeamId = platformTeam.Id,
                UserId = memberUser1.Id,
                JoinedAt = DateTime.UtcNow,
                CreatedBy = managerUser.Email
            };

            var member2 = new TeamMember
            {
                TeamId = platformTeam.Id,
                UserId = memberUser2.Id,
                JoinedAt = DateTime.UtcNow,
                CreatedBy = managerUser.Email
            };

            var member3 = new TeamMember
            {
                TeamId = designTeam.Id,
                UserId = memberUser2.Id,
                JoinedAt = DateTime.UtcNow,
                CreatedBy = managerUser.Email
            };

            context.TeamMembers.AddRange(member1, member2, member3);
            await context.SaveChangesAsync();

            // 4. Seed Tasks
            var task1 = new TaskItem
            {
                Title = "Implement JWT Authentication & RBAC Filters",
                Description = "Establish secure JSON Web Token authentication with claims for Admin, Manager, and User roles. Ensure token expiration handling.",
                Status = TaskItemStatus.Done,
                Priority = TaskPriority.High,
                DueDate = DateTime.UtcNow.AddDays(2),
                TeamId = platformTeam.Id,
                CreatedByUserId = managerUser.Id,
                AssignedToUserId = memberUser1.Id,
                CreatedBy = managerUser.Email,
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            };

            var task2 = new TaskItem
            {
                Title = "Build Interactive Kanban Board & Task Filter Grid",
                Description = "Deliver responsive drag-or-click Kanban board and table filterable by status, priority, and deadline.",
                Status = TaskItemStatus.InProgress,
                Priority = TaskPriority.Urgent,
                DueDate = DateTime.UtcNow.AddDays(5),
                TeamId = platformTeam.Id,
                CreatedByUserId = managerUser.Id,
                AssignedToUserId = memberUser1.Id,
                CreatedBy = managerUser.Email,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var task3 = new TaskItem
            {
                Title = "Develop In-App Notification Center and Mock Email Service",
                Description = "Trigger notifications on task assignment and status updates with real-time UI indicator and email logging.",
                Status = TaskItemStatus.ToDo,
                Priority = TaskPriority.Medium,
                DueDate = DateTime.UtcNow.AddDays(7),
                TeamId = platformTeam.Id,
                CreatedByUserId = managerUser.Id,
                AssignedToUserId = memberUser2.Id,
                CreatedBy = managerUser.Email,
                CreatedAt = DateTime.UtcNow
            };

            var task4 = new TaskItem
            {
                Title = "Audit Mobile Responsive Breakpoints & Accessibility",
                Description = "Validate WCAG 2.1 AA accessibility contrast, keyboard navigation, and responsive drawer layouts on mobile screens.",
                Status = TaskItemStatus.ToDo,
                Priority = TaskPriority.Low,
                DueDate = DateTime.UtcNow.AddDays(10),
                TeamId = designTeam.Id,
                CreatedByUserId = adminUser.Id,
                AssignedToUserId = memberUser2.Id,
                CreatedBy = adminUser.Email,
                CreatedAt = DateTime.UtcNow
            };

            context.TaskItems.AddRange(task1, task2, task3, task4);
            await context.SaveChangesAsync();

            // 5. Seed Comments
            var comment1 = new Comment
            {
                TaskItemId = task2.Id,
                UserId = managerUser.Id,
                Content = "Great progress on the board layouts! Let's ensure columns reflect To Do, In Progress, and Done clearly.",
                CreatedBy = managerUser.Email,
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            };

            var comment2 = new Comment
            {
                TaskItemId = task2.Id,
                UserId = memberUser1.Id,
                Content = "Working on the status transition endpoints and optimistic UI updates now.",
                CreatedBy = memberUser1.Email,
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            };

            context.Comments.AddRange(comment1, comment2);

            // 6. Seed Notifications
            var notif1 = new NotificationLog
            {
                UserId = memberUser1.Id,
                TaskItemId = task2.Id,
                Title = AppMessages.Notifications.TaskAssignedTitle,
                Message = AppMessages.Notifications.TaskAssignedBody(task2.Title, managerUser.FullName),
                Type = NotificationType.TaskAssigned,
                IsRead = false,
                EmailSent = true,
                EmailSentAt = DateTime.UtcNow.AddDays(-1),
                CreatedBy = managerUser.Email,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            var notif2 = new NotificationLog
            {
                UserId = memberUser2.Id,
                TaskItemId = task3.Id,
                Title = AppMessages.Notifications.TaskAssignedTitle,
                Message = AppMessages.Notifications.TaskAssignedBody(task3.Title, managerUser.FullName),
                Type = NotificationType.TaskAssigned,
                IsRead = false,
                EmailSent = true,
                EmailSentAt = DateTime.UtcNow,
                CreatedBy = managerUser.Email,
                CreatedAt = DateTime.UtcNow
            };

            context.NotificationLogs.AddRange(notif1, notif2);
            await context.SaveChangesAsync();

            logger.LogInformation("Database successfully seeded with default users, teams, tasks, and notifications.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database: {Message}", ex.Message);
            throw;
        }
    }
}
