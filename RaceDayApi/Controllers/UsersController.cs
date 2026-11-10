using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Security;

namespace RaceDayApi.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Returns the logged-in user's own profile.</summary>
    /// <response code="200">Name, email and role of the current user.</response>
    /// <response code="401">Not logged in.</response>
    [HttpGet("me")]
    [SessionAuthorize]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;

        var user = await _db.Users
            .Where(u => u.UserId == userId)
            .Select(u => new UserResponse
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.RoleName
            })
            .FirstOrDefaultAsync();

        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Updates the logged-in user's own profile.</summary>
    /// <remarks>Expects: fullName and email.</remarks>
    /// <response code="200">Returns the updated profile.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="409">That email belongs to another user.</response>
    [HttpPut("me")]
    [SessionAuthorize]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
        if (user is null)
        {
            return NotFound();
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email && u.UserId != userId))
        {
            return Conflict(new { message = "This email is used by another account." });
        }

        user.FullName = request.FullName.Trim();
        user.Email = email;
        await _db.SaveChangesAsync();

        return Ok(new UserResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.RoleName
        });
    }

    /// <summary>Lists all registered users (Organiser only).</summary>
    /// <response code="200">Array of users.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser.</response>
    [HttpGet]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(typeof(List<UserResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<List<UserResponse>>> GetAllUsers()
    {
        return await _db.Users
            .OrderBy(u => u.FullName)
            .Select(u => new UserResponse
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role.RoleName
            })
            .ToListAsync();
    }
}