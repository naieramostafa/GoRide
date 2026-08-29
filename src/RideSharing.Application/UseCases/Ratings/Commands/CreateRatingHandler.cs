using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Entities;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Ratings.Commands;

public class CreateRatingHandler : IRequestHandler<CreateRatingCommand, RatingResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateRatingHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<RatingResponseDto> Handle(CreateRatingCommand cmd, CancellationToken ct)
    {
        var ride = await _uow.Rides.GetByIdAsync(cmd.Rating.RideId)
            ?? throw new KeyNotFoundException("Ride not found");

        var rating = new Rating(ride, cmd.Rating.RatedByUserId, cmd.Rating.RatedUserId,
                                cmd.Rating.Score, cmd.Rating.Comment);
        await _uow.Ratings.AddAsync(rating);

        var driver = await _uow.Drivers.GetByIdAsync(cmd.Rating.RatedUserId);
        if (driver != null)
        {
            driver.UpdateRating(cmd.Rating.Score);
            await _uow.Drivers.UpdateAsync(driver);
        }

        await _uow.CommitAsync();

        return _mapper.Map<RatingResponseDto>(rating);
    }
}
