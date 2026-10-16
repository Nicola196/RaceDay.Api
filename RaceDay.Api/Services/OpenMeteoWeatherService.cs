using System.Globalization;
using System.Text.Json;
using RaceDay.Api.DTOs;

namespace RaceDay.Api.Services;

public class OpenMeteoWeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenMeteoWeatherService> _logger;

    public OpenMeteoWeatherService(HttpClient httpClient, ILogger<OpenMeteoWeatherService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri("https://api.open-meteo.com/");
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<WeatherForecastDto> GetForecastAsync(double latitude, double longitude, DateTime date)
    {
        var dateString = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var url =
            $"v1/forecast?latitude={latitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={longitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&daily=temperature_2m_max,temperature_2m_min,weather_code,wind_speed_10m_max" +
            $"&timezone=auto&start_date={dateString}&end_date={dateString}";

        try
        {
            using var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalServiceException(
                    $"Weather provider returned status {(int)response.StatusCode}.");
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();

            if (!json.TryGetProperty("daily", out var daily))
            {
                throw new ExternalServiceException("Weather provider returned an unexpected payload.");
            }

            return new WeatherForecastDto
            {
                Latitude = latitude,
                Longitude = longitude,
                Date = dateString,
                MaxTemperature = ReadFirstDouble(daily, "temperature_2m_max"),
                MinTemperature = ReadFirstDouble(daily, "temperature_2m_min"),
                MaxWindSpeed = ReadFirstDouble(daily, "wind_speed_10m_max"),
                Conditions = DescribeWeatherCode(ReadFirstInt(daily, "weather_code"))
            };
        }
        catch (ExternalServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Weather lookup failed for lat={Lat} lon={Lon}", latitude, longitude);
            throw new ExternalServiceException("The weather provider is currently unavailable.", ex);
        }
    }

    private static double? ReadFirstDouble(JsonElement daily, string propertyName)
    {
        if (!daily.TryGetProperty(propertyName, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return null;

        var first = arr.EnumerateArray().FirstOrDefault();
        return first.ValueKind == JsonValueKind.Number ? first.GetDouble() : null;
    }

    private static int? ReadFirstInt(JsonElement daily, string propertyName)
    {
        if (!daily.TryGetProperty(propertyName, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return null;

        var first = arr.EnumerateArray().FirstOrDefault();
        return first.ValueKind == JsonValueKind.Number ? first.GetInt32() : null;
    }

    private static string DescribeWeatherCode(int? code) => code switch
    {
        0 => "Clear sky",
        1 or 2 or 3 => "Partly cloudy",
        45 or 48 => "Fog",
        51 or 53 or 55 => "Drizzle",
        61 or 63 or 65 => "Rain",
        71 or 73 or 75 => "Snow",
        80 or 81 or 82 => "Rain showers",
        95 or 96 or 99 => "Thunderstorm",
        null => "Unknown",
        _ => "Mixed conditions"
    };
}