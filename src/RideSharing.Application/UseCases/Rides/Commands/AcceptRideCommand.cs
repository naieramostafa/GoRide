using MediatR;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Commands;

public record AcceptRideCommand(RideAcceptDto Accept) : IRequest<RideResponseDto>;
