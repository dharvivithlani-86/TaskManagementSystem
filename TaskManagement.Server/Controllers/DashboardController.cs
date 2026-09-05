using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Server.Common;
using TaskManagement.Server.Data;
using TaskManagement.Server.DTOs;
using TaskManagement.Server.Services;

namespace TaskManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DashboardController(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets aggregated metrics and status overview tailored to the user's role.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var currentUserId = _currentUserService.UserId!.Value;
        var role = _currentUserService.Role!.Value;

        var taskQuery = _context.TaskItems
            .Include(t => t.Team)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments)
            .AsNoTracking()
            .AsQueryable();

        // Scope by role
        if (role == UserRole.User)
        {
            taskQuery = taskQuery.Where(t => t.AssignedToUserId == currentUserId || t.CreatedByUserId == currentUserId);
        }
        else if (role == UserRole.Manager)
        {
            taskQuery = taskQuery.Where(t => t.CreatedByUserId == currentUserId 
                                          || t.AssignedToUserId == currentUserId 
                                          || (t.Team != null && t.Team.LeadManagerId == currentUserId));
        }

        var now = DateTime.UtcNow;
        var allTasks = await taskQuery.ToListAsync();

        var totalTasks = allTasks.Count;
        var todoCount = allTasks.Count(t => t.Status == TaskItemStatus.ToDo);
        var inProgressCount = allTasks.Count(t => t.Status == TaskItemStatus.InProgress);
        var doneCount = allTasks.Count(t => t.Status == TaskItemStatus.Done);
        var overdueCount = allTasks.Count(t => t.Status != TaskItemStatus.Done && t.DueDate < now);

        var totalTeams = role == UserRole.User
            ? await _context.TeamMembers.CountAsync(tm => tm.UserId == currentUserId)
            : await _context.Teams.CountAsync();

        var totalMembers = await _context.Users.CountAsync();

        var statusBreakdown = new List<StatusCountItem>
        {
            new(TaskItemStatus.ToDo, "To Do", todoCount, totalTasks > 0 ? Math.Round((double)todoCount / totalTasks * 100, 1) : 0),
            new(TaskItemStatus.InProgress, "In Progress", inProgressCount, totalTasks > 0 ? Math.Round((double)inProgressCount / totalTasks * 100, 1) : 0),
            new(TaskItemStatus.Done, "Done", doneCount, totalTasks > 0 ? Math.Round((double)doneCount / totalTasks * 100, 1) : 0)
        };

        var priorityBreakdown = new List<PriorityCountItem>
        {
            new(TaskPriority.Low, "Low", allTasks.Count(t => t.Priority == TaskPriority.Low)),
            new(TaskPriority.Medium, "Medium", allTasks.Count(t => t.Priority == TaskPriority.Medium)),
            new(TaskPriority.High, "High", allTasks.Count(t => t.Priority == TaskPriority.High)),
            new(TaskPriority.Urgent, "Urgent", allTasks.Count(t => t.Priority == TaskPriority.Urgent))
        };

        var upcomingTasks = allTasks
            .Where(t => t.Status != TaskItemStatus.Done)
            .OrderBy(t => t.DueDate)
            .Take(5)
            .Select(t => new TaskResponseDto(
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.DueDate,
                t.TeamId,
                t.Team?.Name,
                t.CreatedByUserId,
                t.CreatedByUser.FullName,
                t.AssignedToUserId,
                t.AssignedToUser?.FullName,
                t.Comments.Count,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .ToList();

        var recentActivities = await _context.NotificationLogs
            .Where(n => n.UserId == currentUserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(6)
            .Select(n => new RecentActivityDto(
                n.Id,
                n.Title,
                n.Message,
                n.Type,
                n.CreatedAt
            ))
            .ToListAsync();

        var summary = new DashboardSummaryDto(
            totalTasks,
            todoCount,
            inProgressCount,
            doneCount,
            overdueCount,
            totalTeams,
            totalMembers,
            statusBreakdown,
            priorityBreakdown,
            upcomingTasks,
            recentActivities
        );

        return Ok(ApiResponse<DashboardSummaryDto>.Ok(summary, AppMessages.General.FetchSuccess));
    }
}
