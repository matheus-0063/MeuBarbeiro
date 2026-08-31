namespace MeuBarbeiro.Application.DTOs.Appointments;

public class CreateAppointmentRequestDto
{
    public Guid BarbershopId { get; set; }
    public Guid BarberId { get; set; }
    public List<Guid> ServiceIds { get; set; } = [];
    public DateTime ScheduledAtUtc { get; set; } = DateTime.UtcNow;
}