using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Server.Common;
using TaskManagement.Server.Data;
using TaskManagement.Server.DTOs;

namespace TaskManagement.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all active users with their assigned roles.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers([FromQuery] UserRole? role = null)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        var users = await query
            .OrderBy(u => u.FullName)
            .Select(u => new UserDto(u.Id, u.FullName, u.Email, u.Role, u.CreatedAt))
            .ToListAsync();

        return Ok(ApiResponse<List<UserDto>>.Ok(users, AppMessages.General.FetchSuccess));
    }

    /// <summary>
    /// Gets details of a specific user.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Auth.UserNotFound));
        }

        var userDto = new UserDto(user.Id, user.FullName, user.Email, user.Role, user.CreatedAt);
        return Ok(ApiResponse<UserDto>.Ok(userDto, AppMessages.General.FetchSuccess));
    }
}
