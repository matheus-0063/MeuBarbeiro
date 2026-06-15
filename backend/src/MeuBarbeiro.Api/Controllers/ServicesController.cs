using Asp.Versioning;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.DTOs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuBarbeiro.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/services")]
public class ServicesController(
    IServicesService servicesService,
    IBarberRepository barberRepository) : BaseController
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<ServiceResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServices([FromQuery] Guid barbershopId, CancellationToken cancellationToken = default)
    {
        var result = await servicesService.GetServices(barbershopId, cancellationToken);

        if (result.IsNotFound)
        {
            return NotFound();
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Barber")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddServices([FromBody] AddServicesRequestDto request, CancellationToken cancellationToken)
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
                Title = "O barbeiro autenticado ainda não possui uma barbearia vinculada."
            });
        }

        if (barber.BarbershopId.Value != request.BarbershopId)
        {
            return Forbid();
        }

        var result = await servicesService.AddServices(request);
        if (ResponseHasErros(result.ValidationResult))
        {
            return ValidationProblem();
        }

        return CreatedAtAction(nameof(GetServices), new { version = "1.0", barbershopId = request.BarbershopId }, new
        {
            serviceId = result.Data
        });
    }
}
