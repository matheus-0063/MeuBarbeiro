using MeuBarbeiro.Domain.Enums;

namespace MeuBarbeiro.Domain.Entities;

public sealed class Appointment
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid BarberId { get; set; }
    public Guid BarbershopId { get; set; }
    public DateTime ScheduledAtUtc { get; set; }
    public decimal TotalPrice { get; set; }
    public AppointmentStatus Status { get; set; }
    
    public void AlterStatus(AppointmentStatus newStatus) => Status = newStatus;
}
