using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Entities;
using RideSharing.Application.DTOs;

using RideSharing.Core.Enums;

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

        if (ride.Status != RideStatus.Completed)
            throw new RideSharing.Core.Exceptions.RideStateException("Can only rate completed rides");

        if (ride.DriverId != cmd.Rating.RatedUserId)
            throw new RideSharing.Core.Exceptions.RideStateException("Rated user must be the driver of the ride");

        var existingRatings = await _uow.Ratings.GetByRideAsync(cmd.Rating.RideId);
        if (existingRatings.Any(r => r.RatedByUserId == cmd.Rating.RatedByUserId))
            throw new RideSharing.Core.Exceptions.RideStateException("You have already rated this ride");

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
