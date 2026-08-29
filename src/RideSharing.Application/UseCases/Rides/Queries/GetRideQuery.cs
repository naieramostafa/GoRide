using MediatR;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Queries;

public record GetRideQuery(Guid RideId) : IRequest<RideResponseDto>;
public record GetActiveRidesQuery : IRequest<IEnumerable<RideResponseDto>>;
