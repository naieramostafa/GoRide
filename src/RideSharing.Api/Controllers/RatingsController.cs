using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RideSharing.Application.DTOs;
using RideSharing.Application.UseCases.Ratings.Commands;

namespace RideSharing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RatingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RatingsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<RatingResponseDto>> Create(CreateRatingDto dto)
    {
        var result = await _mediator.Send(new CreateRatingCommand(dto));
        return CreatedAtAction(nameof(Create), result);
    }
}
