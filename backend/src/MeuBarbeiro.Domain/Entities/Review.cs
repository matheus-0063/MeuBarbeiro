namespace MeuBarbeiro.Domain.Entities;

public sealed class Review
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid AppointmentId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid BarberId { get; private set; }
    public Guid BarbershopId { get; private set; }
    public int Stars { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
}
