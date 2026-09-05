using System.ComponentModel.DataAnnotations;
using TaskManagement.Server.Common;

namespace TaskManagement.Server.DTOs;

public record CreateTeamRequest(
    [Required(ErrorMessage = AppMessages.Validation.TeamNameRequired)]
    [StringLength(100, MinimumLength = 2, ErrorMessage = AppMessages.Validation.TeamNameRequired)]
    string Name,

    [StringLength(500)]
    string? Description,

    int? LeadManagerId = null
);

public record UpdateTeamRequest(
    [Required(ErrorMessage = AppMessages.Validation.TeamNameRequired)]
    [StringLength(100, MinimumLength = 2, ErrorMessage = AppMessages.Validation.TeamNameRequired)]
    string Name,

    [StringLength(500)]
    string? Description,

    int LeadManagerId
);

public record AddTeamMemberRequest(
    [Required(ErrorMessage = AppMessages.Validation.IdPositive)]
    int UserId
);

public record TeamMemberDto(
    int UserId,
    string FullName,
    string Email,
    UserRole Role,
    DateTime JoinedAt
);

public record TeamResponseDto(
    int Id,
    string Name,
    string? Description,
    int LeadManagerId,
    string LeadManagerName,
    int MemberCount,
    int ActiveTaskCount,
    List<TeamMemberDto> Members,
    DateTime CreatedAt
);
