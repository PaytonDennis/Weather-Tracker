namespace CanAmWeatherApp.Models;

public class Zone
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string ObservationStationUrl { get; set; } = "";
}