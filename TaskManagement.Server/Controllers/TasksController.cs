using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Server.Common;
using TaskManagement.Server.Data;
using TaskManagement.Server.DTOs;
using TaskManagement.Server.Entities;
using TaskManagement.Server.Services;

namespace TaskManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        INotificationService notificationService,
        ILogger<TasksController> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves tasks with filtering by deadline, status, priority, team, and assignee, with pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TaskResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks([FromQuery] TaskFilterParams filter)
    {
        var currentUserId = _currentUserService.UserId!.Value;
        var role = _currentUserService.Role!.Value;

        var query = _context.TaskItems
            .Include(t => t.Team)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments)
            .AsNoTracking()
            .AsQueryable();

        // Role-based visibility scoping
        if (role == UserRole.User)
        {
            // Users see tasks assigned to them or created by them
            query = query.Where(t => t.AssignedToUserId == currentUserId || t.CreatedByUserId == currentUserId);
        }
        else if (role == UserRole.Manager)
        {
            // Managers see tasks created by them, assigned to them, or in teams they manage
            query = query.Where(t => t.CreatedByUserId == currentUserId 
                                  || t.AssignedToUserId == currentUserId 
                                  || (t.Team != null && t.Team.LeadManagerId == currentUserId));
        }
        // Admin sees all

        // Filter: Search keyword
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(search) || t.Description.ToLower().Contains(search));
        }

        // Filter: Status
        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.Value);
        }

        // Filter: Priority
        if (filter.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filter.Priority.Value);
        }

        // Filter: Assignee
        if (filter.AssignedToUserId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId.Value);
        }

        // Filter: Team
        if (filter.TeamId.HasValue)
        {
            query = query.Where(t => t.TeamId == filter.TeamId.Value);
        }

        // Filter: Deadline range
        if (filter.DueDateFrom.HasValue)
        {
            query = query.Where(t => t.DueDate >= filter.DueDateFrom.Value);
        }
        if (filter.DueDateTo.HasValue)
        {
            query = query.Where(t => t.DueDate <= filter.DueDateTo.Value);
        }

        // Sorting
        query = (filter.SortBy?.ToLower(), filter.IsAscending) switch
        {
            ("title", true) => query.OrderBy(t => t.Title),
            ("title", false) => query.OrderByDescending(t => t.Title),
            ("priority", true) => query.OrderBy(t => t.Priority),
            ("priority", false) => query.OrderByDescending(t => t.Priority),
            ("status", true) => query.OrderBy(t => t.Status),
            ("status", false) => query.OrderByDescending(t => t.Status),
            ("createdat", true) => query.OrderBy(t => t.CreatedAt),
            ("createdat", false) => query.OrderByDescending(t => t.CreatedAt),
            ("duedate", false) => query.OrderByDescending(t => t.DueDate),
            _ => query.OrderBy(t => t.DueDate)
        };

        var totalCount = await query.CountAsync();
        var pageIndex = Math.Max(1, filter.PageIndex);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TaskResponseDto(
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.DueDate,
                t.TeamId,
                t.Team != null ? t.Team.Name : null,
                t.CreatedByUserId,
                t.CreatedByUser.FullName,
                t.AssignedToUserId,
                t.AssignedToUser != null ? t.AssignedToUser.FullName : null,
                t.Comments.Count,
                t.CreatedAt,
                t.UpdatedAt
            ))
            .ToListAsync();

        var pagedResult = new PagedResult<TaskResponseDto>(items, totalCount, pageIndex, pageSize);
        return Ok(ApiResponse<PagedResult<TaskResponseDto>>.Ok(pagedResult, AppMessages.General.FetchSuccess));
    }

    /// <summary>
    /// Gets detailed information for a single task.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskById(int id)
    {
        var task = await _context.TaskItems
            .Include(t => t.Team)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Tasks.NotFound));
        }

        var dto = new TaskResponseDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate,
            task.TeamId,
            task.Team?.Name,
            task.CreatedByUserId,
            task.CreatedByUser.FullName,
            task.AssignedToUserId,
            task.AssignedToUser?.FullName,
            task.Comments.Count,
            task.CreatedAt,
            task.UpdatedAt
        );

        return Ok(ApiResponse<TaskResponseDto>.Ok(dto, AppMessages.General.FetchSuccess));
    }

    /// <summary>
    /// Creates a new task and dispatches notification if assigned (Admin & Manager).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse.Fail(AppMessages.Validation.RequiredField, errors));
        }

        var currentUserId = _currentUserService.UserId!.Value;
        var currentUserRole = _currentUserService.Role!.Value;

        // If team is specified, verify existence and membership
        if (request.TeamId.HasValue)
        {
            var team = await _context.Teams.Include(t => t.Members).FirstOrDefaultAsync(t => t.Id == request.TeamId.Value);
            if (team == null)
            {
                return BadRequest(ApiResponse.Fail(AppMessages.Teams.NotFound));
            }

            if (currentUserRole == UserRole.Manager && team.LeadManagerId != currentUserId)
            {
                return Forbid();
            }

            // If assigning to a user, verify user is in this team
            if (request.AssignedToUserId.HasValue)
            {
                if (!team.Members.Any(m => m.UserId == request.AssignedToUserId.Value))
                {
                    return BadRequest(ApiResponse.Fail(AppMessages.Tasks.AssigneeNotMember));
                }
            }
        }

        var task = new TaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = TaskItemStatus.ToDo,
            Priority = request.Priority,
            DueDate = request.DueDate ?? DateTime.UtcNow.AddDays(7),
            TeamId = request.TeamId,
            CreatedByUserId = currentUserId,
            AssignedToUserId = request.AssignedToUserId,
            CreatedBy = _currentUserService.Email ?? "System"
        };

        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();

        var creator = await _context.Users.FindAsync(currentUserId);

        // Notify assignee if assigned
        if (request.AssignedToUserId.HasValue)
        {
            var assignee = await _context.Users.FindAsync(request.AssignedToUserId.Value);
            if (assignee != null && creator != null)
            {
                await _notificationService.NotifyTaskAssignedAsync(task, assignee, creator);
            }
        }

        return await GetTaskById(task.Id);
    }

    /// <summary>
    /// Updates an existing task's properties (Admin & Manager).
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
    {
        var task = await _context.TaskItems
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Tasks.NotFound));
        }

        var currentUserId = _currentUserService.UserId!.Value;
        var role = _currentUserService.Role!.Value;

        if (role == UserRole.Manager && task.CreatedByUserId != currentUserId && (task.Team == null || task.Team.LeadManagerId != currentUserId))
        {
            return Forbid();
        }

        var oldAssigneeId = task.AssignedToUserId;
        var oldStatus = task.Status;

        task.Title = request.Title.Trim();
        task.Description = request.Description.Trim();
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.TeamId = request.TeamId;
        task.AssignedToUserId = request.AssignedToUserId;

        await _context.SaveChangesAsync();

        var currentUser = await _context.Users.FindAsync(currentUserId);

        // Notify if assignee changed
        if (request.AssignedToUserId.HasValue && request.AssignedToUserId != oldAssigneeId)
        {
            var newAssignee = await _context.Users.FindAsync(request.AssignedToUserId.Value);
            if (newAssignee != null && currentUser != null)
            {
                await _notificationService.NotifyTaskAssignedAsync(task, newAssignee, currentUser);
            }
        }

        // Notify if status changed
        if (request.Status != oldStatus && currentUser != null)
        {
            await _notificationService.NotifyTaskStatusChangedAsync(task, oldStatus, request.Status, currentUser);
        }

        return await GetTaskById(id);
    }

    /// <summary>
    /// Updates only the status of a task (Assignee, Manager, or Admin).
    /// Triggers email and in-app status update notifications.
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateTaskStatusRequest request)
    {
        var task = await _context.TaskItems
            .Include(t => t.Team)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Tasks.NotFound));
        }

        var currentUserId = _currentUserService.UserId!.Value;
        var role = _currentUserService.Role!.Value;

        // Permissions: Admin can always update; Manager can update if creator or team lead; User can update if assigned
        bool canUpdate = role == UserRole.Admin ||
                         task.AssignedToUserId == currentUserId ||
                         task.CreatedByUserId == currentUserId ||
                         (task.Team != null && task.Team.LeadManagerId == currentUserId);

        if (!canUpdate)
        {
            return Forbid();
        }

        var oldStatus = task.Status;
        if (oldStatus == request.Status)
        {
            return await GetTaskById(id);
        }

        task.Status = request.Status;
        await _context.SaveChangesAsync();

        var currentUser = await _context.Users.FindAsync(currentUserId);
        if (currentUser != null)
        {
            await _notificationService.NotifyTaskStatusChangedAsync(task, oldStatus, request.Status, currentUser);
        }

        return await GetTaskById(id);
    }

    /// <summary>
    /// Deletes a task via soft delete (Admin or creator Manager).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.TaskItems.FindAsync(id);
        if (task == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Tasks.NotFound));
        }

        var currentUserId = _currentUserService.UserId!.Value;
        var role = _currentUserService.Role!.Value;

        if (role != UserRole.Admin && task.CreatedByUserId != currentUserId)
        {
            return Forbid();
        }

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok(AppMessages.Tasks.Deleted));
    }
}
