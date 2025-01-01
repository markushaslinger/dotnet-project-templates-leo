using LeoWebApi.Persistence.Model;

namespace LeoWebApi.Core.Logic;

public static class RocketExtensions
{
    public static RocketDto ToDto(this Rocket self) =>
        new()
        {
            Id = self.Id,
            ModelName = self.ModelName,
            Manufacturer = self.Manufacturer,
            MaxThrust = self.MaxThrust,
            PayloadDeltaV = self.PayloadDeltaV
        };
}

public sealed class RocketDto
{
    public int Id { get; set; }
    public required string ModelName { get; set; }
    public required string Manufacturer { get; set; }
    public double MaxThrust { get; set; }
    public long PayloadDeltaV { get; set; }
}
