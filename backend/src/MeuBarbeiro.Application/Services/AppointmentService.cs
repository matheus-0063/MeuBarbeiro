using MeuBarbeiro.Application.Abstractions.Messaging;
using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Contracts.Events;
using MeuBarbeiro.Application.DTOs.Appointments;
using MeuBarbeiro.Application.DTOs.Shared;
using MeuBarbeiro.Application.Mappings.Appointments;
using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Domain.Enums;

namespace MeuBarbeiro.Application.Services;

public class AppointmentService(IAppointmentRepository appointmentRepository, IEventPublisher eventPublisher) : IAppointmentService
{
    public async Task<ServiceResult<Guid>> CreateAppointment(CreateAppointmentRequestDto request, Guid clientId)
    {
        var appointment = request.ToEntity(clientId);
        var validationResult = await appointmentRepository.AddAsync(appointment);

        if (!validationResult.IsValid)
        {
            return ServiceResult<Guid>.Failure(validationResult);
        }

        await eventPublisher.PublishAsync(new AppointmentRequestedIntegrationEvent(
            appointment.Id,
            appointment.ClientId,
            appointment.BarberId,
            appointment.BarbershopId,
            appointment.ScheduledAtUtc,
            appointment.TotalPrice));

        return ServiceResult<Guid>.Success(appointment.Id);
    }

    public async Task<ServiceResult<AppointmentResponseDto>> GetAppointment(Guid id)
    {
        var appointment = await appointmentRepository.GetByIdAsync(id);
        return appointment == null
            ? ServiceResult<AppointmentResponseDto>.NotFound()
            : ServiceResult<AppointmentResponseDto>.Success(appointment.ToResponseDto());
    }

    public async Task<ServiceResult<IEnumerable<AppointmentResponseDto>>> GetListAppointments(Guid userId, AppointmentUserType userType, AppointmentStatus? status = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Appointment> appointments;

        switch (userType)
        {
            case AppointmentUserType.Client:
                appointments = await appointmentRepository.ListByClientAsync(userId, cancellationToken);
                break;
            case AppointmentUserType.Barber:
                appointments = await appointmentRepository.ListByBarberAsync(userId, cancellationToken);
                break;
            default:
            {
                var validationResult = new ValidationResult();
                validationResult.Errors.Add(new ValidationFailure(nameof(userType), "Tipo de usuário inválido."));

                return ServiceResult<IEnumerable<AppointmentResponseDto>>.Failure(validationResult);
            }
        }

        if (status.HasValue)
        {
            appointments = appointments
                .Where(appointment => appointment.Status == status.Value)
                .ToArray();
        }

        var response = appointments.Select(appointment => appointment.ToResponseDto());
        return ServiceResult<IEnumerable<AppointmentResponseDto>>.Success(response);
    }

    public async Task<ServiceResult<bool>> UpdateStatusAppointment(UpdateAppointmentStatusRequestDto request)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId);
        if (appointment == null)
        {
            return ServiceResult<bool>.NotFound();
        }

        appointment.AlterStatus(request.Status);

        var validationResult = await appointmentRepository.UpdateAsync(appointment);

        if (!validationResult.IsValid)
        {
            return ServiceResult<bool>.Failure(validationResult);
        }

        await eventPublisher.PublishAsync(new AppointmentStatusUpdatedIntegrationEvent(
            appointment.Id,
            appointment.BarberId,
            appointment.Status.ToString(),
            DateTime.UtcNow));

        return ServiceResult<bool>.Success(true);
    }
}
