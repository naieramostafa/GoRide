using System.Text.Json;
using RideSharing.Application.Interfaces;
using RideSharing.Core.ValueObjects;

namespace RideSharing.Infrastructure.Services.Maps;

public class OsrmMapService : IMapService
{
    private readonly HttpClient _httpClient;

    public OsrmMapService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<double> CalculateDistanceAsync(Location origin, Location destination)
    {
        var url = $"https://router.project-osrm.org/route/v1/driving/{origin.Longitude},{origin.Latitude};{destination.Longitude},{destination.Latitude}?overview=false";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var distanceMeters = doc.RootElement.GetProperty("routes")[0].GetProperty("distance").GetDouble();
        return distanceMeters / 1000.0;
    }

    public async Task<int> EstimateDurationAsync(Location origin, Location destination)
    {
        var url = $"https://router.project-osrm.org/route/v1/driving/{origin.Longitude},{origin.Latitude};{destination.Longitude},{destination.Latitude}?overview=false";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var durationSeconds = doc.RootElement.GetProperty("routes")[0].GetProperty("duration").GetDouble();
        return (int)Math.Ceiling(durationSeconds / 60.0);
    }

    public async Task<Location> GeocodeAsync(string address)
    {
        var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("RideSharingApp/1.0");
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var result = doc.RootElement[0];
        var lat = result.GetProperty("lat").GetDouble();
        var lng = result.GetProperty("lon").GetDouble();
        return new Location(lat, lng, address);
    }

    public async Task<string> ReverseGeocodeAsync(double latitude, double longitude)
    {
        var url = $"https://nominatim.openstreetmap.org/reverse?lat={latitude}&lon={longitude}&format=json";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("RideSharingApp/1.0");
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var address = doc.RootElement.GetProperty("display_name").GetString() ?? $"{latitude},{longitude}";
        return address;
    }
}
