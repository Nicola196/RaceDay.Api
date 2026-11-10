using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Models;
using RaceDayApi.Security;

namespace RaceDayApi.Controllers;

[ApiController]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Enrols the logged-in Member in an event.</summary>
    /// <remarks>No body needed. The enrolment links the member, the event and the event's category.</remarks>
    /// <response code="201">Enrolment created.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Only Members can enrol.</response>
    /// <response code="404">Event does not exist.</response>
    /// <response code="409">Already enrolled in this event.</response>
    [HttpPost("api/events/{eventId:int}/enrol")]
    [SessionAuthorize("Member")]
    [ProducesResponseType(typeof(EnrolmentResponse), 201)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Enrol(int eventId)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;

        if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
        {
            return NotFound(new { message = "Event not found." });
        }

        if (await _db.EventEnrolments.AnyAsync(x => x.EventId == eventId && x.UserId == userId))
        {
            return Conflict(new { message = "You are already enrolled in this event." });
        }

        var enrolment = new EventEnrolment
        {
            UserId = userId,
            EventId = eventId,
            EnrolmentDate = DateTime.UtcNow
        };

        _db.EventEnrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        var response = await _db.EventEnrolments
            .Where(x => x.EnrolmentId == enrolment.EnrolmentId)
            .Select(EnrolmentResponse.Projection)
            .FirstAsync();

        return StatusCode(201, response);
    }

    /// <summary>Lists all events the logged-in Member has enrolled in.</summary>
    /// <response code="200">Array of enrolments.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Only Members have enrolments.</response>
    [HttpGet("api/users/me/enrolments")]
    [SessionAuthorize("Member")]
    [ProducesResponseType(typeof(List<EnrolmentResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<List<EnrolmentResponse>>> GetMyEnrolments()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;

        return await _db.EventEnrolments
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Event.EventDate)
            .Select(EnrolmentResponse.Projection)
            .ToListAsync();
    }

    /// <summary>Cancels the logged-in Member's own enrolment.</summary>
    /// <response code="204">Enrolment cancelled.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not your enrolment, or you are not a Member.</response>
    /// <response code="404">Enrolment does not exist.</response>
    [HttpDelete("api/enrolments/{id:int}")]
    [SessionAuthorize("Member")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var userId = HttpContext.Session.GetUserId()!.Value;

        var enrolment = await _db.EventEnrolments.FindAsync(id);
        if (enrolment is null)
        {
            return NotFound(new { message = "Enrolment not found." });
        }

        if (enrolment.UserId != userId)
        {
            return StatusCode(403, new { message = "This is not your enrolment." });
        }

        _db.EventEnrolments.Remove(enrolment);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Lists everyone enrolled in an event (Organiser only).</summary>
    /// <response code="200">Array of enrolments including the member's name and email.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpGet("api/events/{eventId:int}/enrolments")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(typeof(List<EnrolmentResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
        {
            return NotFound(new { message = "Event not found." });
        }

        var list = await _db.EventEnrolments
            .Where(x => x.EventId == eventId)
            .OrderBy(x => x.User.FullName)
            .Select(EnrolmentResponse.Projection)
            .ToListAsync();

        return Ok(list);
    }
}