using Asp.Versioning;
using MeuBarbeiro.Api.Models.Responses;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.DTOs.Barbershop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuBarbeiro.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/barbershop")]
public class BarbershopController(IBarbershopService barbershopService) : BaseController
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<BarbershopResponseDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBarbershops([FromQuery] string? city = null, CancellationToken cancellationToken = default)
    {
        var result = await barbershopService.GetBarbershops(city, cancellationToken);
        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Barber")]
    [ProducesResponseType<BarbershopIdResponseModel>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBarbershop([FromBody] CreateBarbershopRequestDto request)
    {
        var result = await barbershopService.CreateBarbershop(request);
        
        if (ResponseHasErros(result.ValidationResult)) return ValidationProblem();
        
        return CreatedAtAction(nameof(GetBarbershop), new { version = "1.0", barbershopId = result.Data }, 
            new BarbershopIdResponseModel() { BarbershopId = result.Data! });
    }
    
    [HttpGet("{barbershopId:guid}")]
    [ProducesResponseType<BarbershopResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBarbershop(Guid barbershopId)
    {
        var result = await barbershopService.GetBarbershop(barbershopId);

        if (result.IsNotFound) return NotFound();
        return Ok(result.Data);
    }
}
