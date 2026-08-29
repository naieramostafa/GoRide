using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Entities;
using RideSharing.Core.ValueObjects;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Commands;

public class RequestRideHandler : IRequestHandler<RequestRideCommand, RideResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IMapService _mapService;

    public RequestRideHandler(IUnitOfWork uow, IMapper mapper, IMapService mapService)
    {
        _uow = uow;
        _mapper = mapper;
        _mapService = mapService;
    }

    public async Task<RideResponseDto> Handle(RequestRideCommand cmd, CancellationToken ct)
    {
        var passenger = await _uow.Passengers.GetByUserIdAsync(cmd.Request.UserId)
            ?? throw new KeyNotFoundException("Passenger not found. Register as a Passenger first.");

        var pickup = new Location(cmd.Request.PickupLatitude, cmd.Request.PickupLongitude, cmd.Request.PickupAddress);
        var dropoff = new Location(cmd.Request.DropoffLatitude, cmd.Request.DropoffLongitude, cmd.Request.DropoffAddress);

        var distance = await _mapService.CalculateDistanceAsync(pickup, dropoff);
        var duration = await _mapService.EstimateDurationAsync(pickup, dropoff);
        var baseFare = 2.5m;
        var perKmRate = 1.5m;
        var estimatedFare = baseFare + (decimal)distance * perKmRate;
        var fare = new Money(Math.Round(estimatedFare, 2));

        var ride = new Ride(passenger, pickup, dropoff, fare, distance, duration);
        await _uow.Rides.AddAsync(ride);
        await _uow.CommitAsync();

        return _mapper.Map<RideResponseDto>(ride);
    }
}
