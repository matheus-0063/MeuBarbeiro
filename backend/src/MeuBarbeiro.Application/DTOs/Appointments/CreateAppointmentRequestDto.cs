namespace MeuBarbeiro.Application.DTOs.Appointments;

public class CreateAppointmentRequestDto
{
    public Guid BarbershopId { get; set; }
    public DateTime ScheduledAtUtc { get; set; } = DateTime.UtcNow;
    public decimal TotalPrice { get; set; }
}
