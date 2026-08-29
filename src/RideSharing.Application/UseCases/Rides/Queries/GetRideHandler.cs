using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Queries;

public class GetRideHandler : IRequestHandler<GetRideQuery, RideResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetRideHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<RideResponseDto> Handle(GetRideQuery query, CancellationToken ct)
    {
        var ride = await _uow.Rides.GetByIdAsync(query.RideId)
            ?? throw new KeyNotFoundException("Ride not found");
        return _mapper.Map<RideResponseDto>(ride);
    }
}
