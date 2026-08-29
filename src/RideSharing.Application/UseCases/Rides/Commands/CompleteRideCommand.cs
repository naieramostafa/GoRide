using MediatR;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Commands;

public record CompleteRideCommand(RideCompleteDto Complete) : IRequest<RideResponseDto>;
