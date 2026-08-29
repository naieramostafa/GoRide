using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Tasks.Commands;

public class UpdateTaskStatusHandler : IRequestHandler<UpdateTaskStatusCommand, TaskResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateTaskStatusHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<TaskResponseDto> Handle(UpdateTaskStatusCommand cmd, CancellationToken ct)
    {
        var task = await _uow.Tasks.GetByIdAsync(cmd.TaskId)
            ?? throw new KeyNotFoundException("Task not found");

        switch (cmd.Status.Status)
        {
            case Core.Enums.TaskItemStatus.InProgress:
                task.Start();
                break;
            case Core.Enums.TaskItemStatus.Completed:
                task.Complete();
                break;
            case Core.Enums.TaskItemStatus.Cancelled:
                task.Cancel();
                break;
        }

        await _uow.Tasks.UpdateAsync(task);
        await _uow.CommitAsync();

        return _mapper.Map<TaskResponseDto>(task);
    }
}
