using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDayApi.Tests;

public class EnrolmentTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public EnrolmentTests(RaceDayFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Member_CanEnrol_Returns201()
    {
        var organiser = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.OrganiserRoleId);
        var eventId = await TestHelpers.CreateEventAsync(organiser);
        var member = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.MemberRoleId);

        var response = await member.PostAsync($"/api/events/{eventId}/enrol", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Enrolment_IsRecordedAndVisibleToMember()
    {
        var organiser = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.OrganiserRoleId);
        var eventId = await TestHelpers.CreateEventAsync(organiser);
        var member = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.MemberRoleId);
        await member.PostAsync($"/api/events/{eventId}/enrol", null);

        var response = await member.GetAsync("/api/users/me/enrolments");
        var list = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, list.GetArrayLength());
        Assert.Equal(eventId, list[0].GetProperty("eventId").GetInt32());
    }

    [Fact]
    public async Task Enrolling_Twice_Returns409()
    {
        var organiser = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.OrganiserRoleId);
        var eventId = await TestHelpers.CreateEventAsync(organiser);
        var member = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.MemberRoleId);

        await member.PostAsync($"/api/events/{eventId}/enrol", null);
        var second = await member.PostAsync($"/api/events/{eventId}/enrol", null);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Enrolling_InMissingEvent_Returns404()
    {
        var member = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.MemberRoleId);

        var response = await member.PostAsync("/api/events/99999/enrol", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Enrolling_WithoutLogin_Returns401()
    {
        var anonymous = _factory.CreateClient();

        var response = await anonymous.PostAsync("/api/events/1/enrol", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanSeeEnrolmentsForTheirEvent()
    {
        var organiser = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.OrganiserRoleId);
        var eventId = await TestHelpers.CreateEventAsync(organiser);
        var member = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.MemberRoleId);
        await member.PostAsync($"/api/events/{eventId}/enrol", null);

        var response = await organiser.GetAsync($"/api/events/{eventId}/enrolments");
        var list = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, list.GetArrayLength());
    }

    [Fact]
    public async Task Organiser_CanCaptureResult_ThenDuplicateReturns409()
    {
        var organiser = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.OrganiserRoleId);
        var eventId = await TestHelpers.CreateEventAsync(organiser);
        var member = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.MemberRoleId);
        var enrol = await member.PostAsync($"/api/events/{eventId}/enrol", null);
        var enrolmentId = await TestHelpers.ReadIdAsync(enrol, "enrolmentId");

        var first = await organiser.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/results",
            new { finishTime = "00:52:10", position = 1 });
        var second = await organiser.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/results",
            new { finishTime = "00:50:00", position = 2 });
        var memberResults = await member.GetAsync("/api/users/me/results");

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal(HttpStatusCode.OK, memberResults.StatusCode);
    }

    [Fact]
    public async Task Member_CannotCaptureResult_Returns403()
    {
        var organiser = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.OrganiserRoleId);
        var eventId = await TestHelpers.CreateEventAsync(organiser);
        var member = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.MemberRoleId);
        var enrol = await member.PostAsync($"/api/events/{eventId}/enrol", null);
        var enrolmentId = await TestHelpers.ReadIdAsync(enrol, "enrolmentId");

        var response = await member.PostAsJsonAsync($"/api/enrolments/{enrolmentId}/results",
            new { finishTime = "00:52:10", position = 1 });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}