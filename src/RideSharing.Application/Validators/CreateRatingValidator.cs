using FluentValidation;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.Validators;

public class CreateRatingValidator : AbstractValidator<CreateRatingDto>
{
    public CreateRatingValidator()
    {
        RuleFor(x => x.RideId).NotEmpty();
        RuleFor(x => x.RatedByUserId).NotEmpty();
        RuleFor(x => x.RatedUserId).NotEmpty();
        RuleFor(x => x.Score).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).MaximumLength(500);
    }
}
