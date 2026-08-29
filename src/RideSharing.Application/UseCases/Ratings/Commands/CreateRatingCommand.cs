using MediatR;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Ratings.Commands;

public record CreateRatingCommand(CreateRatingDto Rating) : IRequest<RatingResponseDto>;
