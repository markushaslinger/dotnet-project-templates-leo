using LeoWebApi.Core.Util;
using LeoWebApi.Persistence.Model;

namespace LeoWebApi.Core.Logic;

public static class PayloadExtensions
{
    public static PayloadDto ToDto(this Payload self) =>
        new()
        {
            Id = self.Id,
            Description = self.Description,
            Weight = self.Weight,
            Destination = self.Destination,
            Type = self.Type
        };
}

public sealed class PayloadDto
{
    public int Id { get; set; }
    public required string Description { get; set; }
    public double Weight { get; set; }
    public PayloadDestination Destination { get; set; }
    public PayloadType Type { get; set; }
}
