using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDayApi.Tests;

public class AuthTests : IClassFixture<RaceDayFactory>
{
    private readonly RaceDayFactory _factory;

    public AuthTests(RaceDayFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "New Person",
            email = TestHelpers.NewEmail(),
            password = TestHelpers.TestPassword,
            roleId = TestHelpers.MemberRoleId
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var client = _factory.CreateClient();
        var body = new
        {
            fullName = "Dup User",
            email = TestHelpers.NewEmail(),
            password = TestHelpers.TestPassword,
            roleId = TestHelpers.MemberRoleId
        };

        await client.PostAsJsonAsync("/api/auth/register", body);
        var second = await client.PostAsJsonAsync("/api/auth/register", body);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_WithMissingFields_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new { email = "bad" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithUnknownRole_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Bad Role",
            email = TestHelpers.NewEmail(),
            password = TestHelpers.TestPassword,
            roleId = 99
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_Returns200()
    {
        var client = _factory.CreateClient();
        var email = TestHelpers.NewEmail();
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Login User",
            email,
            password = TestHelpers.TestPassword,
            roleId = TestHelpers.MemberRoleId
        });

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = TestHelpers.TestPassword });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var client = _factory.CreateClient();
        var email = TestHelpers.NewEmail();
        await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Login User",
            email,
            password = TestHelpers.TestPassword,
            roleId = TestHelpers.MemberRoleId
        });

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new { email, password = "WrongPassword!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Profile_WithoutLogin_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Profile_AfterLogin_Returns200()
    {
        var client = await TestHelpers.LoggedInClientAsync(_factory, TestHelpers.MemberRoleId);

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}