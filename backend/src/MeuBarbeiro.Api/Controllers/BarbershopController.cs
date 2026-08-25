using Asp.Versioning;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.DTOs.Barbershop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuBarbeiro.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/barbershop")]
public class BarbershopController(
    IBarbershopService barbershopService,
    IBarberRepository barberRepository) : BaseController
{
    [HttpPost("barbershop")]
    [Authorize(Roles = "BarbershopOwner")]
    public async Task<IActionResult> CreateBarbershop(CreateBarbershopRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();
        
        var result = await barbershopService.CreateBarbershop(request, userId, cancellationToken);
        
        return Ok(result.Data);
    }
    
    [HttpGet]
    [ProducesResponseType<IEnumerable<BarbershopResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBarbershops([FromQuery] string? city = null, CancellationToken cancellationToken = default)
    {
        var result = await barbershopService.GetBarbershops(city, cancellationToken);
        return Ok(result.Data);
    }

    [HttpGet("me")]
    [Authorize(Roles = "Barber")]
    [ProducesResponseType<BarbershopResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyBarbershop(CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized();
        }

        var barber = await barberRepository.GetByUserIdAsync(userId, cancellationToken);
        if (barber is null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Perfil de barbeiro não encontrado."
            });
        }

        if (!barber.BarbershopId.HasValue)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Barbearia ainda não cadastrada para o barbeiro autenticado."
            });
        }

        var result = await barbershopService.GetBarbershop(barber.BarbershopId.Value, cancellationToken);
        return result.IsNotFound ? NotFound() : Ok(result.Data);
    }
    
    [HttpGet("{barbershopId:guid}")]
    [ProducesResponseType<BarbershopResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBarbershop(Guid barbershopId, CancellationToken cancellationToken)
    {
        var result = await barbershopService.GetBarbershop(barbershopId, cancellationToken);

        if (result.IsNotFound) return NotFound();
        return Ok(result.Data);
    }
}
