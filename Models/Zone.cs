namespace CanAmWeatherApp.Models;

public class Zone
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public double Latitude { get; set; } = double.NaN;
    public double Longitude { get; set; } = double.NaN;
    public string ObservationStationUrl { get; set; } = "";
}