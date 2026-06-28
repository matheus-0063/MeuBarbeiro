namespace MeuBarbeiro.Application.DTOs.Appointments;

public class AppointmentSelectedServiceDto
{
    public Guid ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
}
