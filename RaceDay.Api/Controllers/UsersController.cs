using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public UsersController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new UnauthorizedAccessException("No user id claim present."));

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

        if (user is null)
            throw new KeyNotFoundException("User account no longer exists.");

        return Ok(new UserProfileDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = user.Role?.RoleName ?? string.Empty
        });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

        if (user is null)
            throw new KeyNotFoundException("User account no longer exists.");

        var newEmail = dto.Email.Trim().ToLowerInvariant();

        var emailTaken = await _db.Users
            .AnyAsync(u => u.Email == newEmail && u.UserId != CurrentUserId);

        if (emailTaken)
            return Conflict(new { message = "That email address is already in use." });

        user.FullName = dto.FullName.Trim();
        user.Email = newEmail;

        await _db.SaveChangesAsync();

        return Ok(new UserProfileDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            RoleName = user.Role?.RoleName ?? string.Empty
        });
    }

    [HttpGet]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _db.Users
            .Include(u => u.Role)
            .OrderBy(u => u.UserId)
            .Select(u => new UserProfileDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                RoleName = u.Role!.RoleName
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("me/enrolments")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        var enrolments = await _db.EventEnrolments
            .Include(ee => ee.Event)
            .Include(ee => ee.Result)
            .Where(ee => ee.UserId == CurrentUserId)
            .OrderBy(ee => ee.EnrolmentDate)
            .Select(ee => new EnrolmentDto
            {
                EnrolmentId = ee.EnrolmentId,
                EventId = ee.EventId,
                EventName = ee.Event!.EventName,
                EventDate = ee.Event.EventDate,
                Location = ee.Event.Location,
                EventType = ee.Event.EventType,
                EnrolmentDate = ee.EnrolmentDate,
                HasResult = ee.Result != null
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    [HttpGet("me/results")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyResults()
    {
        var results = await _db.Results
            .Include(r => r.EventEnrolment)!
                .ThenInclude(ee => ee!.Event)
            .Include(r => r.EventEnrolment)!
                .ThenInclude(ee => ee!.User)
            .Where(r => r.EventEnrolment!.UserId == CurrentUserId)
            .OrderByDescending(r => r.EventEnrolment!.Event!.EventDate)
            .Select(r => new ResultDto
            {
                ResultId = r.ResultId,
                EnrolmentId = r.EnrolmentId,
                EventId = r.EventEnrolment!.EventId,
                EventName = r.EventEnrolment.Event!.EventName,
                UserId = r.EventEnrolment.UserId,
                ParticipantName = r.EventEnrolment.User!.FullName,
                FinishTime = r.FinishTime,
                Position = r.Position
            })
            .ToListAsync();

        return Ok(results);
    }
}