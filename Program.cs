using System.Text.Json;
using CanAmWeatherApp.Services;

var client = new NwsApiClient();

Console.Write("Enter a state abbreviation (e.g. CO): ");
var state = Console.ReadLine()?.Trim().ToUpper() ?? "";

try
{
    Console.WriteLine("\nFetching zones...\n");
    var zones = await client.GetZonesByStateAsync(state);

    if (zones.Count == 0)
    {
        Console.WriteLine("No public forecast zones were found for that state.");
        return;
    }

    for (int i = 0; i < zones.Count; i++)
        Console.WriteLine($"{i + 1}. {zones[i].Name} ({zones[i].Id})");

    Console.Write("\nSelect a zone number: ");
    if (!int.TryParse(Console.ReadLine(), out var selection) || selection < 1 || selection > zones.Count)
    {
        Console.WriteLine("Please select a number from the list.");
        return;
    }
    var selectedZone = zones[selection - 1];

    Console.WriteLine($"\nFetching forecast for {selectedZone.Name}...\n");
    var periods = await client.GetForecastByZoneAsync(selectedZone);

    if (periods.Count == 0)
    {
        Console.WriteLine("No forecast periods were returned for this zone.");
        return;
    }

    Console.WriteLine($"{"Period",-20} {"Date",-12} {"Type",-6} {"Temp",-8} {"Conditions"}");
    Console.WriteLine(new string('-', 100));
    foreach (var period in periods)
    {
        var temperatureType = period.IsDaytime ? "High" : "Low";
        var localStartTime = period.StartTime.ToLocalTime();
        Console.WriteLine($"{period.Name,-20} {localStartTime:MMM dd, yyyy} {temperatureType,-6} {period.Temperature}°{period.TemperatureUnit,-6} {period.ShortForecast}");
        Console.WriteLine($"  Details: {period.DetailedForecast}");
    }
}
catch (ArgumentException exception)
{
    Console.WriteLine($"Input error: {exception.Message}");
}
catch (HttpRequestException exception)
{
    Console.WriteLine($"Weather service error: {exception.Message}");
}
catch (JsonException exception)
{
    Console.WriteLine($"Weather service returned unexpected data: {exception.Message}");
}
catch (InvalidOperationException exception)
{
    Console.WriteLine($"Weather service returned incomplete data: {exception.Message}");
}
