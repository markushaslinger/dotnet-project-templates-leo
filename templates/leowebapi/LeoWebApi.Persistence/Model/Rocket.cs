namespace LeoWebApi.Persistence.Model;

public class Rocket
{
    public int Id { get; set; }
    public required string ModelName { get; set; }
    public required string Manufacturer { get; set; }
    public double MaxThrust { get; set; }
    public long PayloadDeltaV { get; set; }
}
