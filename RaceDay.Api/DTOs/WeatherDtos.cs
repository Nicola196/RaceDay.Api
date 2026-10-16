namespace RaceDay.Api.DTOs;

public class WeatherForecastDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Date { get; set; } = string.Empty;
    public double? MinTemperature { get; set; }
    public double? MaxTemperature { get; set; }
    public double? MaxWindSpeed { get; set; }
    public string Conditions { get; set; } = string.Empty;
}