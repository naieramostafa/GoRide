using MediatR;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Tasks.Commands;

public record CreateTaskCommand(CreateTaskDto Task) : IRequest<TaskResponseDto>;
public record UpdateTaskStatusCommand(Guid TaskId, UpdateTaskStatusDto Status) : IRequest<TaskResponseDto>;
