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
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;

    public CommentsController(
        ApplicationDbContext context,
        ICurrentUserService currentUserService,
        INotificationService notificationService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Gets all comments for a specific task in chronological order.
    /// </summary>
    [HttpGet("api/tasks/{taskId}/comments")]
    [ProducesResponseType(typeof(ApiResponse<List<CommentResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskComments(int taskId)
    {
        var taskExists = await _context.TaskItems.AnyAsync(t => t.Id == taskId);
        if (!taskExists)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Tasks.NotFound));
        }

        var comments = await _context.Comments
            .Include(c => c.User)
            .Where(c => c.TaskItemId == taskId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentResponseDto(
                c.Id,
                c.TaskItemId,
                c.UserId,
                c.User.FullName,
                c.User.Email,
                c.User.Role,
                c.Content,
                c.CreatedAt
            ))
            .ToListAsync();

        return Ok(ApiResponse<List<CommentResponseDto>>.Ok(comments, AppMessages.General.FetchSuccess));
    }

    /// <summary>
    /// Adds a comment to a task.
    /// </summary>
    [HttpPost("api/tasks/{taskId}/comments")]
    [ProducesResponseType(typeof(ApiResponse<CommentResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddComment(int taskId, [FromBody] CreateCommentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.Fail(AppMessages.Comments.EmptyComment));
        }

        var task = await _context.TaskItems.FindAsync(taskId);
        if (task == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Tasks.NotFound));
        }

        var currentUserId = _currentUserService.UserId!.Value;
        var currentUser = await _context.Users.FindAsync(currentUserId);
        if (currentUser == null)
        {
            return Unauthorized(ApiResponse.Fail(AppMessages.Auth.Unauthorized));
        }

        var comment = new Comment
        {
            TaskItemId = taskId,
            UserId = currentUserId,
            Content = request.Content.Trim(),
            CreatedBy = currentUser.Email
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        await _notificationService.NotifyCommentAddedAsync(task, comment, currentUser);

        var dto = new CommentResponseDto(
            comment.Id,
            comment.TaskItemId,
            currentUser.Id,
            currentUser.FullName,
            currentUser.Email,
            currentUser.Role,
            comment.Content,
            comment.CreatedAt
        );

        return CreatedAtAction(nameof(GetTaskComments), new { taskId }, ApiResponse<CommentResponseDto>.Ok(dto, AppMessages.Comments.Added));
    }

    /// <summary>
    /// Deletes a comment (Author or Admin).
    /// </summary>
    [HttpDelete("api/comments/{commentId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Comments.NotFound));
        }

        var currentUserId = _currentUserService.UserId!.Value;
        var role = _currentUserService.Role!.Value;

        if (role != UserRole.Admin && comment.UserId != currentUserId)
        {
            return Forbid();
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok(AppMessages.Comments.Deleted));
    }
}
