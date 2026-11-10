using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Models;
using RaceDayApi.Security;

namespace RaceDayApi.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;

    public EventsController(RaceDayDbContext db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>Lists events (public), optionally filtered by category.</summary>
    /// <remarks>
    /// Optional query parameters: categoryId (only events in that category) and
    /// upcomingOnly (true = hide events whose date has passed). Results are ordered by date.
    /// </remarks>
    /// <response code="200">Array of events.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<EventResponse>), 200)]
    public async Task<ActionResult<List<EventResponse>>> GetEvents(
        [FromQuery] int? categoryId, [FromQuery] bool upcomingOnly = false)
    {
        var query = _db.Events.AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(e => e.CategoryId == categoryId.Value);
        }

        if (upcomingOnly)
        {
            query = query.Where(e => e.EventDate >= DateTime.UtcNow);
        }

        return await query
            .OrderBy(e => e.EventDate)
            .Select(EventResponse.Projection)
            .ToListAsync();
    }

    /// <summary>Returns full details of one event (public).</summary>
    /// <response code="200">Event details.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetEvent(int id)
    {
        var ev = await _db.Events
            .Where(e => e.EventId == id)
            .Select(EventResponse.Projection)
            .FirstOrDefaultAsync();

        return ev is null ? NotFound(new { message = "Event not found." }) : Ok(ev);
    }

    /// <summary>Creates a new event (Organiser only).</summary>
    /// <remarks>
    /// Expects: eventName, eventDescription, eventDate, location, distanceKm,
    /// eventType (Run, Walk or Cycle), latitude, longitude, optional mapUrl and categoryId.
    /// </remarks>
    /// <response code="201">Event created.</response>
    /// <response code="400">Invalid data or categoryId does not exist.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser.</response>
    [HttpPost]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(typeof(EventResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateEvent(EventRequest request)
    {
        if (!await _db.Categories.AnyAsync(c => c.CategoryId == request.CategoryId))
        {
            return BadRequest(new { message = "The selected category does not exist." });
        }

        var ev = new Event
        {
            EventName = request.EventName.Trim(),
            EventDescription = request.EventDescription.Trim(),
            EventDate = request.EventDate,
            Location = request.Location.Trim(),
            DistanceKm = request.DistanceKm,
            EventType = request.EventType,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            MapUrl = request.MapUrl,
            CategoryId = request.CategoryId
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        var response = await _db.Events
            .Where(e => e.EventId == ev.EventId)
            .Select(EventResponse.Projection)
            .FirstAsync();

        return CreatedAtAction(nameof(GetEvent), new { id = ev.EventId }, response);
    }

    /// <summary>Updates an existing event (Organiser only).</summary>
    /// <remarks>Expects the same fields as creating an event.</remarks>
    /// <response code="200">Returns the updated event.</response>
    /// <response code="400">Invalid data or categoryId does not exist.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpPut("{id:int}")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(typeof(EventResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateEvent(int id, EventRequest request)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        if (!await _db.Categories.AnyAsync(c => c.CategoryId == request.CategoryId))
        {
            return BadRequest(new { message = "The selected category does not exist." });
        }

        ev.EventName = request.EventName.Trim();
        ev.EventDescription = request.EventDescription.Trim();
        ev.EventDate = request.EventDate;
        ev.Location = request.Location.Trim();
        ev.DistanceKm = request.DistanceKm;
        ev.EventType = request.EventType;
        ev.Latitude = request.Latitude;
        ev.Longitude = request.Longitude;
        ev.MapUrl = request.MapUrl;
        ev.CategoryId = request.CategoryId;
        await _db.SaveChangesAsync();

        var response = await _db.Events
            .Where(e => e.EventId == id)
            .Select(EventResponse.Projection)
            .FirstAsync();

        return Ok(response);
    }

    /// <summary>Deletes an event and its enrolments (Organiser only).</summary>
    /// <response code="204">Deleted.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpDelete("{id:int}")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Returns an event's location for map display (public).</summary>
    /// <response code="200">Location name, latitude, longitude and optional map link.</response>
    /// <response code="404">Event does not exist.</response>
    [HttpGet("{id:int}/location")]
    [ProducesResponseType(typeof(LocationResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetLocation(int id)
    {
        var location = await _db.Events
            .Where(e => e.EventId == id)
            .Select(e => new LocationResponse
            {
                EventId = e.EventId,
                EventName = e.EventName,
                Location = e.Location,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                MapUrl = e.MapUrl
            })
            .FirstOrDefaultAsync();

        return location is null ? NotFound(new { message = "Event not found." }) : Ok(location);
    }

    /// <summary>Returns the weather forecast for the event's location and date (public).</summary>
    /// <remarks>
    /// The server calls the external Open-Meteo service using the event's stored latitude/longitude.
    /// Forecasts only exist for events happening within the next 16 days.
    /// </remarks>
    /// <response code="200">Temperature, rain chance, wind and conditions.</response>
    /// <response code="400">Event date is in the past or more than 16 days away.</response>
    /// <response code="404">Event does not exist.</response>
    /// <response code="502">The weather provider is unavailable.</response>
    [HttpGet("{id:int}/weather")]
    [ProducesResponseType(typeof(WeatherResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(502)]
    public async Task<IActionResult> GetWeather(int id)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev is null)
        {
            return NotFound(new { message = "Event not found." });
        }

        var date = DateOnly.FromDateTime(ev.EventDate);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (date < today || date > today.AddDays(15))
        {
            return BadRequest(new { message = "Forecasts are only available for events within the next 16 days." });
        }

        var url = FormattableString.Invariant($"https://api.open-meteo.com/v1/forecast?latitude={ev.Latitude}&longitude={ev.Longitude}&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max,wind_speed_10m_max&timezone=auto&start_date={date:yyyy-MM-dd}&end_date={date:yyyy-MM-dd}");
        try
        {
            var client = _httpClientFactory.CreateClient();
            using var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(502, new { message = "The weather provider is unavailable." });
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var daily = doc.RootElement.GetProperty("daily");

            double? Read(string name)
            {
                var value = daily.GetProperty(name)[0];
                return value.ValueKind == JsonValueKind.Number ? value.GetDouble() : null;
            }

            return Ok(new WeatherResponse
            {
                EventId = ev.EventId,
                EventName = ev.EventName,
                Date = date.ToString("yyyy-MM-dd"),
                Conditions = DescribeWeather((int)(Read("weather_code") ?? 0)),
                MaxTempC = Read("temperature_2m_max"),
                MinTempC = Read("temperature_2m_min"),
                RainChancePercent = Read("precipitation_probability_max"),
                MaxWindKmh = Read("wind_speed_10m_max")
            });
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException
                                      or JsonException or KeyNotFoundException)
        {
            return StatusCode(502, new { message = "The weather provider is unavailable." });
        }
    }

    private static string DescribeWeather(int code) => code switch
    {
        0 => "Clear sky",
        1 or 2 or 3 => "Partly cloudy",
        45 or 48 => "Fog",
        >= 51 and <= 57 => "Drizzle",
        >= 61 and <= 67 => "Rain",
        >= 71 and <= 77 => "Snow",
        >= 80 and <= 82 => "Rain showers",
        >= 95 and <= 99 => "Thunderstorm",
        _ => "Unknown"
    };
}