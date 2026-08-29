using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Tasks.Queries;

public class GetAllTasksHandler : IRequestHandler<GetAllTasksQuery, IEnumerable<TaskResponseDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllTasksHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TaskResponseDto>> Handle(GetAllTasksQuery query, CancellationToken ct)
    {
        var tasks = await _uow.Tasks.GetAllAsync();
        return _mapper.Map<IEnumerable<TaskResponseDto>>(tasks);
    }
}
