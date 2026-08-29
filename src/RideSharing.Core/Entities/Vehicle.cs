using RideSharing.Core.Enums;

namespace RideSharing.Core.Entities;

public class Vehicle
{
    public Guid Id { get; private set; }
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public string Year { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
    public string LicensePlate { get; private set; } = string.Empty;
    public VehicleType Type { get; private set; }
    public int Capacity { get; private set; }

    private Vehicle() { }

    public Vehicle(string make, string model, string year, string color,
                   string licensePlate, VehicleType type, int capacity)
    {
        Id = Guid.NewGuid();
        Make = make;
        Model = model;
        Year = year;
        Color = color;
        LicensePlate = licensePlate;
        Type = type;
        Capacity = capacity;
    }
}
