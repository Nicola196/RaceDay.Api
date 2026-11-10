using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Models;
using RaceDayApi.Security;

namespace RaceDayApi.Controllers;

[ApiController]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ResultsController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Captures the finish time and position for an enrolment (Organiser only).</summary>
    /// <remarks>Expects: finishTime (hh:mm:ss, e.g. 01:45:30) and position (1 or higher).</remarks>
    /// <response code="201">Result recorded.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser.</response>
    /// <response code="404">Enrolment does not exist.</response>
    /// <response code="409">A result was already captured for this enrolment.</response>
    [HttpPost("api/enrolments/{enrolmentId:int}/results")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(typeof(ResultResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> CaptureResult(int enrolmentId, ResultRequest request)
    {
        if (!await _db.EventEnrolments.AnyAsync(x => x.EnrolmentId == enrolmentId))
        {
            return NotFound(new { message = "Enrolment not found." });
        }

        if (await _db.Results.AnyAsync(r => r.EnrolmentId == enrolmentId))
        {
            return Conflict(new { message = "A result has already been captured for this enrolment." });
        }

        var result = new RaceResult
        {
            EnrolmentId = enrolmentId,
            FinishTime = request.FinishTime,
            Position = request.Position
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        var response = await _db.Results
            .Where(r => r.ResultId == result.ResultId)
            .Select(ResultResponse.Projection)
            .FirstAsync();

        return StatusCode(201, response);
    }

    /// <summary>Returns the leaderboard for an event, ordered by position (public).</summary>
    /// <response code="200">Array of results.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpGet("api/events/{eventId:int}/results")]
    [ProducesResponseType(typeof(List<ResultResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEventResults(int eventId)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == eventId))
        {
            return NotFound(new { message = "Event not found." });
        }

        var results = await _db.Results
            .Where(r => r.Enrolment.EventId == eventId)
            .OrderBy(r => r.Position)
            .Select(ResultResponse.Projection)
            .ToListAsync();

        return Ok(results);
    }

    /// <summary>Lists the logged-in Member's own race history and results.</summary>
    /// <response code="200">Array of the member's results.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Only Members have results.</response>
    [HttpGet("api/users/me/results")]
    [SessionAuthorize("Member")]
    [ProducesResponseType(typeof(List<ResultResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<List<ResultResponse>>> GetMyResults()
    {
        var userId = HttpContext.Session.GetUserId()!.Value;

        return await _db.Results
            .Where(r => r.Enrolment.UserId == userId)
            .OrderByDescending(r => r.Enrolment.Event.EventDate)
            .Select(ResultResponse.Projection)
            .ToListAsync();
    }
}