using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Commands;

public class StartRideHandler : IRequestHandler<StartRideCommand, RideResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public StartRideHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<RideResponseDto> Handle(StartRideCommand cmd, CancellationToken ct)
    {
        var ride = await _uow.Rides.GetByIdAsync(cmd.RideId)
            ?? throw new KeyNotFoundException("Ride not found");

        var driver = await _uow.Drivers.GetByUserIdAsync(cmd.DriverId)
            ?? throw new KeyNotFoundException("Driver not found");

        if (ride.DriverId != driver.Id)
            throw new RideSharing.Core.Exceptions.RideStateException("Only the assigned driver can start this ride");

        ride.StartRide();
        await _uow.Rides.UpdateAsync(ride);
        await _uow.CommitAsync();

        return _mapper.Map<RideResponseDto>(ride);
    }
}
