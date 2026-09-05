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
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ApplicationDbContext context,
        ITokenService tokenService,
        ICurrentUserService currentUserService,
        ILogger<AuthController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and issues a JWT Bearer token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse.Fail(AppMessages.Validation.EmailRequired, errors));
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", request.Email);
            return BadRequest(ApiResponse.Fail(AppMessages.Auth.InvalidCredentials));
        }

        var (token, expiresAt) = _tokenService.GenerateToken(user);
        var userDto = new UserDto(user.Id, user.FullName, user.Email, user.Role, user.CreatedAt);

        _logger.LogInformation("User {Email} logged in successfully with role {Role}", user.Email, user.Role);

        return Ok(ApiResponse<AuthResponse>.Ok(
            new AuthResponse(token, expiresAt, userDto),
            AppMessages.Auth.LoginSuccess
        ));
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse.Fail(AppMessages.Validation.RequiredField, errors));
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        if (exists)
        {
            return BadRequest(ApiResponse.Fail(AppMessages.Auth.EmailAlreadyExists));
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            CreatedBy = request.Email
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var (token, expiresAt) = _tokenService.GenerateToken(user);
        var userDto = new UserDto(user.Id, user.FullName, user.Email, user.Role, user.CreatedAt);

        _logger.LogInformation("New user registered: {Email} ({Role})", user.Email, user.Role);

        return Ok(ApiResponse<AuthResponse>.Ok(
            new AuthResponse(token, expiresAt, userDto),
            AppMessages.Auth.RegisterSuccess
        ));
    }

    /// <summary>
    /// Retrieves current authenticated user profile.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized(ApiResponse.Fail(AppMessages.Auth.Unauthorized));
        }

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Auth.UserNotFound));
        }

        var userDto = new UserDto(user.Id, user.FullName, user.Email, user.Role, user.CreatedAt);
        return Ok(ApiResponse<UserDto>.Ok(userDto, AppMessages.General.FetchSuccess));
    }
}
