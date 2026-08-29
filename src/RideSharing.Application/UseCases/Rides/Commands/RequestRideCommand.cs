using MediatR;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Commands;

public record RequestRideCommand(RideRequestDto Request) : IRequest<RideResponseDto>;
