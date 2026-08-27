using MeuBarbeiro.Application.DTOs.Appointments;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Mappings.Appointments;

public static class AppointmentMapping
{
    public static Appointment ToEntity(this CreateAppointmentRequestDto request, Guid clientId, Guid barberId,
        decimal totalPrice)
    {
        return new Appointment(
            clientId,
            barberId,
            request.BarbershopId,
            request.ScheduledAtUtc,
            totalPrice
        );
    }

    public static AppointmentResponseDto ToResponseDto(this Appointment entity)
    {
        return new AppointmentResponseDto
        {
            Id = entity.Id,
            ClientId = entity.ClientId,
            BarberId = entity.BarberId,
            BarbershopId = entity.BarbershopId,
            ScheduledAtUtc = entity.ScheduledAtUtc,
            Status = entity.Status.ToString()
        };
    }
}