using FluentValidation;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.Validators;

public class RideRequestValidator : AbstractValidator<RideRequestDto>
{
    public RideRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PickupLatitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.PickupLongitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.DropoffLatitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.DropoffLongitude).InclusiveBetween(-180, 180);
    }
}
