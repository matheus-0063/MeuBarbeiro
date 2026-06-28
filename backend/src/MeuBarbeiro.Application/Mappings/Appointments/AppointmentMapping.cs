using MeuBarbeiro.Application.DTOs.Appointments;
using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Domain.Enums;

namespace MeuBarbeiro.Application.Mappings.Appointments;

public static class AppointmentMapping
{
    public static Appointment ToEntity(this CreateAppointmentRequestDto request, Guid clientId, Guid barberId) => new Appointment
    {
        Id = Guid.NewGuid(),
        ClientId = clientId,
        BarberId = barberId,
        BarbershopId = request.BarbershopId,
        ScheduledAtUtc = request.ScheduledAtUtc,
        TotalPrice = request.TotalPrice,
        Status = AppointmentStatus.Pending
    };

    public static AppointmentResponseDto ToResponseDto(this Appointment entity) => new AppointmentResponseDto
    {
        Id = entity.Id,
        ClientId = entity.ClientId,
        BarberId = entity.BarberId,
        BarbershopId = entity.BarbershopId,
        ScheduledAtUtc = entity.ScheduledAtUtc,
        TotalPrice = entity.TotalPrice,
        Status = entity.Status.ToString()
    };
}
