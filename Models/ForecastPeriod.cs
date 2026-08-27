namespace CanAmWeatherApp.Models;

public class ForecastPeriod
{
    public string Name { get; set; } = "";
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public int Temperature { get; set; }
    public string TemperatureUnit { get; set; } = "";
    public string ShortForecast { get; set; } = "";
    public string DetailedForecast { get; set; } = "";
    public bool IsDaytime { get; set; }
}