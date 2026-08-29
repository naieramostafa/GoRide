using AutoMapper;
using RideSharing.Application.DTOs;
using RideSharing.Core.Entities;
using RideSharing.Core.ValueObjects;

namespace RideSharing.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserResponseDto>();
        CreateMap<Driver, UserResponseDto>();

        CreateMap<Ride, RideResponseDto>()
            .ForMember(d => d.PassengerName, o => o.MapFrom(s => s.Passenger.User.FirstName + " " + s.Passenger.User.LastName))
            .ForMember(d => d.DriverName, o => o.MapFrom(s => s.Driver != null ? s.Driver.User.FirstName + " " + s.Driver.User.LastName : null))
            .ForMember(d => d.Fare, o => o.MapFrom(s => s.Fare.Amount))
            .ForMember(d => d.FinalFare, o => o.MapFrom(s => s.FinalFare != null ? s.FinalFare.Amount : (decimal?)null));

        CreateMap<Payment, PaymentResponseDto>()
            .ForMember(d => d.Amount, o => o.MapFrom(s => s.Amount.Amount))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.Amount.Currency));

        CreateMap<TaskItem, TaskResponseDto>()
            .ForMember(d => d.AssignedToName, o => o.MapFrom(s => s.AssignedTo != null ? s.AssignedTo.FirstName + " " + s.AssignedTo.LastName : null));

        CreateMap<Rating, RatingResponseDto>();
    }
}
