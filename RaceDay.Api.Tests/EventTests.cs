using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace RaceDay.Api.Tests;

public class EventTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;

    public EventTests(RaceDayApiFactory factory) => _factory = factory;

    private static object BuildValidEvent() => new
    {
        eventName = "Unit Test Race",
        eventDescription = "Created by the test suite.",
        eventDate = DateTime.UtcNow.AddDays(20),
        location = "Test Location",
        latitude = -26.2041,
        longitude = 28.0473,
        distance = "10km",
        eventType = "Run",
        categoryId = 3
    };

    [Fact]
    public async Task GetEvents_WithoutToken_Returns200_PublicEndpoint()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/events");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateEvent_AsParticipant_Returns403()
    {
        var client = await TestHelpers.CreateParticipantClientAsync(_factory);

        var response = await client.PostAsJsonAsync("/api/events", BuildValidEvent());

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateEvent_AsOrganiser_Returns201()
    {
        var client = await TestHelpers.CreateOrganiserClientAsync(_factory);

        var response = await client.PostasJsonAsync("/api/events", BuildValidEvent());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateEvent_WithInvalidEventType_Returns400()
    {
        var client = await TestHelpers.CreateOrganiserClientAsync(_factory);

        var badEvent = new
        {
            eventName = "Bad Type",
            eventDate = DateTime.UtcNow.AddDays(10),
            location = "Somewhere",
            latitude = -26.2,
            longitude = 28.0,
            eventType = "Swim",
            categoryId = 3
        };

        var response = await client.PostAsJsonAsync("/api/events", badEvent);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetEventById_WithUnknownId_Returns404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/events/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEventWeather_Returns200_FromFakeProvider()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/events/1/weather");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}