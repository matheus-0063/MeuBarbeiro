using Asp.Versioning;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.DTOs.Barbershop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuBarbeiro.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/barbershop")]
public class BarbershopController(
    IBarbershopService barbershopService) : BaseController
{
    [HttpPost("barbershop")]
    [Authorize(Roles = "BarbershopOwner")]
    [ProducesResponseType<BarbershopResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateBarbershop(CreateBarbershopRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();

        var result = await barbershopService.CreateBarbershop(request, userId, cancellationToken);

        return ResponseHasErros(result.ValidationResult)
            ? ValidationProblem()
            : Ok(result.Data);
    }

    [HttpPut("barbershop/{barbershopId:guid}")]
    [Authorize(Roles = "BarbershopOwner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateBarbershop(UpdateBarbershopRequestDto request, Guid barbershopId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();

        var result = await barbershopService.UpdateBarbershop(request, barbershopId, userId, cancellationToken);

        if (result.IsNotFound) return NotFound();
        if (result.IsForbidden) return Forbid();

        return ResponseHasErros(result.ValidationResult)
            ? ValidationProblem()
            : NoContent();
    }

    [HttpPut("barbershop/{barbershopId:guid}/link-barber/{barberId:guid}")]
    [Authorize(Roles = "BarbershopOwner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LinkBaberToTheBarbershop(Guid barbershopId, Guid barberId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();

        var result =
            await barbershopService.LinkBaberToTheBarbershop(barbershopId, barberId, userId, cancellationToken);

        if (result.IsNotFound) return NotFound();
        if (result.IsForbidden) return Forbid();

        return ResponseHasErros(result.ValidationResult)
            ? ValidationProblem()
            : NoContent();
    }

    [HttpPut("barbershop/{barbershopId:guid}/remove-barber/{barberId:guid}")]
    [Authorize(Roles = "BarbershopOwner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveBarberToBarbershop(Guid barbershopId, Guid barberId,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();

        var result =
            await barbershopService.RemoveBarberToBarbershop(barbershopId, barberId, userId, cancellationToken);

        if (result.IsNotFound) return NotFound();
        if (result.IsForbidden) return Forbid();

        return ResponseHasErros(result.ValidationResult)
            ? ValidationProblem()
            : NoContent();
    }

    [HttpGet("my-barbershops")]
    [Authorize(Roles = "BarbershopOwner")]
    [ProducesResponseType<IEnumerable<BarbershopResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyBarbershops(CancellationToken cancellationToken = default)
    {
        if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();

        var result = await barbershopService.ListBarbershopsToOwner(userId, cancellationToken);

        if (result.IsNotFound) return NotFound();

        return ResponseHasErros(result.ValidationResult)
            ? ValidationProblem()
            : Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<BarbershopResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBarbershops([FromQuery] string? city = null,
        CancellationToken cancellationToken = default)
    {
        var result = await barbershopService.GetBarbershops(city, cancellationToken);
        if (result.IsNotFound) return NotFound();

        return ResponseHasErros(result.ValidationResult)
            ? ValidationProblem()
            : Ok(result.Data);
    }

    [HttpGet("barbers/{barbershopId:guid}")]
    [ProducesResponseType<IEnumerable<BarbershopResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBarbers(Guid barbershopId,
        CancellationToken cancellationToken = default)
    {
        var result = await barbershopService.ListBarbersToBarbershop(barbershopId, cancellationToken);

        if (result.IsNotFound) return NotFound();

        return ResponseHasErros(result.ValidationResult)
            ? ValidationProblem()
            : Ok(result.Data);
    }

    [HttpGet("{barbershopId:guid}")]
    [ProducesResponseType<BarbershopResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBarbershop(Guid barbershopId, CancellationToken cancellationToken)
    {
        var result = await barbershopService.GetBarbershop(barbershopId, cancellationToken);

        if (result.IsNotFound) return NotFound();
        return ResponseHasErros(result.ValidationResult)
            ? ValidationProblem()
            : Ok(result);
    }
}