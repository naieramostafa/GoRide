using RideSharing.Core.ValueObjects;

namespace RideSharing.Application.Interfaces;

public interface IMapService
{
    Task<double> CalculateDistanceAsync(Location origin, Location destination);
    Task<int> EstimateDurationAsync(Location origin, Location destination);
    Task<Location> GeocodeAsync(string address);
    Task<string> ReverseGeocodeAsync(double latitude, double longitude);
}
