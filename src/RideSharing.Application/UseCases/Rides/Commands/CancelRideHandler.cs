using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Commands;

public class CancelRideHandler : IRequestHandler<CancelRideCommand, RideResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CancelRideHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<RideResponseDto> Handle(CancelRideCommand cmd, CancellationToken ct)
    {
        var ride = await _uow.Rides.GetByIdAsync(cmd.Request.RideId)
            ?? throw new KeyNotFoundException("Ride not found");

        var passenger = await _uow.Passengers.GetByUserIdAsync(cmd.UserId);
        var isPassenger = passenger != null && ride.PassengerId == passenger.Id;
        var isDriver = ride.DriverId.HasValue && ride.Driver?.UserId == cmd.UserId;

        if (!isPassenger && !isDriver)
            throw new UnauthorizedAccessException("Only the passenger or assigned driver can cancel this ride");

        ride.Cancel(cmd.Request.Reason);
        await _uow.CommitAsync();

        return _mapper.Map<RideResponseDto>(ride);
    }
}

