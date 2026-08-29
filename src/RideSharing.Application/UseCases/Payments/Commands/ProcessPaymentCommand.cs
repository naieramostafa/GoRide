using MediatR;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Payments.Commands;

public record ProcessPaymentCommand(Guid RideId, ProcessPaymentDto Dto) : IRequest<PaymentResponseDto>;
