namespace LeoWebApi.Persistence.Model;

public class Payload
{
    public int Id { get; set; }
    public required string Description { get; set; }
    public double Weight { get; set; }
    public PayloadType Type { get; set; }
    public PayloadDestination Destination { get; set; }
    public Guid LaunchId { get; set; }
    public required Launch Launch { get; set; }
}

public enum PayloadDestination
{
    Unknown = 0,
    LowEarthOrbit = 10,
    GeostationaryOrbit = 20,
    Moon = 30,
    Mars = 40
}

public enum PayloadType
{
    Unknown = 0,
    Cargo = 10,
    Crew = 20,
    Satellite = 30
}