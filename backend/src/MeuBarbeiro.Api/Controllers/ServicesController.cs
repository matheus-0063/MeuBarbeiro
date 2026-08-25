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
    IServicesService servicesService) : BaseController
{
    [HttpPost("barbershop/{barbershopId}")]
    [Authorize(Roles = "BarbershopOwner")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddServices([FromBody] CreateServicesRequestDto request, Guid barbershopId,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
            return Unauthorized();

        var result = await servicesService.CreateService(request, userId, barbershopId, cancellationToken);

        if (result.IsNotFound)
            return NotFound();

        if (result.IsForbidden)
            return Forbid();

        return ResponseHasErros(result.ValidationResult)
            ? ValidationProblem()
            : CreatedAtAction(nameof(GetService), new { result.Data?.Id });
    }

    [HttpPatch("barbershop/{barbershopId}/services/{serviceId}")]
    [Authorize(Roles = "BarbershopOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateServices([FromBody] UpdateServicesRequestDto request, [FromQuery] Guid barbershopId, [FromQuery] Guid serviceId,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();
        
        var result = await servicesService.UpdateService(request, userId, barbershopId, serviceId, cancellationToken);
        
        if (result.IsNotFound)
            return NotFound();
        
        if (result.IsForbidden)
            return Forbid();
        
        return ResponseHasErros(result.ValidationResult)
            ? ValidationProblem()
            : Ok(result.Data);
    }

    [HttpGet("{serviceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetService([FromQuery] Guid serviceId, CancellationToken cancellationToken)
    {
        var result = await servicesService.GetService(serviceId, cancellationToken);
        
        if(result.IsNotFound)
            return NotFound();
        
        return Ok(result.Data);
    }
    
    [HttpGet]
    [ProducesResponseType<IEnumerable<ServiceResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServices([FromQuery] Guid barbershopId,
        CancellationToken cancellationToken = default)
    {
        var result = await servicesService.GetServices(barbershopId, cancellationToken);

        if (result.IsNotFound)
            return NotFound();

        return Ok(result.Data);
    }
}