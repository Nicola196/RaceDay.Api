using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services; // <-- THIS IS THE MISSING LINE

namespace RaceDay.Mvc.Controllers;

public class ParticipantController : BaseController
{
    private readonly ApiService _api;

    public ParticipantController(ApiService api) => _api = api;

    public async Task<IActionResult> Index()
    {
        // Call the API to get all events
        var response = await _api.GetAsync("/api/events");

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Could not load events from the API.";
            return View(new List<EventDto>());
        }

        var json = await response.Content.ReadAsStringAsync();
        var events = System.Text.Json.JsonSerializer.Deserialize<List<EventDto>>(json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return View(events ?? new List<EventDto>());
    }
}

// A simple model to catch the API data
public class EventDto
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Distance { get; set; }
    public int EnrolmentCount { get; set; }
}