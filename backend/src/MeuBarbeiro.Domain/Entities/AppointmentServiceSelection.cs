namespace MeuBarbeiro.Domain.Entities;

public sealed class AppointmentServiceSelection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AppointmentId { get; set; }
    public Guid ServiceOfferingId { get; set; }
}
