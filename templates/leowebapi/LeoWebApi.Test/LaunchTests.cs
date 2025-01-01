using LeoWebApi.Core.Logic;
using LeoWebApi.Persistence.Model;

namespace LeoWebApi.Test;

public sealed class LaunchTests
{
    [Fact]
    public void TryAddPayload_Success()
    {
        const string Description = "Lunchboxes";
        const double Weight = 1234.5D;
        const PayloadDestination Destination = PayloadDestination.LowEarthOrbit;
        const PayloadType Type = PayloadType.Cargo;

        var launch = CreateDefaultLaunch();
        var result = launch.TryAddPayload(Description, Weight, Destination, Type, out var payload);

        result.Should().BeTrue();
        payload.Should().NotBeNull();
        payload!.Description.Should().Be(Description);
        payload.Weight.Should().Be(Weight);
        payload.Destination.Should().Be(Destination);
        payload.Type.Should().Be(Type);
    }


    [Fact]
    public void TryAddPayload_TooHeavy()
    {
        var launch = CreateDefaultLaunch();
        var result = launch.TryAddPayload("Heavyweight", 5000, PayloadDestination.Mars, PayloadType.Cargo, out var payload);

        result.Should().BeFalse();
        payload.Should().BeNull();
    }

    private static Launch CreateDefaultLaunch()
    {
        var launch = new Launch
        {
            Id = Guid.NewGuid(),
            PlannedLaunchDate = new LocalDate(2025, 02, 01),
            LaunchDate = null,
            LaunchSite = "Cape Canaveral",
            Customer = "NASA",
            Rocket = new Rocket
            {
                Id = 1,
                ModelName = "SLS Block 1B",
                Manufacturer = "Boeing",
                MaxThrust = 16000000,
                PayloadDeltaV = 205000000
            },
            Payloads = []
        };
        launch.Payloads.Add(new Payload
        {
            Id = 1,
            Description = "Orion",
            Weight = 16000,
            Destination = PayloadDestination.Moon,
            Type = PayloadType.Crew,
            Launch = launch
        });
        launch.Rocket.Launch = launch;

        return launch;
    }
}
