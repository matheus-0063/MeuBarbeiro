using Asp.Versioning;
using MeuBarbeiro.Api.Models.Responses;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.DTOs.Appointments;
using MeuBarbeiro.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeuBarbeiro.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/appointment")]
public class AppointmentController(
    IAppointmentService appointmentService,
    IClientRepository clientRepository,
    IBarberRepository barberRepository) : BaseController
{
    [Authorize(Roles = "Client")]
    [HttpPost]
    [ProducesResponseType<AppointmentIdResponseModel>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();

        var client = await clientRepository.GetByUserIdAsync(userId, cancellationToken);
        if (client is null)
            return NotFound(new ProblemDetails
            {
                Title = "Perfil de cliente não encontrado."
            });

        var result = await appointmentService.CreateAppointment(request, client.Id, cancellationToken);

        if (ResponseHasErros(result.ValidationResult)) return ValidationProblem();

        return CreatedAtAction(nameof(GetAppointment), new { version = "1.0", appointmentId = result.Data },
            new AppointmentIdResponseModel { AppointmentId = result.Data! });
    }

    [Authorize(Roles = "Client,Barber")]
    [HttpGet("{appointmentId:guid}")]
    [ProducesResponseType<AppointmentResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointment(Guid appointmentId, CancellationToken cancellationToken)
    {
        var result = await appointmentService.GetAppointmentAsync(appointmentId);

        if (result.IsNotFound) return NotFound();
        if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();

        if (User.IsInRole("Client"))
        {
            var client = await clientRepository.GetByUserIdAsync(userId, cancellationToken);
            if (client is null)
                return NotFound(new ProblemDetails { Title = "Perfil de cliente não encontrado." });

            if (result.Data!.ClientId != client.Id)
                return Forbid();
        }
        else if (User.IsInRole("Barber"))
        {
            var barber = await barberRepository.GetByUserIdAsync(userId, cancellationToken);
            if (barber is null)
                return NotFound(new ProblemDetails { Title = "Perfil de barbeiro não encontrado." });

            if (result.Data!.BarberId != barber.Id)
                return Forbid();
        }

        return Ok(result.Data);
    }

    [Authorize(Roles = "Client,Barber")]
    [HttpGet("mine")]
    [ProducesResponseType<IEnumerable<AppointmentResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointments([FromQuery] AppointmentStatus? status,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();

        Guid actorId;
        AppointmentUserType userType;

        if (User.IsInRole("Client"))
        {
            var client = await clientRepository.GetByUserIdAsync(userId, cancellationToken);
            if (client is null)
                return NotFound(new ProblemDetails
                {
                    Title = "Perfil de cliente não encontrado."
                });

            actorId = client.Id;
            userType = AppointmentUserType.Client;
        }
        else
        {
            var barber = await barberRepository.GetByUserIdAsync(userId, cancellationToken);
            if (barber is null)
                return NotFound(new ProblemDetails
                {
                    Title = "Perfil de barbeiro não encontrado."
                });

            actorId = barber.Id;
            userType = AppointmentUserType.Barber;
        }

        var dataValidationResult =
            await appointmentService.GetListAppointments(actorId, userType, status, cancellationToken);

        return ResponseHasErros(dataValidationResult.ValidationResult)
            ? ValidationProblem()
            : Ok(dataValidationResult.Data);
    }

    [Authorize(Roles = "Client")]
    [HttpPost("{appointmentId:guid}/review")]
    [ProducesResponseType<AppointmentReviewResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateReview(
        Guid appointmentId,
        [FromBody] CreateAppointmentReviewRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();

        var client = await clientRepository.GetByUserIdAsync(userId, cancellationToken);
        if (client is null)
            return NotFound(new ProblemDetails
            {
                Title = "Perfil de cliente não encontrado."
            });

        var result = await appointmentService.CreateReview(appointmentId, client.Id, request, cancellationToken);

        if (result.IsNotFound) return NotFound();

        if (ResponseHasErros(result.ValidationResult)) return ValidationProblem();

        return CreatedAtAction(nameof(GetAppointment), new { version = "1.0", appointmentId }, result.Data);
    }


    [Authorize(Roles = "Barber")]
    [HttpPatch("{appointmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptAppointment(Guid appointmentId, CancellationToken cancellationToken)
    {
        if (!TryGetAuthenticatedUserId(out var userId)) return Unauthorized();

        var result = await appointmentService.AcceptAppointment(appointmentId, userId, cancellationToken);

        if (result.IsNotFound) return NotFound();
        if (result.IsForbidden) return Forbid();

        return ResponseHasErros(result.ValidationResult)
            ? ValidationProblem()
            : NoContent();
    }
}