using Asp.Versioning;
using MeuBarbeiro.Api.Models.Responses;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.DTOs.Appointments;
using MeuBarbeiro.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MeuBarbeiro.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/appointment")]
public class AppointmentController(IAppointmentService appointmentService) : BaseController
{
    [HttpPost]
    [ProducesResponseType<AppointmentIdResponseModel>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequestDto request)
    {
        var result = await appointmentService.CreateAppointment(request);

        if (ResponseHasErros(result.ValidationResult))
        {
            return ValidationProblem();
        }

        return CreatedAtAction(
            nameof(GetAppointment),
            new { version = "1.0", appointmentId = result.Data },
            new AppointmentIdResponseModel { AppointmentId = result.Data! });
    }

    [HttpGet("{appointmentId:guid}")]
    [ProducesResponseType<AppointmentResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointment(Guid appointmentId)
    {
        var result = await appointmentService.GetAppointment(appointmentId);

        if (result.IsNotFound)
        {
            return NotFound();
        }

        return Ok(result.Data);
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<AppointmentResponseDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAppointments([FromQuery] Guid userId, [FromQuery] AppointmentUserType userType, [FromQuery] AppointmentStatus? status)
    {
        var dataValidationResult = await appointmentService.GetListAppointments(userId, userType, status);

        return ResponseHasErros(dataValidationResult.ValidationResult)
            ? ValidationProblem()
            : Ok(dataValidationResult.Data);
    }

    [HttpPatch("{appointmentId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(Guid appointmentId, [FromBody] UpdateAppointmentStatusRequestDto request)
    {
        request.AppointmentId = appointmentId;
        var result = await appointmentService.UpdateStatusAppointment(request);

        if (result.IsNotFound)
        {
            return NotFound();
        }

        return ResponseHasErros(result.ValidationResult)
            ? ValidationProblem()
            : NoContent();
    }
}
