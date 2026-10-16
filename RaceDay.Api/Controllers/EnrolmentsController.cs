using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/enrolments")]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new UnauthorizedAccessException("No user id claim present."));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Cancel(int id)
    {
        var enrolment = await _db.EventEnrolments.FindAsync(id);
        if (enrolment is null)
            return NotFound(new { message = $"Enrolment {id} does not exist." });

        if (enrolment.UserId != CurrentUserId)
            return Forbid();

        _db.EventEnrolments.Remove(enrolment);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:int}/results")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CaptureResult(int id, [FromBody] CreateResultDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var enrolment = await _db.EventEnrolments
            .Include(ee => ee.Event)
            .Include(ee => ee.User)
            .Include(ee => ee.Result)
            .FirstOrDefaultAsync(ee => ee.EnrolmentId == id);

        if (enrolment is null)
            return NotFound(new { message = $"Enrolment {id} does not exist." });

        if (enrolment.Result is not null)
            return Conflict(new { message = "A result has already been captured for this enrolment." });

        var result = new Result
        {
            EnrolmentId = enrolment.EnrolmentId,
            FinishTime = dto.FinishTime,
            Position = dto.Position
        };

        _db.Results.Add(result);
        await _db.SaveChangesAsync();

        return Created($"/api/enrolments/{id}/results", new ResultDto
        {
            ResultId = result.ResultId,
            EnrolmentId = result.EnrolmentId,
            EventId = enrolment.EventId,
            EventName = enrolment.Event!.EventName,
            UserId = enrolment.UserId,
            ParticipantName = enrolment.User!.FullName,
            FinishTime = result.FinishTime,
            Position = result.Position
        });
    }
}