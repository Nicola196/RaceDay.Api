using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDayApi.Tests;

public static class TestHelpers
{
    public const int OrganiserRoleId = 1;
    public const int MemberRoleId = 2;
    public const string TestPassword = "Password123!";

    public static string NewEmail() => $"{Guid.NewGuid():N}@test.com";

    /// <summary>Registers a new user and logs them in. The returned client keeps the session cookie.</summary>
    public static async Task<HttpClient> LoggedInClientAsync(RaceDayFactory factory, int roleId)
    {
        var client = factory.CreateClient();
        var email = NewEmail();

        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Test User",
            email,
            password = TestPassword,
            roleId
        });
        register.EnsureSuccessStatusCode();

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = TestPassword });
        login.EnsureSuccessStatusCode();

        return client;
    }

    public static async Task<int> CreateCategoryAsync(HttpClient organiser)
    {
        var response = await organiser.PostAsJsonAsync("/api/categories", new { categoryName = "10km" });
        response.EnsureSuccessStatusCode();
        return await ReadIdAsync(response, "categoryId");
    }

    public static object EventBody(int categoryId, string name = "Test Run") => new
    {
        eventName = name,
        eventDescription = "A test event",
        eventDate = DateTime.UtcNow.AddDays(10),
        location = "Johannesburg",
        distanceKm = 10,
        eventType = "Run",
        latitude = -26.2041,
        longitude = 28.0473,
        categoryId
    };

    public static async Task<int> CreateEventAsync(HttpClient organiser)
    {
        var categoryId = await CreateCategoryAsync(organiser);
        var response = await organiser.PostAsJsonAsync("/api/events", EventBody(categoryId));
        response.EnsureSuccessStatusCode();
        return await ReadIdAsync(response, "eventId");
    }

    public static async Task<int> ReadIdAsync(HttpResponseMessage response, string property)
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty(property).GetInt32();
    }
}