using System.Net.Http.Headers;
using System.Text.Json;
using CanAmWeatherApp.Models;

namespace CanAmWeatherApp.Services;

public class NwsApiClient
{
	private const string BaseAddress = "https://api.weather.gov/";
	private readonly HttpClient _httpClient;
	private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

	public NwsApiClient(HttpClient? httpClient = null)
	{
		_httpClient = httpClient ?? new HttpClient();
		_httpClient.BaseAddress ??= new Uri(BaseAddress);

		if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
			_httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("CanAmWeatherApp", "1.0"));
	}

	public async Task<List<Zone>> GetZonesByStateAsync(string state)
	{
		if (state.Length != 2 || !state.All(char.IsLetter))
			throw new ArgumentException("Enter a two-letter state abbreviation.", nameof(state));

		var response = await GetJsonAsync<ZoneCollection>($"zones/public?area={Uri.EscapeDataString(state)}&include_geometry=true");
		return response.Features
			.Where(feature => feature.Properties is not null)
			.Select(feature =>
			{
				var point = GetRepresentativePoint(feature.Geometry?.Coordinates);
				return new Zone
				{
					Id = feature.Properties!.Id,
					Name = feature.Properties.Name,
					Latitude = point.Latitude,
					Longitude = point.Longitude,
					ObservationStationUrl = feature.Properties.ObservationStations.FirstOrDefault() ?? ""
				};
			})
			.Where(zone => zone.Id.Length > 0)
			.OrderBy(zone => zone.Name)
			.ToList();
	}

	public async Task<List<ForecastPeriod>> GetForecastByZoneAsync(Zone zone)
	{
		if (double.IsNaN(zone.Latitude) || double.IsNaN(zone.Longitude) || zone.Latitude is < -90 or > 90 || zone.Longitude is < -180 or > 180)
		{
			if (string.IsNullOrWhiteSpace(zone.ObservationStationUrl))
				throw new InvalidOperationException("The NWS API did not provide a location for this zone.");

			var station = await GetJsonAsync<StationResponse>(zone.ObservationStationUrl);
			var stationPoint = GetRepresentativePoint(station.Geometry?.Coordinates);
			zone.Latitude = stationPoint.Latitude;
			zone.Longitude = stationPoint.Longitude;
		}

		var forecastPoint = await GetJsonAsync<PointResponse>($"points/{zone.Latitude:F4},{zone.Longitude:F4}");
		var forecastUrl = forecastPoint.Properties?.Forecast;
		if (string.IsNullOrWhiteSpace(forecastUrl))
			throw new InvalidOperationException("The NWS API did not provide a forecast for this zone.");

		var forecast = await GetJsonAsync<ForecastResponse>(forecastUrl);
		return forecast.Properties?.Periods ?? [];
	}

	private async Task<T> GetJsonAsync<T>(string requestUri)
	{
		using var response = await _httpClient.GetAsync(requestUri);
		var body = await response.Content.ReadAsStringAsync();
		if (!response.IsSuccessStatusCode)
			throw new HttpRequestException($"NWS API returned {(int)response.StatusCode}: {body}");

		return JsonSerializer.Deserialize<T>(body, _jsonOptions)
			?? throw new JsonException("The NWS API returned an empty response.");
	}

	private static (double Latitude, double Longitude) GetRepresentativePoint(JsonElement? coordinates)
	{
		var points = new List<(double Longitude, double Latitude)>();
		if (coordinates is JsonElement value)
			CollectCoordinatePairs(value, points);

		if (points.Count == 0)
			return (double.NaN, double.NaN);

		return (points.Average(point => point.Latitude), points.Average(point => point.Longitude));
	}

	private static void CollectCoordinatePairs(JsonElement value, List<(double Longitude, double Latitude)> points)
	{
		if (value.ValueKind != JsonValueKind.Array)
			return;

		if (value.GetArrayLength() >= 2 && value[0].ValueKind == JsonValueKind.Number && value[1].ValueKind == JsonValueKind.Number)
		{
			points.Add((value[0].GetDouble(), value[1].GetDouble()));
			return;
		}

		foreach (var child in value.EnumerateArray())
			CollectCoordinatePairs(child, points);
	}

	private sealed class ZoneCollection
	{
		public List<ZoneFeature> Features { get; set; } = [];
	}

	private sealed class ZoneFeature
	{
		public ZoneProperties? Properties { get; set; }
		public Geometry? Geometry { get; set; }
	}

	private sealed class ZoneProperties
	{
		public string Id { get; set; } = "";
		public string Name { get; set; } = "";
		public List<string> ObservationStations { get; set; } = [];
	}

	private sealed class Geometry
	{
		public JsonElement Coordinates { get; set; }
	}

	private sealed class PointResponse
	{
		public PointProperties? Properties { get; set; }
	}

	private sealed class PointProperties
	{
		public string? Forecast { get; set; }
	}

	private sealed class StationResponse
	{
		public Geometry? Geometry { get; set; }
	}

	private sealed class ForecastResponse
	{
		public ForecastProperties? Properties { get; set; }
	}

	private sealed class ForecastProperties
	{
		public List<ForecastPeriod>? Periods { get; set; }
	}
}
