namespace MeuBarbeiro.Application.DTOs.Appointments;

public class AppointmentResponseDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid BarberId { get; set; }
    public Guid BarbershopId { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
}
