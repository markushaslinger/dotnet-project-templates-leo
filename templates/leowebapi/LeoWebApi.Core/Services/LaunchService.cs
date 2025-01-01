using LeoWebApi.Core.Logic;
using LeoWebApi.Core.Util;
using LeoWebApi.Persistence;
using LeoWebApi.Persistence.Model;
using LeoWebApi.Shared;
using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;

namespace LeoWebApi.Core.Services;

public interface ILaunchService
{
    public ValueTask<OneOf<Launch, NotFound>> GetLaunchByIdAsync(Guid id, bool tracking);
    public ValueTask<IReadOnlyCollection<FutureLaunch>> GetFutureLaunchesAsync();

    public ValueTask<OneOf<Launch, NotFound, RocketNotAvailable>> ScheduleLaunchAsync(
        LocalDate plannedLaunchDate, string launchSite,
        string customer, int rocketId);

    public ValueTask<OneOf<Payload, NotFound, TransportCapacityExceeded>> AddPayloadAsync(
        string description, double weight, PayloadDestination destination, PayloadType type, Guid launchId);

    public ValueTask<OneOf<Payload, NotFound>> GetPayloadByIdAsync(Guid id, int payloadId, bool tracking);

    public readonly record struct RocketNotAvailable;

    public readonly record struct TransportCapacityExceeded;

    public sealed record FutureLaunch(Guid Id, LocalDate PlannedLaunchDate);
}

internal sealed class LaunchService(
    IUnitOfWork uow,
    IClock clock,
    IOptions<Settings> options,
    IRocketService rocketService,
    ILogger<LaunchService> logger) : ILaunchService
{
    private readonly Settings _settings = options.Value;

    public async ValueTask<OneOf<Launch, NotFound>> GetLaunchByIdAsync(Guid id, bool tracking)
    {
        var launch = await uow.LaunchRepository.GetLaunchByIdAsync(id, tracking);

        return launch is null ? new NotFound() : launch;
    }

    public async ValueTask<IReadOnlyCollection<ILaunchService.FutureLaunch>> GetFutureLaunchesAsync()
    {
        var today = clock.GetLocalDate();
        var launches = await uow.LaunchRepository.GetLaunchesWithLaunchDateAfterAsync(today);

        return launches
               .Select(t => new ILaunchService.FutureLaunch(t.Id, t.PlannedDate))
               .ToList();
    }

    public async ValueTask<OneOf<Launch, NotFound, ILaunchService.RocketNotAvailable>> ScheduleLaunchAsync(
        LocalDate plannedLaunchDate, string launchSite, string customer, int rocketId)
    {
        var rocketResult = await rocketService.GetRocketByIdAsync(rocketId, true);
        var rocket = rocketResult.Match<Rocket?>(r => r, notFound => null);
        if (rocket is null || rocket.Launch is not null)
        {
            var bookedForOtherLaunch = rocket?.Launch is not null;
            if (!bookedForOtherLaunch)
            {
                return new NotFound();
            }

            logger.LogWarning("An attempt was made to schedule a launch with rocket {RocketId}, which is already booked for launch {LaunchId}",
                              rocketId, rocket?.Launch?.Id);

            return new ILaunchService.RocketNotAvailable();
        }

        var launch = uow.LaunchRepository.AddLaunch(plannedLaunchDate, launchSite, customer, rocket);

        await uow.SaveChangesAsync();

        return launch;
    }

    public async ValueTask<OneOf<Payload, NotFound, ILaunchService.TransportCapacityExceeded>>
        AddPayloadAsync(string description, double weight, PayloadDestination destination, PayloadType type,
                        Guid launchId)
    {
        if (_settings.MaxPayloadWeight < weight)
        {
            return new ILaunchService.TransportCapacityExceeded();
        }

        var launch = await uow.LaunchRepository.GetLaunchByIdAsync(launchId, true);
        if (launch is null)
        {
            return new NotFound();
        }

        if (!launch.TryAddPayload(description, weight, destination, type, out var payload))
        {
            return new ILaunchService.TransportCapacityExceeded();
        }

        await uow.SaveChangesAsync();

        return payload;
    }

    public async ValueTask<OneOf<Payload, NotFound>> GetPayloadByIdAsync(Guid id, int payloadId, bool tracking)
    {
        var launchResult = await GetLaunchByIdAsync(id, tracking);

        return launchResult
            .Match<OneOf<Payload, NotFound>>(launch =>
                                             {
                                                 var payload = launch.Payloads
                                                                     .FirstOrDefault(p => p.Id == payloadId);

                                                 return payload is null ? new NotFound() : payload;
                                             },
                                             notFound => new NotFound());
    }
}
