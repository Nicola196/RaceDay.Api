using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IWeatherService _weatherService;

    public EventsController(RaceDayDbContext db, IWeatherService weatherService)
    {
        _db = db;
        _weatherService = weatherService;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? throw new UnauthorizedAccessException("No user id claim present."));

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var events = await _db.Events
            .Include(e => e.Category)
            .OrderBy(e => e.EventDate)
            .Select(e => new EventDto
            {
                EventId = e.EventId,
                EventName = e.EventName,
                EventDescription = e.EventDescription,
                EventDate = e.EventDate,
                Location = e.Location,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                Distance = e.Distance,
                EventType = e.EventType,
                CategoryId = e.CategoryId,
                CategoryName = e.Category!.CategoryName,
                EnrolmentCount = e.EventEnrolments.Count
            })
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _db.Events
            .Include(e => e.Category)
            .Where(e => e.EventId == id)
            .Select(e => new EventDto
            {
                EventId = e.EventId,
                EventName = e.EventName,
                EventDescription = e.EventDescription,
                EventDate = e.EventDate,
                Location = e.Location,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                Distance = e.Distance,
                EventType = e.EventType,
                CategoryId = e.CategoryId,
                CategoryName = e.Category!.CategoryName,
                EnrolmentCount = e.EventEnrolments.Count
            })
            .FirstOrDefaultAsync();

        if (dto is null)
            return NotFound(new { message = $"Event {id} does not exist." });

        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!await _db.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId))
            return BadRequest(new { message = $"Category {dto.CategoryId} does not exist." });

        var newEvent = new Event
        {
            EventName = dto.EventName.Trim(),
            EventDescription = dto.EventDescription,
            EventDate = dto.EventDate,
            Location = dto.Location.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Distance = dto.Distance,
            EventType = dto.EventType,
            CategoryId = dto.CategoryId
        };

        _db.Events.Add(newEvent);
        await _db.SaveChangesAsync();

        var category = await _db.Categories.FindAsync(newEvent.CategoryId);

        return CreatedAtAction(nameof(GetById), new { id = newEvent.EventId }, new EventDto
        {
            EventId = newEvent.EventId,
            EventName = newEvent.EventName,
            EventDescription = newEvent.EventDescription,
            EventDate = newEvent.EventDate,
            Location = newEvent.Location,
            Latitude = newEvent.Latitude,
            Longitude = newEvent.Longitude,
            Distance = newEvent.Distance,
            EventType = newEvent.EventType,
            CategoryId = newEvent.CategoryId,
            CategoryName = category?.CategoryName ?? string.Empty,
            EnrolmentCount = 0
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _db.Events.FindAsync(id);
        if (existing is null)
            return NotFound(new { message = $"Event {id} does not exist." });

        if (!await _db.Categories.AnyAsync(c => c.CategoryId == dto.CategoryId))
            return BadRequest(new { message = $"Category {dto.CategoryId} does not exist." });

        existing.EventName = dto.EventName.Trim();
        existing.EventDescription = dto.EventDescription;
        existing.EventDate = dto.EventDate;
        existing.Location = dto.Location.Trim();
        existing.Latitude = dto.Latitude;
        existing.Longitude = dto.Longitude;
        existing.Distance = dto.Distance;
        existing.EventType = dto.EventType;
        existing.CategoryId = dto.CategoryId;

        await _db.SaveChangesAsync();

        var category = await _db.Categories.FindAsync(existing.CategoryId);
        var enrolmentCount = await _db.EventEnrolments.CountAsync(ee => ee.EventId == id);

        return Ok(new EventDto
        {
            EventId = existing.EventId,
            EventName = existing.EventName,
            EventDescription = existing.EventDescription,
            EventDate = existing.EventDate,
            Location = existing.Location,
            Latitude = existing.Latitude,
            Longitude = existing.Longitude,
            Distance = existing.Distance,
            EventType = existing.EventType,
            CategoryId = existing.CategoryId,
            CategoryName = category?.CategoryName ?? string.Empty,
            EnrolmentCount = enrolmentCount
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Events.FindAsync(id);
        if (existing is null)
            return NotFound(new { message = $"Event {id} does not exist." });

        _db.Events.Remove(existing);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id:int}/location")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLocation(int id)
    {
        var location = await _db.Events
            .Where(e => e.EventId == id)
            .Select(e => new
            {
                e.EventId,
                e.EventName,
                e.Location,
                e.Latitude,
                e.Longitude
            })
            .FirstOrDefaultAsync();

        if (location is null)
            return NotFound(new { message = $"Event {id} does not exist." });

        return Ok(location);
    }

    [HttpGet("{id:int}/weather")]
    [AllowAnonymous]
    public async Task<IActionResult> GetWeather(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null)
            return NotFound(new { message = $"Event {id} does not exist." });

        var forecast = await _weatherService.GetForecastAsync(ev.Latitude, ev.Longitude, ev.EventDate);
        return Ok(forecast);
    }

    [HttpPost("{id:int}/enrol")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enrol(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null)
            return NotFound(new { message = $"Event {id} does not exist." });

        var alreadyEnrolled = await _db.EventEnrolments
            .AnyAsync(ee => ee.EventId == id && ee.UserId == CurrentUserId);

        if (alreadyEnrolled)
            return Conflict(new { message = "You are already enrolled in this event." });

        var enrolment = new EventEnrolment
        {
            UserId = CurrentUserId,
            EventId = id,
            EnrolmentDate = DateTime.UtcNow
        };

        _db.EventEnrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return Created($"/api/enrolments/{enrolment.EnrolmentId}", new EnrolmentDto
        {
            EnrolmentId = enrolment.EnrolmentId,
            EventId = ev.EventId,
            EventName = ev.EventName,
            EventDate = ev.EventDate,
            Location = ev.Location,
            EventType = ev.EventType,
            EnrolmentDate = enrolment.EnrolmentDate,
            HasResult = false
        });
    }

    [HttpGet("{id:int}/enrolments")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetEnrolments(int id)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == id))
            return NotFound(new { message = $"Event {id} does not exist." });

        var enrolments = await _db.EventEnrolments
            .Include(ee => ee.User)
            .Where(ee => ee.EventId == id)
            .OrderBy(ee => ee.EnrolmentDate)
            .Select(ee => new EnrolledUserDto
            {
                EnrolmentId = ee.EnrolmentId,
                UserId = ee.UserId,
                FullName = ee.User!.FullName,
                Email = ee.User.Email,
                EnrolmentDate = ee.EnrolmentDate
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    [HttpGet("{id:int}/results")]
    [AllowAnonymous]
    public async Task<IActionResult> GetResults(int id)
    {
        if (!await _db.Events.AnyAsync(e => e.EventId == id))
            return NotFound(new { message = $"Event {id} does not exist." });

        var results = await _db.Results
            .Include(r => r.EventEnrolment)!
                .ThenInclude(ee => ee!.User)
            .Include(r => r.EventEnrolment)!
                .ThenInclude(ee => ee!.Event)
            .Where(r => r.EventEnrolment!.EventId == id)
            .OrderBy(r => r.Position)
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