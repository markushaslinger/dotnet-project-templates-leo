using FluentValidation;
using LeoWebApi.Core.Logic;
using LeoWebApi.Core.Services;
using LeoWebApi.Core.Util;
using LeoWebApi.Persistence;
using LeoWebApi.Persistence.Model;
using LeoWebApi.Shared;
using LeoWebApi.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;

namespace LeoWebApi.Controllers;

[Route("api/launches")]
public sealed class LaunchController(
    ITransactionProvider transaction,
    IOptions<Settings> options,
    IClock clock,
    ILaunchService launchService,
    ILogger<LaunchController> logger) : BaseController
{
    private readonly Settings _settings = options.Value;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> AddLaunch([FromBody] AddLaunchRequest addRequest)
    {
        if (!ValidateRequest(addRequest, new AddLaunchRequest.Validator(_settings.ValidLaunchSites, clock)))
        {
            return BadRequest();
        }

        try
        {
            await transaction.BeginTransactionAsync();

            OneOf<Launch, NotFound, ILaunchService.RocketNotAvailable> addResult
                = await launchService.ScheduleLaunchAsync(addRequest.PlannedLaunchDate,
                                                          addRequest.LaunchSite,
                                                          addRequest.Customer,
                                                          addRequest.RocketId);

            return await addResult
                .Match<ValueTask<IActionResult>>(async launch =>
                                                 {
                                                     await transaction.CommitAsync();

                                                     return CreatedAtAction(nameof(GetLaunchById),
                                                                            new { launch.Id }, launch.ToDto());
                                                 }, async notFound =>
                                                 {
                                                     await transaction.RollbackAsync();

                                                     return NotFound();
                                                 }
                                                 , async notAvailable =>
                                                 {
                                                     await transaction.RollbackAsync();

                                                     return BadRequest();
                                                 });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            logger.LogError(ex, "Failed to add launch");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    [Route("{id:Guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<LaunchDto>> GetLaunchById([FromRoute] Guid id)
    {
        OneOf<Launch, NotFound> launchResult = await launchService.GetLaunchByIdAsync(id, false);

        return launchResult.Match<ActionResult<LaunchDto>>(launch => Ok(launch.ToDto()),
                                                           notFound => NotFound());
    }

    [HttpGet]
    [Route("future")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<FutureLaunchesResponse>> GetFutureLaunches()
    {
        IReadOnlyCollection<ILaunchService.FutureLaunch> futureLaunches = await launchService.GetFutureLaunchesAsync();

        return Ok(new FutureLaunchesResponse
        {
            FutureLaunches = futureLaunches.ToList()
        });
    }

    [HttpGet]
    [Route("{id:Guid}/payload/{payloadId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<PayloadDto>> GetPayloadById([FromRoute] Guid id, [FromRoute] int payloadId)
    {
        OneOf<Payload, NotFound> payloadResult = await launchService.GetPayloadByIdAsync(id, payloadId, false);

        return payloadResult.Match<ActionResult<PayloadDto>>(payload => Ok(payload.ToDto()),
                                                             notFound => NotFound());
    }

    [HttpPost]
    [Route("{id:Guid}/payload")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<IActionResult> AddPayload([FromRoute] Guid id, [FromBody] AddPayloadRequest addRequest)
    {
        if (!ValidateRequest(addRequest, new AddPayloadRequest.Validator()))
        {
            return BadRequest();
        }

        try
        {
            await transaction.BeginTransactionAsync();

            OneOf<Payload, NotFound, ILaunchService.TransportCapacityExceeded> addResult
                = await launchService.AddPayloadAsync(addRequest.Description, addRequest.Weight,
                                                      addRequest.Destination, addRequest.Type, id);

            return await addResult
                .Match<ValueTask<IActionResult>>(async payload =>
                {
                    await transaction.CommitAsync();

                    return CreatedAtAction(nameof(GetPayloadById),
                                           new { Id = id, PayloadId = payload.Id }, payload.ToDto());
                }, async notFound =>
                {
                    await transaction.RollbackAsync();

                    return NotFound();
                }, async transportCapacityExceeded =>
                {
                    await transaction.RollbackAsync();

                    return BadRequest();
                });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            logger.LogError(ex, "Failed to add payload");

            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}

public sealed class AddLaunchRequest
{
    public LocalDate PlannedLaunchDate { get; set; }
    public required string LaunchSite { get; set; }
    public required string Customer { get; set; }
    public int RocketId { get; set; }

    public sealed class Validator : AbstractValidator<AddLaunchRequest>
    {
        public Validator(IEnumerable<string> validLaunchSites, IClock clock)
        {
            var today = clock.GetLocalDate();
            var allowedLaunchSites = validLaunchSites.ToHashSet();

            RuleFor(r => r.LaunchSite).NotEmpty();
            RuleFor(r => r.LaunchSite)
                .Must(allowedLaunchSites.Contains)
                .WithMessage("Invalid launch site");
            RuleFor(r => r.Customer).NotEmpty();
            RuleFor(r => r.RocketId).GreaterThan(0);
            RuleFor(r => r.PlannedLaunchDate).GreaterThan(today);
        }
    }
}

public sealed class AddPayloadRequest
{
    public required string Description { get; set; }
    public double Weight { get; set; }
    public required PayloadDestination Destination { get; set; }
    public required PayloadType Type { get; set; }

    public sealed class Validator : AbstractValidator<AddPayloadRequest>
    {
        public Validator()
        {
            RuleFor(r => r.Description).NotEmpty();
            RuleFor(r => r.Weight).GreaterThan(0);
            RuleFor(r => r.Destination).IsInEnum();
            RuleFor(r => r.Type).IsInEnum();
        }
    }
}

public sealed class FutureLaunchesResponse
{
    // not using an extra DTO here - is already an immutable record based on a projection
    public required List<ILaunchService.FutureLaunch> FutureLaunches { get; set; }
}
