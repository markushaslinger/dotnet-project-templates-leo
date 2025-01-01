using LeoWebApi.Core.Logic;
using LeoWebApi.Persistence.Model;

namespace LeoWebApi.Test;

public sealed class RocketTests
{
    [Fact]
    public void ToDto_Success()
    {
        var rocket = new Rocket
        {
            Id = 1,
            ModelName = "H-IIA",
            Manufacturer = "JAXA",
            MaxThrust = 1_500_000,
            PayloadDeltaV = 36_000_000
        };

        var dto = rocket.ToDto();

        dto.Should().NotBeNull();
        dto.Id.Should().Be(rocket.Id);
        dto.ModelName.Should().Be(rocket.ModelName);
        dto.Manufacturer.Should().Be(rocket.Manufacturer);
        dto.MaxThrust.Should().Be(rocket.MaxThrust);
        dto.PayloadDeltaV.Should().Be(rocket.PayloadDeltaV);
    }
}
