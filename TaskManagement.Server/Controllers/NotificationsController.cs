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
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets all notifications for the current authenticated user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<NotificationResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserNotifications()
    {
        var currentUserId = _currentUserService.UserId!.Value;

        var notifications = await _context.NotificationLogs
            .Include(n => n.TaskItem)
            .Where(n => n.UserId == currentUserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationResponseDto(
                n.Id,
                n.TaskItemId,
                n.TaskItem != null ? n.TaskItem.Title : null,
                n.Title,
                n.Message,
                n.Type,
                n.IsRead,
                n.CreatedAt
            ))
            .ToListAsync();

        return Ok(ApiResponse<List<NotificationResponseDto>>.Ok(notifications, AppMessages.General.FetchSuccess));
    }

    /// <summary>
    /// Marks a single notification as read.
    /// </summary>
    [HttpPatch("{id}/read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var currentUserId = _currentUserService.UserId!.Value;
        var notif = await _context.NotificationLogs
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == currentUserId);

        if (notif == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Notifications.NotFound));
        }

        notif.IsRead = true;
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok(AppMessages.Notifications.MarkedAsRead));
    }

    /// <summary>
    /// Marks all unread notifications for current user as read.
    /// </summary>
    [HttpPatch("read-all")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var currentUserId = _currentUserService.UserId!.Value;

        var unread = await _context.NotificationLogs
            .Where(n => n.UserId == currentUserId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
        }

        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok(AppMessages.Notifications.AllMarkedAsRead));
    }
}
