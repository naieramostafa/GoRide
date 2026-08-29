using MediatR;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Commands;

public record StartRideCommand(Guid RideId, Guid DriverId) : IRequest<RideResponseDto>;
