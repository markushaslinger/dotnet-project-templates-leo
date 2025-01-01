using FluentValidation;
using LeoWebApi.Core.Logic;
using LeoWebApi.Core.Services;
using LeoWebApi.Persistence;
using LeoWebApi.Persistence.Model;
using LeoWebApi.Persistence.Util;
using LeoWebApi.Util;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;

namespace LeoWebApi.Controllers;

[Route("api/rockets")]
public sealed class RocketController(
    ITransactionProvider transaction,
    IRocketService rocketService,
    ILogger<RocketController> logger) : BaseController
{
    [HttpGet]
    [Route("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async ValueTask<ActionResult<RocketDto>> GetRocketById([FromRoute] int id)
    {
        if (id < 0)
        {
            return BadRequest();
        }

        OneOf<Rocket, NotFound> rocketResult = await rocketService.GetRocketByIdAsync(id, false);

        return rocketResult.Match<ActionResult<RocketDto>>(rocket => Ok(rocket.ToDto()),
                                                           notFound => NotFound());
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async ValueTask<ActionResult<AllRocketsResponse>> GetAllRockets()
    {
        IReadOnlyCollection<Rocket> rockets = await rocketService.GetAllRocketsAsync();

        return Ok(new AllRocketsResponse
        {
            Rockets = rockets.Select(r => r.ToDto()).ToList()
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async ValueTask<IActionResult> AddRocket([FromBody] AddRocketRequest addRequest)
    {
        if (!ValidateRequest<AddRocketRequest.Validator, AddRocketRequest>(addRequest))
        {
            return BadRequest();
        }

        try
        {
            await transaction.BeginTransactionAsync();

            var rocket = await rocketService.AddRocketAsync(addRequest.Manufacturer, addRequest.ModelName,
                                                            addRequest.MaxThrust, addRequest.PayloadDeltaV);
            await transaction.CommitAsync();

            return CreatedAtAction(nameof(GetRocketById), new
            {
                rocket.Id
            }, rocket.ToDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add rocket");
            await transaction.RollbackAsync();

            return Problem();
        }
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async ValueTask<IActionResult> DeleteRocket([FromRoute] int id)
    {
        try
        {
            await transaction.BeginTransactionAsync();

            OneOf<Success, NotFound> deleteResult = await rocketService.DeleteRocketAsync(id);

            return await deleteResult.Match<ValueTask<IActionResult>>(async success =>
            {
                await transaction.CommitAsync();

                return NoContent();
            }, async notFound =>
            {
                await transaction.RollbackAsync();

                return NotFound();
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete rocket");
            await transaction.RollbackAsync();

            return Problem();
        }
    }
}

public sealed class AddRocketRequest
{
    public required string ModelName { get; set; }
    public required string Manufacturer { get; set; }
    public double MaxThrust { get; set; }
    public long PayloadDeltaV { get; set; }

    public sealed class Validator : AbstractValidator<AddRocketRequest>
    {
        public Validator()
        {
            RuleFor(x => x.ModelName).NotEmpty();
            RuleFor(x => x.Manufacturer).NotEmpty();
            RuleFor(x => x.MaxThrust).GreaterThan(0);
            RuleFor(x => x.PayloadDeltaV).GreaterThan(0);
        }
    }
}

public sealed class AllRocketsResponse
{
    public required List<RocketDto> Rockets { get; set; }
}
