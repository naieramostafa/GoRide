using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideSharing.Application.DTOs;
using RideSharing.Application.UseCases.Payments.Commands;

namespace RideSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("{rideId:guid}/process")]
    public async Task<ActionResult<PaymentResponseDto>> ProcessPayment(Guid rideId, [FromBody] ProcessPaymentDto? dto)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        dto ??= new ProcessPaymentDto();
        var result = await _mediator.Send(new ProcessPaymentCommand(rideId, dto, userId));
        return Ok(result);
    }
}
