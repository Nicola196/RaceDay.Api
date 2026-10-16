using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Api.Tests;

public static class TestHelpers
{
    public const string OrganiserEmail = "organiser@raceday.com";
    public const string ParticipantEmail = "participant@raceday.com";
    public const string SeedPassword = "Password123!";

    public static async Task<string> LoginAndGetTokenAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("token").GetString()!;
    }

    public static async Task<HttpClient> CreateOrganiserClientAsync(RaceDayApiFactory factory)
    {
        var client = factory.CreateClient();
        var token = await LoginAndGetTokenAsync(client, OrganiserEmail, SeedPassword);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<HttpClient> CreateParticipantClientAsync(RaceDayApiFactory factory)
    {
        var client = factory.CreateClient();
        var token = await LoginAndGetTokenAsync(client, ParticipantEmail, SeedPassword);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}