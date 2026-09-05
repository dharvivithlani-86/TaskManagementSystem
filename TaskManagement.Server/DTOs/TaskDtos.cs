using System.ComponentModel.DataAnnotations;
using TaskManagement.Server.Common;

namespace TaskManagement.Server.DTOs;

public record CreateTaskRequest(
    [Required(ErrorMessage = AppMessages.Validation.TitleRequired)]
    [StringLength(200, MinimumLength = 3, ErrorMessage = AppMessages.Validation.TitleRequired)]
    string Title,

    [Required(ErrorMessage = AppMessages.Validation.DescriptionRequired)]
    string Description,

    TaskPriority Priority = TaskPriority.Medium,
    DateTime? DueDate = null,
    int? TeamId = null,
    int? AssignedToUserId = null
);

public record UpdateTaskRequest(
    [Required(ErrorMessage = AppMessages.Validation.TitleRequired)]
    [StringLength(200, MinimumLength = 3, ErrorMessage = AppMessages.Validation.TitleRequired)]
    string Title,

    [Required(ErrorMessage = AppMessages.Validation.DescriptionRequired)]
    string Description,

    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime DueDate,
    int? TeamId = null,
    int? AssignedToUserId = null
);

public record UpdateTaskStatusRequest(
    [Required(ErrorMessage = AppMessages.Validation.StatusRequired)]
    TaskItemStatus Status
);

public record AssignTaskRequest(
    int? AssignedToUserId
);

public record TaskResponseDto(
    int Id,
    string Title,
    string Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime DueDate,
    int? TeamId,
    string? TeamName,
    int CreatedByUserId,
    string CreatedByName,
    int? AssignedToUserId,
    string? AssignedToName,
    int CommentsCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record TaskFilterParams(
    string? Search = null,
    TaskItemStatus? Status = null,
    TaskPriority? Priority = null,
    int? AssignedToUserId = null,
    int? TeamId = null,
    DateTime? DueDateFrom = null,
    DateTime? DueDateTo = null,
    string? SortBy = "DueDate",
    bool IsAscending = true,
    int PageIndex = 1,
    int PageSize = 10
);
