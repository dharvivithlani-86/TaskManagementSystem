using System.ComponentModel.DataAnnotations;
using TaskManagement.Server.Common;

namespace TaskManagement.Server.DTOs;

public record CreateCommentRequest(
    [Required(ErrorMessage = AppMessages.Validation.CommentRequired)]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = AppMessages.Validation.CommentRequired)]
    string Content
);

public record CommentResponseDto(
    int Id,
    int TaskItemId,
    int UserId,
    string UserName,
    string UserEmail,
    UserRole UserRole,
    string Content,
    DateTime CreatedAt
);
