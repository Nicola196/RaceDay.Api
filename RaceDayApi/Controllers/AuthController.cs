using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Models;
using RaceDayApi.Security;

namespace RaceDayApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public AuthController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Registers a new user account.</summary>
    /// <remarks>
    /// Expects: fullName, email, password (min 6 characters) and roleId (1 = Organiser, 2 = Member).
    /// The password is hashed with BCrypt and never stored in its original form.
    /// </remarks>
    /// <response code="201">Account created. Returns the new user (no password).</response>
    /// <response code="400">Missing or invalid fields, or unknown roleId.</response>
    /// <response code="409">Email is already registered.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var role = await _db.Roles.FindAsync(request.RoleId);
        if (role is null)
        {
            return BadRequest(new { message = "Invalid roleId. Use 1 for Organiser or 2 for Member." });
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new { message = "This email is already registered." });
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = role.RoleId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return StatusCode(201, new UserResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = role.RoleName
        });
    }

    /// <summary>Logs a user in and starts a server-side session.</summary>
    /// <remarks>
    /// Expects: email and password. On success the server stores the user's ID and role
    /// in the session and sends back a session cookie that Swagger/Postman reuses automatically.
    /// </remarks>
    /// <response code="200">Login successful. Returns user info.</response>
    /// <response code="401">Wrong email or password.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Wrong email or password." });
        }

        HttpContext.Session.Clear();
        HttpContext.Session.SetInt32(SessionKeys.UserId, user.UserId);
        HttpContext.Session.SetString(SessionKeys.Role, user.Role.RoleName);

        return Ok(new UserResponse
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.RoleName
        });
    }

    /// <summary>Logs the current user out and clears the session.</summary>
    /// <response code="200">Logged out.</response>
    [HttpPost("logout")]
    [ProducesResponseType(200)]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out." });
    }
}