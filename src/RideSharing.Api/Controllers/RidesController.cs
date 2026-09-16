using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideSharing.Application.DTOs;
using RideSharing.Application.Interfaces;
using RideSharing.Application.UseCases.Rides.Commands;
using RideSharing.Application.UseCases.Rides.Queries;

namespace RideSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RidesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICacheService _cache;

    public RidesController(IMediator mediator, ICacheService cache)
    {
        _mediator = mediator;
        _cache = cache;
    }

    [HttpPost]
    public async Task<ActionResult<RideResponseDto>> RequestRide(RideRequestDto request)
    {
        var passengerId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _mediator.Send(new RequestRideCommand(passengerId, request));
        await _cache.RemoveAsync("rides_active");
        await _cache.RemoveAsync($"user_rides_{passengerId}");
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/accept")]
    public async Task<ActionResult<RideResponseDto>> AcceptRide(Guid id)
    {
        var driverId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var dto = new RideAcceptDto { RideId = id, DriverId = driverId };
        var result = await _mediator.Send(new AcceptRideCommand(dto));
        await _cache.RemoveAsync($"ride_{id}");
        await _cache.RemoveAsync("rides_active");
        return Ok(result);
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<RideResponseDto>> StartRide(Guid id)
    {
        var driverId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _mediator.Send(new StartRideCommand(id, driverId));
        await _cache.RemoveAsync($"ride_{id}");
        await _cache.RemoveAsync("rides_active");
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<RideResponseDto>> CompleteRide(Guid id, RideCompleteDto complete)
    {
        var driverId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        complete.RideId = id;
        complete.DriverId = driverId;
        var result = await _mediator.Send(new CompleteRideCommand(complete));
        await _cache.RemoveAsync($"ride_{id}");
        await _cache.RemoveAsync("rides_active");
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RideResponseDto>> GetById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var cacheKey = $"ride_{id}";
        var cached = await _cache.GetAsync<RideResponseDto>(cacheKey);
        if (cached != null)
        {
            if (cached.PassengerId != userId && cached.DriverId != userId)
                return Forbid();
            return Ok(cached);
        }

        var result = await _mediator.Send(new GetRideQuery(id));
        if (result.PassengerId != userId && result.DriverId != userId)
            return Forbid();
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<RideResponseDto>> CancelRide(Guid id, RideCancelDto cancel)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        cancel.RideId = id;
        var result = await _mediator.Send(new CancelRideCommand(cancel, userId));
        await _cache.RemoveAsync($"ride_{id}");
        await _cache.RemoveAsync("rides_active");
        await _cache.RemoveAsync($"user_rides_{userId}");
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<RideResponseDto>>> GetActive()
    {
        var cached = await _cache.GetAsync<List<RideResponseDto>>("rides_active");
        if (cached != null)
            return Ok(cached);

        var result = await _mediator.Send(new GetActiveRidesQuery());
        var list = result.ToList();
        await _cache.SetAsync("rides_active", list, TimeSpan.FromSeconds(30));
        return Ok(list);
    }
}
