using System.ComponentModel.DataAnnotations;
using TaskManagement.Server.Common;

namespace TaskManagement.Server.DTOs;

public record RegisterRequest(
    [Required(ErrorMessage = AppMessages.Validation.FullNameRequired)]
    [StringLength(100, MinimumLength = 2, ErrorMessage = AppMessages.Validation.FullNameRequired)]
    string FullName,

    [Required(ErrorMessage = AppMessages.Validation.EmailRequired)]
    [EmailAddress(ErrorMessage = AppMessages.Validation.EmailRequired)]
    string Email,

    [Required(ErrorMessage = AppMessages.Validation.PasswordRequired)]
    [MinLength(6, ErrorMessage = AppMessages.Validation.PasswordRequired)]
    string Password,

    UserRole Role = UserRole.User
);

public record LoginRequest(
    [Required(ErrorMessage = AppMessages.Validation.EmailRequired)]
    [EmailAddress(ErrorMessage = AppMessages.Validation.EmailRequired)]
    string Email,

    [Required(ErrorMessage = AppMessages.Validation.PasswordRequired)]
    string Password
);

public record AuthResponse(
    string Token,
    DateTime ExpiresAt,
    UserDto User
);

public record UserDto(
    int Id,
    string FullName,
    string Email,
    UserRole Role,
    DateTime CreatedAt
);
