namespace MeuBarbeiro.Application.DTOs.Appointments;

public class AppointmentReviewResponseDto
{
    public Guid Id { get; set; }
    public Guid AppointmentId { get; set; }
    public int Stars { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}