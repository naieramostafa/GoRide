using MediatR;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Tasks.Queries;

public record GetAllTasksQuery : IRequest<IEnumerable<TaskResponseDto>>;
public record GetTasksByAssigneeQuery(Guid UserId) : IRequest<IEnumerable<TaskResponseDto>>;
