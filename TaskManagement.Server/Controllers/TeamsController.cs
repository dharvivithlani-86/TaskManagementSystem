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
public class TeamsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public TeamsController(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets all active teams.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TeamResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeams()
    {
        var teams = await _context.Teams
            .Include(t => t.LeadManager)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Tasks)
            .OrderBy(t => t.Name)
            .Select(t => new TeamResponseDto(
                t.Id,
                t.Name,
                t.Description,
                t.LeadManagerId,
                t.LeadManager.FullName,
                t.Members.Count,
                t.Tasks.Count(task => task.Status != TaskItemStatus.Done),
                t.Members.Select(m => new TeamMemberDto(
                    m.UserId,
                    m.User.FullName,
                    m.User.Email,
                    m.User.Role,
                    m.JoinedAt
                )).ToList(),
                t.CreatedAt
            ))
            .ToListAsync();

        return Ok(ApiResponse<List<TeamResponseDto>>.Ok(teams, AppMessages.General.FetchSuccess));
    }

    /// <summary>
    /// Gets details of a specific team.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<TeamResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeam(int id)
    {
        var team = await _context.Teams
            .Include(t => t.LeadManager)
            .Include(t => t.Members)
                .ThenInclude(m => m.User)
            .Include(t => t.Tasks)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Teams.NotFound));
        }

        var dto = new TeamResponseDto(
            team.Id,
            team.Name,
            team.Description,
            team.LeadManagerId,
            team.LeadManager.FullName,
            team.Members.Count,
            team.Tasks.Count(task => task.Status != TaskItemStatus.Done),
            team.Members.Select(m => new TeamMemberDto(
                m.UserId,
                m.User.FullName,
                m.User.Email,
                m.User.Role,
                m.JoinedAt
            )).ToList(),
            team.CreatedAt
        );

        return Ok(ApiResponse<TeamResponseDto>.Ok(dto, AppMessages.General.FetchSuccess));
    }

    /// <summary>
    /// Creates a new team (Admin or Manager).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<TeamResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTeam([FromBody] CreateTeamRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse.Fail(AppMessages.Validation.RequiredField, errors));
        }

        var currentUserId = _currentUserService.UserId!.Value;
        var currentUserRole = _currentUserService.Role!.Value;

        // If lead manager is not specified, default to current user if manager, else require valid manager id
        int leadManagerId = request.LeadManagerId ?? currentUserId;
        var leadUser = await _context.Users.FindAsync(leadManagerId);
        if (leadUser == null || (leadUser.Role != UserRole.Manager && leadUser.Role != UserRole.Admin))
        {
            return BadRequest(ApiResponse.Fail(AppMessages.Teams.ManagerRequired));
        }

        var team = new Team
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            LeadManagerId = leadManagerId
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        // Also add the lead manager as a team member automatically
        var leadMembership = new TeamMember
        {
            TeamId = team.Id,
            UserId = leadManagerId,
            JoinedAt = DateTime.UtcNow
        };
        _context.TeamMembers.Add(leadMembership);
        await _context.SaveChangesAsync();

        var responseDto = new TeamResponseDto(
            team.Id,
            team.Name,
            team.Description,
            leadUser.Id,
            leadUser.FullName,
            1,
            0,
            new List<TeamMemberDto>
            {
                new(leadUser.Id, leadUser.FullName, leadUser.Email, leadUser.Role, leadMembership.JoinedAt)
            },
            team.CreatedAt
        );

        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, ApiResponse<TeamResponseDto>.Ok(responseDto, AppMessages.Teams.Created));
    }

    /// <summary>
    /// Updates team details (Admin or Team Lead Manager).
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<TeamResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTeam(int id, [FromBody] UpdateTeamRequest request)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Teams.NotFound));
        }

        var currentUserId = _currentUserService.UserId!.Value;
        var role = _currentUserService.Role!.Value;

        if (role != UserRole.Admin && team.LeadManagerId != currentUserId)
        {
            return Forbid();
        }

        var leadUser = await _context.Users.FindAsync(request.LeadManagerId);
        if (leadUser == null || (leadUser.Role != UserRole.Manager && leadUser.Role != UserRole.Admin))
        {
            return BadRequest(ApiResponse.Fail(AppMessages.Teams.ManagerRequired));
        }

        team.Name = request.Name.Trim();
        team.Description = request.Description?.Trim();
        team.LeadManagerId = request.LeadManagerId;

        await _context.SaveChangesAsync();

        return await GetTeam(id);
    }

    /// <summary>
    /// Adds a user as a member of the team.
    /// </summary>
    [HttpPost("{id}/members")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddMember(int id, [FromBody] AddTeamMemberRequest request)
    {
        var team = await _context.Teams.Include(t => t.Members).FirstOrDefaultAsync(t => t.Id == id);
        if (team == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Teams.NotFound));
        }

        var currentUserId = _currentUserService.UserId!.Value;
        var role = _currentUserService.Role!.Value;
        if (role != UserRole.Admin && team.LeadManagerId != currentUserId)
        {
            return Forbid();
        }

        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Auth.UserNotFound));
        }

        if (team.Members.Any(m => m.UserId == request.UserId))
        {
            return BadRequest(ApiResponse.Fail(AppMessages.Teams.MemberAlreadyExists));
        }

        var membership = new TeamMember
        {
            TeamId = id,
            UserId = request.UserId,
            JoinedAt = DateTime.UtcNow
        };

        _context.TeamMembers.Add(membership);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok(AppMessages.Teams.MemberAdded));
    }

    /// <summary>
    /// Removes a user from the team.
    /// </summary>
    [HttpDelete("{id}/members/{userId}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveMember(int id, int userId)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Teams.NotFound));
        }

        var currentUserId = _currentUserService.UserId!.Value;
        var role = _currentUserService.Role!.Value;
        if (role != UserRole.Admin && team.LeadManagerId != currentUserId)
        {
            return Forbid();
        }

        var membership = await _context.TeamMembers
            .FirstOrDefaultAsync(m => m.TeamId == id && m.UserId == userId);

        if (membership == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Teams.MemberNotFound));
        }

        _context.TeamMembers.Remove(membership);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok(AppMessages.Teams.MemberRemoved));
    }

    /// <summary>
    /// Soft deletes a team (Admin only).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var team = await _context.Teams.Include(t => t.Tasks).FirstOrDefaultAsync(t => t.Id == id);
        if (team == null)
        {
            return NotFound(ApiResponse.Fail(AppMessages.Teams.NotFound));
        }

        if (team.Tasks.Any(t => t.Status != TaskItemStatus.Done))
        {
            return BadRequest(ApiResponse.Fail(AppMessages.Teams.CannotDeleteTeamWithTasks));
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse.Ok(AppMessages.Teams.Deleted));
    }
}
