namespace MeuBarbeiro.Domain.Entities;

public sealed class Review
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid AppointmentId { get; set; }
    public Guid ClientId { get; set; }
    public Guid BarberId { get; set; }
    public Guid BarbershopId { get; set; }
    public int Stars { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
