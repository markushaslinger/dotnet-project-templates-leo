using System.Diagnostics.CodeAnalysis;
using LeoWebApi.Persistence.Model;
using LeoWebApi.Shared;

namespace LeoWebApi.Core.Logic;

public static class LaunchExtensions
{
    public static bool TryAddPayload(this Launch self, string description, double weight,
                                     PayloadDestination destination, PayloadType type,
                                     [NotNullWhen(true)] out Payload? payload)
    {
        var totalCurrentPayloadWeight = self.Payloads.Sum(p => p.Weight);
        var newTotalWeight = totalCurrentPayloadWeight + weight;
        var newRequiredDeltaV = Const.RequiredDeltaVPerKg * newTotalWeight;

        if (newRequiredDeltaV > self.Rocket.PayloadDeltaV)
        {
            payload = null;

            return false;
        }

        payload = new Payload
        {
            Description = description,
            Weight = weight,
            Destination = destination,
            Type = type,
            Launch = self
        };
        self.Payloads.Add(payload);

        return true;
    }

    public static LaunchDto ToDto(this Launch self) =>
        new()
        {
            Id = self.Id,
            PlannedLaunchDate = self.PlannedLaunchDate,
            LaunchDate = self.LaunchDate,
            LaunchSite = self.LaunchSite,
            Customer = self.Customer,
            Rocket = self.Rocket.ToDto(),
            Payloads = self.Payloads.Select(p => p.ToDto()).ToList()
        };
}

public sealed class LaunchDto
{
    public Guid Id { get; set; }
    public LocalDate PlannedLaunchDate { get; set; }
    public Instant? LaunchDate { get; set; }
    public required string LaunchSite { get; set; }
    public required string Customer { get; set; }
    public required RocketDto Rocket { get; set; }
    public required List<PayloadDto> Payloads { get; set; } = [];
}
