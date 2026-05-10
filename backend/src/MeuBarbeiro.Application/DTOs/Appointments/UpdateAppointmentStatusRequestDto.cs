using MeuBarbeiro.Domain.Enums;

namespace MeuBarbeiro.Application.DTOs.Appointments;

public class UpdateAppointmentStatusRequestDto
{
    public Guid AppointmentId { get; set; }
    public AppointmentStatus Status { get; set; }
}