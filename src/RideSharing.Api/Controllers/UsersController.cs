using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideSharing.Application.DTOs;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Enums;

namespace RideSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ITokenService _tokenService;
    private readonly ICacheService _cache;

    public UsersController(IUnitOfWork uow, IMapper mapper, ITokenService tokenService, ICacheService cache)
    {
        _uow = uow;
        _mapper = mapper;
        _tokenService = tokenService;
        _cache = cache;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterUserDto dto)
    {
        var existing = await _uow.Users.GetByEmailAsync(dto.Email);
        if (existing != null)
            return BadRequest(new { error = "Email already registered" });

        var user = new Core.Entities.User(dto.FirstName, dto.LastName, dto.Email, dto.Phone, dto.Role, dto.Password);
        await _uow.Users.AddAsync(user);

        if (dto.Role == UserRole.Passenger)
        {
            var passenger = new Core.Entities.Passenger(user);
            await _uow.Passengers.AddAsync(passenger);
        }

        await _uow.CommitAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(_tokenService.RefreshExpiryDays);
        await _uow.RefreshTokens.AddAsync(new Core.Entities.RefreshToken(user.Id, refreshToken, expiry));
        await _uow.CommitAsync();

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            User = _mapper.Map<UserResponseDto>(user)
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _uow.Users.GetByEmailAsync(dto.Email);
        if (user == null || !user.VerifyPassword(dto.Password))
            return Unauthorized(new { error = "Invalid email or password" });

        user.MarkLogin();
        await _uow.Users.UpdateAsync(user);

        await _uow.RefreshTokens.RevokeAllForUserAsync(user.Id);
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(_tokenService.RefreshExpiryDays);
        await _uow.RefreshTokens.AddAsync(new Core.Entities.RefreshToken(user.Id, refreshToken, expiry));
        await _uow.CommitAsync();

        return Ok(new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            User = _mapper.Map<UserResponseDto>(user)
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh(RefreshTokenDto dto)
    {
        var stored = await _uow.RefreshTokens.GetByTokenAsync(dto.RefreshToken);
        if (stored == null || !stored.IsActive)
            return Unauthorized(new { error = "Invalid or expired refresh token" });

        stored.Revoke();
        var user = stored.User;
        var accessToken = _tokenService.GenerateAccessToken(user);
        var newRefresh = _tokenService.GenerateRefreshToken();
        var expiry = DateTime.UtcNow.AddDays(_tokenService.RefreshExpiryDays);
        await _uow.RefreshTokens.AddAsync(new Core.Entities.RefreshToken(user.Id, newRefresh, expiry));
        await _uow.CommitAsync();

        return Ok(new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = newRefresh,
            User = _mapper.Map<UserResponseDto>(user)
        });
    }

    [HttpPost("register-driver")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> RegisterDriver(RegisterDriverDto dto)
    {
        var user = await _uow.Users.GetByIdAsync(dto.UserId)
            ?? throw new KeyNotFoundException("User not found");

        var vehicle = new Core.Entities.Vehicle(dto.Make, dto.Model, dto.Year, dto.Color,
                                  dto.LicensePlate, dto.VehicleType, dto.Capacity);
        var driver = new Core.Entities.Driver(user, dto.LicenseNumber, vehicle);
        await _uow.Drivers.AddAsync(driver);

        await _uow.CommitAsync();
        await _cache.RemoveAsync($"drivers_nearby_{dto.UserId}");

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, _mapper.Map<UserResponseDto>(user));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
    {
        var cacheKey = $"user_{id}";
        var cached = await _cache.GetAsync<UserResponseDto>(cacheKey);
        if (cached != null) return Ok(cached);

        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("User not found");

        var dto = _mapper.Map<UserResponseDto>(user);
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));
        return Ok(dto);
    }

    [HttpGet("{userId:guid}/rides")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<RideResponseDto>>> GetUserRides(Guid userId)
    {
        var cacheKey = $"user_rides_{userId}";
        var cached = await _cache.GetAsync<List<RideResponseDto>>(cacheKey);
        if (cached != null) return Ok(cached);

        var passenger = await _uow.Passengers.GetByUserIdAsync(userId);
        if (passenger == null)
            return Ok(Array.Empty<RideResponseDto>());

        var rides = await _uow.Rides.GetByPassengerAsync(passenger.Id);
        var dto = _mapper.Map<List<RideResponseDto>>(rides);
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(2));
        return Ok(dto);
    }
}
