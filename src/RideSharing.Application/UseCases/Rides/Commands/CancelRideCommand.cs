using MediatR;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Commands;

public record CancelRideCommand(RideCancelDto Request) : IRequest<RideResponseDto>;
