using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDayApi.Tests;

public class EventAndRoleTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public EventAndRoleTests(RaceDayFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Organiser_CanCreateEvent_Returns201()
    {
        var organiser = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.OrganiserRoleId);
        var categoryId = await TestHelpers.CreateCategoryAsync(organiser);

        var response = await organiser.PostAsJsonAsync("/api/events", TestHelpers.EventBody(categoryId));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CanUpdateAndDeleteEvent()
    {
        var organiser = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.OrganiserRoleId);
        var categoryId = await TestHelpers.CreateCategoryAsync(organiser);
        var create = await organiser.PostAsJsonAsync("/api/events", TestHelpers.EventBody(categoryId));
        var eventId = await TestHelpers.ReadIdAsync(create, "eventId");

        var update = await organiser.PutAsJsonAsync($"/api/events/{eventId}",
            TestHelpers.EventBody(categoryId, "Renamed Run"));
        var delete = await organiser.DeleteAsync($"/api/events/{eventId}");
        var afterDelete = await organiser.GetAsync($"/api/events/{eventId}");

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithInvalidCategory_Returns400()
    {
        var organiser = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.OrganiserRoleId);

        var response = await organiser.PostAsJsonAsync("/api/events", TestHelpers.EventBody(categoryId: 9999));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetEvent_ThatDoesNotExist_Returns404()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/events/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Member_CannotCreateEvent_Returns403()
    {
        var organiser = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.OrganiserRoleId);
        var categoryId = await TestHelpers.CreateCategoryAsync(organiser);
        var member = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.MemberRoleId);

        var response = await member.PostAsJsonAsync("/api/events", TestHelpers.EventBody(categoryId));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Member_CannotCreateCategory_Returns403()
    {
        var member = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.MemberRoleId);

        var response = await member.PostAsJsonAsync("/api/categories", new { categoryName = "Hacked" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_CannotCreateEvent_Returns401()
    {
        var anonymous = _factory.CreateClient();

        var response = await anonymous.PostAsJsonAsync("/api/events", TestHelpers.EventBody(1));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Member_CannotViewEventEnrolments_Returns403()
    {
        var organiser = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.OrganiserRoleId);
        var eventId = await TestHelpers.CreateEventAsync(organiser);
        var member = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.MemberRoleId);

        var response = await member.GetAsync($"/api/events/{eventId}/enrolments");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotEnrol_Returns403()
    {
        var organiser = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.OrganiserRoleId);
        var eventId = await TestHelpers.CreateEventAsync(organiser);

        var response = await organiser.PostAsync($"/api/events/{eventId}/enrol", null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Member_CannotListAllUsers_Returns403()
    {
        var member = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.MemberRoleId);

        var response = await member.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}