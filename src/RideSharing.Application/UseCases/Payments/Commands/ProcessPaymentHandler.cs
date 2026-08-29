using AutoMapper;
using MediatR;
using RideSharing.Application.Interfaces;
using RideSharing.Application.DTOs;

namespace RideSharing.Application.UseCases.Payments.Commands;

public class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, PaymentResponseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IPaymentService _paymentService;
    private readonly IMapper _mapper;

    public ProcessPaymentHandler(IUnitOfWork uow, IPaymentService paymentService, IMapper mapper)
    {
        _uow = uow;
        _paymentService = paymentService;
        _mapper = mapper;
    }

    public async Task<PaymentResponseDto> Handle(ProcessPaymentCommand cmd, CancellationToken ct)
    {
        var payment = await _uow.Payments.GetByRideIdAsync(cmd.RideId)
            ?? throw new KeyNotFoundException("Payment not found for this ride");

        var confirm = cmd.Dto.Confirm ?? false;
        var (intentId, clientSecret, chargeId) = await _paymentService.CreatePaymentIntent(
            payment.Amount,
            payment.PassengerId.ToString(),
            cmd.Dto.PaymentMethod,
            confirm);

        if (chargeId != null)
        {
            payment.Complete(chargeId);
        }
        else
        {
            payment.Process(intentId);
        }

        await _uow.Payments.UpdateAsync(payment);
        await _uow.CommitAsync();

        var dto = _mapper.Map<PaymentResponseDto>(payment);
        dto.ClientSecret = clientSecret;
        return dto;
    }
}
