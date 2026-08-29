using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideSharing.Application.DTOs;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Constants;

namespace RideSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriversController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public DriversController(IUnitOfWork uow, IMapper mapper, ICacheService cache)
    {
        _uow = uow;
        _mapper = mapper;
        _cache = cache;
    }

    [HttpPut("{userId:guid}/location")]
    public async Task<IActionResult> UpdateLocation(Guid userId, [FromBody] RideLocationUpdateDto dto)
    {
        var driver = await _uow.Drivers.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Driver not found");
        driver.UpdateLocation(new(dto.Latitude, dto.Longitude));
        await _uow.CommitAsync();
        await _cache.RemoveAsync($"drivers_nearby_{userId}");
        return Ok(new { message = "Location updated", latitude = dto.Latitude, longitude = dto.Longitude });
    }

    [HttpPut("{userId:guid}/availability")]
    public async Task<IActionResult> SetAvailability(Guid userId, [FromBody] bool available)
    {
        var driver = await _uow.Drivers.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Driver not found");
        if (available && !driver.IsVerified)
            throw new RideSharing.Core.Exceptions.DomainException("Driver is not verified yet. An admin must verify you first.");
        driver.SetAvailable(available);
        await _uow.CommitAsync();
        return Ok(new { message = available ? "Driver is now online" : "Driver is now offline", isAvailable = available });
    }

    [HttpGet("{userId:guid}/rides")]
    public async Task<ActionResult<IEnumerable<RideResponseDto>>> GetDriverRides(Guid userId)
    {
        var driver = await _uow.Drivers.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Driver not found");
        var rides = await _uow.Rides.GetByDriverAsync(driver.Id);
        return Ok(_mapper.Map<IEnumerable<RideResponseDto>>(rides));
    }

    [HttpPut("{userId:guid}/verify")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> VerifyDriver(Guid userId)
    {
        var driver = await _uow.Drivers.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Driver not found");
        driver.Verify();
        await _uow.CommitAsync();
        return Ok(new { message = "Driver verified", driverId = driver.Id });
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<IEnumerable<NearbyDriverDto>>> GetNearbyDrivers(
        [FromQuery] double lat, [FromQuery] double lng, [FromQuery] double radius = 5)
    {
        var cacheKey = $"drivers_nearby_{lat}_{lng}_{radius}";
        var cached = await _cache.GetAsync<List<NearbyDriverDto>>(cacheKey);
        if (cached != null)
            return Ok(cached);

        var drivers = await _uow.Drivers.GetNearbyDriversAsync(lat, lng, radius);
        var result = drivers.Select(d => new NearbyDriverDto
        {
            Id = d.Id,
            FirstName = d.User.FirstName,
            LastName = d.User.LastName,
            Rating = d.Rating,
            VehicleType = d.Vehicle?.Type.ToString(),
            VehicleMake = d.Vehicle?.Make,
            VehicleModel = d.Vehicle?.Model,
            VehicleColor = d.Vehicle?.Color,
            Latitude = d.CurrentLocation?.Latitude,
            Longitude = d.CurrentLocation?.Longitude
        }).ToList();

        await _cache.SetAsync(cacheKey, result, TimeSpan.FromSeconds(30));
        return Ok(result);
    }
}
