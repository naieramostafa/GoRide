using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Commands;

public class AcceptRideHandler : IRequestHandler<AcceptRideCommand, RideResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public AcceptRideHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<RideResponseDto> Handle(AcceptRideCommand cmd, CancellationToken ct)
    {
        var ride = await _uow.Rides.GetByIdAsync(cmd.Accept.RideId)
            ?? throw new KeyNotFoundException("Ride not found");

        var driver = await _uow.Drivers.GetByUserIdAsync(cmd.Accept.DriverId)
            ?? throw new KeyNotFoundException("Driver not found. Register as a Driver first and add a vehicle.");

        ride.AssignDriver(driver);
        await _uow.Rides.UpdateAsync(ride);
        await _uow.CommitAsync();

        return _mapper.Map<RideResponseDto>(ride);
    }
}
