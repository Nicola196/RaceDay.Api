using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace RaceDay.Api.Tests;

public class EnrolmentTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;

    public EnrolmentTests(RaceDayApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Enrol_AsParticipant_Returns201_AndIsRecorded()
    {
        var client = await TestHelpers.CreateParticipantClientAsync(_factory);

        var response = await client.PostAsync("/api/events/1/enrol", null);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var mine = await client.GetAsync("/api/users/me/enrolments");
        mine.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await mine.Content.ReadAsStringAsync();
        body.Should().Contain("Rosebank City Run");
    }

    [Fact]
    public async Task Enrol_Twice_Returns409()
    {
        var client = await TestHelpers.CreateParticipantClientAsync(_factory);

        var first = await client.PostAsync("/api/events/2/enrol", null);
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsync("/api/events/2/enrol", null);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Enrol_AsOrganiser_Returns403()
    {
        var client = await TestHelpers.CreateOrganiserClientAsync(_factory);

        var response = await client.PostAsync("/api/events/1/enrol", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Enrol_ToUnknownEvent_Returns404()
    {
        var client = await TestHelpers.CreateParticipantClientAsync(_factory);

        var response = await client.PostAsync("/api/events/999999/enrol", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetEventEnrolments_AsParticipant_Returns403()
    {
        var client = await TestHelpers.CreateParticipantClientAsync(_factory);

        var response = await client.GetAsync("/api/events/1/enrolments");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetEventEnrolments_AsOrganiser_Returns200()
    {
        var client = await TestHelpers.CreateOrganiserClientAsync(_factory);

        var response = await client.GetAsync("/api/events/1/enrolments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CaptureResult_AsOrganiser_Returns201_And_LeaderboardShowsIt()
    {
        var organiser = await TestHelpers.CreateOrganiserClientAsync(_factory);

        var enrolmentsResponse = await organiser.GetAsync("/api/events/1/enrolments");
        enrolmentsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var enrolments = await enrolmentsResponse.Content.ReadFromJsonAsync<List<EnrolmentLookup>>();
        enrolments.Should().NotBeNullOrEmpty();

        var enrolmentId = enrolments![0].EnrolmentId;

        var capture = await organiser.PostAsJsonAsync(
            $"/api/enrolments/{enrolmentId}/results",
            new { finishTime = "00:42:15", position = 1 });

        capture.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicate = await organiser.PostAsJsonAsync(
            $"/api/enrolments/{enrolmentId}/results",
            new { finishTime = "00:45:00", position = 2 });
        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var publicClient = _factory.CreateClient();
        var leaderboard = await publicClient.GetAsync("/api/events/1/results");
        leaderboard.StatusCode.Should().Be(HttpStatusCode.OK);
        (await leaderboard.Content.ReadAsStringAsync()).Should().Contain("00:42:15");
    }

    private class Enrolmentlookup
    {
        public int EnrolmentId { get; set; }
        public int UserId { get; set; }
    }
}