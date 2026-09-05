using TaskManagement.Server.Common;

namespace TaskManagement.Server.DTOs;

public record NotificationResponseDto(
    int Id,
    int? TaskItemId,
    string? TaskTitle,
    string Title,
    string Message,
    NotificationType Type,
    bool IsRead,
    DateTime CreatedAt
);
