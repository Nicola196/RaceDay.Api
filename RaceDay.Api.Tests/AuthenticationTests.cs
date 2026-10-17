using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit; // <--- ADDED THIS LINE

namespace RaceDay.Api.Tests;

public class AuthenticationTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;

    public AuthenticationTests(RaceDayApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Test User",
            email = $"newuser_{Guid.NewGuid()}@raceday.com",
            password = "Password123!",
            roleId = 2
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var client = _factory.CreateClient();
        var email = $"dupe_{Guid.NewGuid()}@raceday.com";

        var first = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "First User",
            email,
            password = "Password123!",
            roleId = 2
        });
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Second User",
            email,
            password = "Password123!",
            roleId = 2
        });

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithInvalidRoleId_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Bad Role",
            email = $"badrole_{Guid.NewGuid()}@raceday.com",
            password = "Password123!",
            roleId = 999
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = TestHelpers.ParticipantEmail,
            password = TestHelpers.SeedPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("token").GetString().Should().NotBeNullOrWhiteSpace();
        json.GetProperty("roleName").GetString().Should().Be("Participant");
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = TestHelpers.ParticipantEmail,
            password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyProfile_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyProfile_WithToken_Returns200()
    {
        var client = await TestHelpers.CreateParticipantClientAsync(_factory);

        var response = await client.GetAsync("/api/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("email").GetString().Should().Be(TestHelpers.ParticipantEmail);
    }
}