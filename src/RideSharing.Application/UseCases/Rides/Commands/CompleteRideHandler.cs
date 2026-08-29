using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Entities;
using RideSharing.Core.ValueObjects;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Commands;

public class CompleteRideHandler : IRequestHandler<CompleteRideCommand, RideResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CompleteRideHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<RideResponseDto> Handle(CompleteRideCommand cmd, CancellationToken ct)
    {
        var ride = await _uow.Rides.GetByIdAsync(cmd.Complete.RideId)
            ?? throw new KeyNotFoundException("Ride not found");

        var driver = await _uow.Drivers.GetByUserIdAsync(cmd.Complete.DriverId);
        if (driver == null)
            throw new KeyNotFoundException("Driver not found");

        if (ride.DriverId != driver.Id)
            throw new RideSharing.Core.Exceptions.RideStateException("Only the assigned driver can complete this ride");

        var finalFare = new Money(cmd.Complete.FinalFare);
        ride.CompleteRide(cmd.Complete.DistanceKm, cmd.Complete.DurationMinutes, finalFare);

        var payment = new Payment(ride, finalFare);
        await _uow.Payments.AddAsync(payment);
        await _uow.Rides.UpdateAsync(ride);
        await _uow.CommitAsync();

        return _mapper.Map<RideResponseDto>(ride);
    }
}
