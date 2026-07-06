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

    public Appointment() { }

    public Appointment(Guid id, Guid clientId, Guid barberId, Guid barbershopId, DateTime scheduledAtUtc, decimal totalPrice, AppointmentStatus status)
    {
        Id = id;
        ClientId = clientId;
        BarberId = barberId;
        BarbershopId = barbershopId;
        ScheduledAtUtc = scheduledAtUtc;
        TotalPrice = totalPrice;
        Status = status;
    }

    public void SetStatus(AppointmentStatus newStatus) => Status = newStatus;
}
