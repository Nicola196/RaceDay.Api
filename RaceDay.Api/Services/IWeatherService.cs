using RaceDay.Api.DTOs;

namespace RaceDay.Api.Services;

public interface IWeatherService
{
    Task<WeatherForecastDto> GetForecastAsync(double latitude, double longitude, DateTime date);
}