using MeuBarbeiro.Domain.Enums;

namespace MeuBarbeiro.Domain.Entities;

public sealed class Appointment
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid BarberId { get; private set; }
    public Guid BarbershopId { get; private set; }
    public DateTime ScheduledAtUtc { get; private set; }
    public decimal TotalPrice { get; private set; }
    public AppointmentStatus Status { get; private set; }

    public Appointment() { }

    public Appointment(Guid clientId, Guid barberId, Guid barbershopId, DateTime scheduledAtUtc, decimal totalPrice)
    {
        if (clientId == Guid.Empty) throw new ArgumentNullException(nameof(clientId));
        if (barberId == Guid.Empty) throw new ArgumentNullException(nameof(barberId));
        if (barbershopId == Guid.Empty) throw new ArgumentNullException(nameof(barbershopId));
        
        Id = Guid.NewGuid();
        ClientId = clientId;
        BarberId = barberId;
        BarbershopId = barbershopId;
        ScheduledAtUtc = scheduledAtUtc;
        TotalPrice = totalPrice;
        Status = AppointmentStatus.Pending;
    }

    public void Accept(Guid barberId)
    {
        VerifyBarberId(barberId);
        if (Status != AppointmentStatus.Pending) throw new InvalidOperationException();
        
        Status = AppointmentStatus.Accepted;
    }

    public void Reject(Guid barberId)
    {
        VerifyBarberId(barberId);
        if (Status != AppointmentStatus.Pending) throw new InvalidOperationException();
        
        Status = AppointmentStatus.Rejected;
    }

    public void Cancel(Guid userId, DateTime nowUtc)
    {
        if (userId == Guid.Empty || (userId != BarberId && userId != ClientId)) throw new ArgumentException(nameof(userId));
        if (nowUtc >= ScheduledAtUtc.AddHours(-2)) throw new InvalidOperationException();
        if(Status == AppointmentStatus.Cancelled || Status == AppointmentStatus.Completed || Status == AppointmentStatus.Rejected) throw new InvalidOperationException();
        
        Status = AppointmentStatus.Cancelled;
    }

    public void Start(Guid barberId)
    {
        VerifyBarberId(barberId);
        if (Status != AppointmentStatus.Accepted) throw new InvalidOperationException();
        
        Status = AppointmentStatus.InProgress;
    }

    public void Complete(Guid barberId)
    {
        VerifyBarberId(barberId);
        if(Status != AppointmentStatus.InProgress) throw new InvalidOperationException();
        
        Status = AppointmentStatus.Completed;
    }

    private void VerifyBarberId(Guid barberId)
    {
        if (barberId == Guid.Empty) throw new ArgumentException(nameof(barberId));
        if (BarberId != barberId) throw new ArgumentException(nameof(barberId));
    }
}
