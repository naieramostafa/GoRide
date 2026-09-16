using MediatR;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Commands;

public record RequestRideCommand(Guid PassengerId, RideRequestDto Request) : IRequest<RideResponseDto>;
