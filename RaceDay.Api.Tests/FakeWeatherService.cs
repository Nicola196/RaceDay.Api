using RaceDay.Api.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.Api.Tests;

public class FakeWeatherService : IWeatherService
{
    public Task<WeatherForecastDto> GetForecastAsync(double latitude, double longitude, DateTime date)
    {
        return Task.FromResult(new WeatherForecastDto
        {
            Latitude = latitude,
            Longitude = longitude,
            Date = date.ToString("yyyy-MM-dd"),
            MinTemperature = 12.5,
            MaxTemperature = 24.0,
            MaxWindSpeed = 8.0,
            Conditions = "Clear sky"
        });
    }
}