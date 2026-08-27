# Weather App

A C# console application that retrieves daily weather forecasts from the National Weather Service API.

## Features

- Accepts a two-letter US state abbreviation.
- Lists public NWS forecast zones for that state.
- Allows the user to select a specific forecast zone.
- Resolves the selected zone to a representative observation-station location.
- Displays daily forecast periods with:
  - Date
  - High and low temperature labels
  - Temperature and unit
  - Short conditions
  - Detailed forecast text
- Handles invalid input, empty results, API failures, and unexpected API responses.


## Requirements

- .NET 10 SDK
- Internet access for requests to the NWS API

The project targets `net10.0`, as configured in `CanAmWeatherApp.csproj`.

## Run the Application

From the project directory, run:

```powershell
dotnet run
```

When prompted:

1. Enter a state abbreviation, such as `CO`.
2. Select a forecast zone by entering its number.
3. Review the daily forecast, including daytime highs and nighttime lows.

## Build the Application

To compile without running:

```powershell
dotnet build
```

## Test the Application Manually

Run a complete Colorado forecast request using the first zone:

```powershell
@("CO", "1") | dotnet run
```

Test invalid state input:

```powershell
"X" | dotnet run
```

## National Weather Service API

The application uses the public NWS API:

- API documentation: <https://www.weather.gov/documentation/services-web-api>
- Zone lookup: `GET https://api.weather.gov/zones/public?area={state}`
- Point metadata: `GET https://api.weather.gov/points/{latitude},{longitude}`
- Forecast: the forecast URL returned by the point metadata response

NWS requires clients to identify themselves with a `User-Agent` header. This application sends `CanAmWeatherApp/1.0`.

## Project Structure

```text
CanAmWeatherApp/
|-- Models/
|   |-- ForecastPeriod.cs
|   |-- Zone.cs
|-- Services/
|   |-- NwsApiClient.cs
|-- Program.cs
|-- CanAmWeatherApp.csproj
|-- README.md
```

`NwsApiClient` handles HTTP requests and maps NWS responses. `Program.cs` handles console input and formatted output. The `bin/` and `obj/` build folders are excluded through `.gitignore`.
