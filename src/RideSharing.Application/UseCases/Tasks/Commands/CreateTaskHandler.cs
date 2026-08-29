using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Entities;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Tasks.Commands;

public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, TaskResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateTaskHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<TaskResponseDto> Handle(CreateTaskCommand cmd, CancellationToken ct)
    {
        var task = new TaskItem(
            cmd.Task.Title,
            cmd.Task.Description,
            cmd.Task.Priority,
            cmd.Task.AssignedToUserId,
            cmd.Task.DueDate
        );

        await _uow.Tasks.AddAsync(task);
        await _uow.CommitAsync();

        return _mapper.Map<TaskResponseDto>(task);
    }
}
