using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Services;
using System.Text.Json;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : BaseController
    {
        private readonly ApiService _api;

        public OrganiserController(ApiService api) => _api = api;

        public async Task<IActionResult> Dashboard()
        {
            var response = await _api.GetAsync("/api/events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Could not load events from the API.";
                return View(new List<EventDto>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<List<EventDto>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(events ?? new List<EventDto>());
        }

        [HttpGet]
        public IActionResult CreateEvent() => View();

        [HttpPost]
        public async Task<IActionResult> CreateEvent(string eventName, string eventDescription, DateTime eventDate, string eventLocation, string eventDistance, string eventType, IFormFile eventImage)
        {
            string imageUrl = "";

            // 1. Try to upload the image first
            if (eventImage != null && eventImage.Length > 0)
            {
                var imgResponse = await _api.PostFileAsync("api/images/upload", eventImage);

                if (imgResponse.IsSuccessStatusCode)
                {
                    var imgResult = JsonDocument.Parse(await imgResponse.Content.ReadAsStringAsync()).RootElement;
                    imageUrl = imgResult.GetProperty("imageUrl").GetString() ?? "";
                }
                else
                {
                    var imgError = await imgResponse.Content.ReadAsStringAsync();
                    ViewBag.Error = $"Image Upload Failed: {imgResponse.StatusCode} - {imgError}";
                    return View();
                }
            }

            // 2. Try to create the event
            // We map the fields to exactly what the API expects based on the validation errors
            var eventData = new
            {
                EventName = eventName,
                EventDescription = eventDescription,
                EventDate = eventDate,
                Location = eventLocation,
                DistanceKm = 10.0,
                EventType = eventType,
                RouteStartPoint = "Start",
                RouteEndPoint = "End",
                RouteMapUrl = imageUrl,
                CategoryId = 1
            };

            var response = await _api.PostAsync("/api/events", eventData);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Dashboard");

            // 3. If it fails, show the EXACT error from the API
            var errorContent = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"API Error: {response.StatusCode} - {errorContent}";
            return View();
        }
    }
}