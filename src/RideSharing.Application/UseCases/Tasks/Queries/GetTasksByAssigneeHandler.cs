using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Tasks.Queries;

public class GetTasksByAssigneeHandler : IRequestHandler<GetTasksByAssigneeQuery, IEnumerable<TaskResponseDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetTasksByAssigneeHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TaskResponseDto>> Handle(GetTasksByAssigneeQuery query, CancellationToken ct)
    {
        var tasks = await _uow.Tasks.GetByAssigneeAsync(query.UserId);
        return _mapper.Map<IEnumerable<TaskResponseDto>>(tasks);
    }
}
