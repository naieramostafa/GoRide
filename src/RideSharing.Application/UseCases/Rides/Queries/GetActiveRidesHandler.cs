using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Rides.Queries;

public class GetActiveRidesHandler : IRequestHandler<GetActiveRidesQuery, IEnumerable<RideResponseDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetActiveRidesHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<RideResponseDto>> Handle(GetActiveRidesQuery query, CancellationToken ct)
    {
        var rides = await _uow.Rides.GetActiveRidesAsync();
        return _mapper.Map<IEnumerable<RideResponseDto>>(rides);
    }
}
