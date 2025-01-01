using System.Net;
using LeoWebApi.Controllers;
using LeoWebApi.Core.Logic;
using LeoWebApi.Persistence.Model;
using LeoWebApi.TestInt.Util;

namespace LeoWebApi.TestInt;

public sealed class RocketTests(WebApiTestFixture webApiFixture) : WebApiTestBase(webApiFixture)
{
    [Fact]
    public async Task GetAllRockets_ExistingRockets_Success()
    {
        const string Manufacturer1 = "Blue Origin";
        const string ModelName1 = "New Shepard";
        const string Manufacturer2 = "SpaceX";
        const string ModelName2 = "Falcon 9";

        await ModifyDatabaseContent(async ctx =>
        {
            ctx.Rockets.Add(new Rocket
            {
                Manufacturer = Manufacturer1,
                ModelName = ModelName1,
                MaxThrust = 1_000_000,
                PayloadDeltaV = 44_000_000
            });
            ctx.Rockets.Add(new Rocket
            {
                Manufacturer = Manufacturer2,
                ModelName = ModelName2,
                MaxThrust = 1_800_000,
                PayloadDeltaV = 22_800_000
            });

            await ctx.SaveChangesAsync();
        });

        var response = await ApiClient.GetAsync("api/rockets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<AllRocketsResponse>();

        content.Should().NotBeNull();
        content!.Rockets.Should().NotBeEmpty()
                .And.HaveCount(2);
        content.Rockets.Should().ContainSingle(r => r.Manufacturer == Manufacturer1 && r.ModelName == ModelName1);
        content.Rockets.Should().ContainSingle(r => r.Manufacturer == Manufacturer2 && r.ModelName == ModelName2);
    }

    [Fact]
    public async Task AddRocket_Success()
    {
        const string Manufacturer = "NASA";
        const string ModelName = "Saturn V";
        const double MaxThrust = 34_000_000D;
        const long PayloadDeltaV = 1_372_000_000;

        var response = await ApiClient.PostAsJsonAsync("api/rockets", new AddRocketRequest
        {
            Manufacturer = Manufacturer,
            ModelName = ModelName,
            MaxThrust = MaxThrust,
            PayloadDeltaV = PayloadDeltaV
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        var content = await response.Content.ReadFromJsonAsync<RocketDto>();

        content.Should().NotBeNull();
        ValidateRocket(content!);

        response = await ApiClient.GetAsync("api/rockets");
        var allRocketsContent = await response.Content.ReadFromJsonAsync<AllRocketsResponse>();

        allRocketsContent.Should().NotBeNull();
        allRocketsContent!.Rockets.Should().NotBeEmpty()
                          .And.HaveCount(1);
        ValidateRocket(allRocketsContent.Rockets.First());

        return;

        void ValidateRocket(RocketDto rocket)
        {
            rocket.Manufacturer.Should().Be(Manufacturer);
            rocket.ModelName.Should().Be(ModelName);
            rocket.MaxThrust.Should().Be(MaxThrust);
            rocket.PayloadDeltaV.Should().Be(PayloadDeltaV);
        }
    }
}