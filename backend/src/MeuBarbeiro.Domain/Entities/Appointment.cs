using MeuBarbeiro.Domain.Enums;
using MeuBarbeiro.Domain.Exceptions;

namespace MeuBarbeiro.Domain.Entities;

public sealed class Appointment
{
    public Appointment()
    {
    }

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

    public Guid Id { get; }
    public Guid ClientId { get; }
    public Guid BarberId { get; }
    public Guid BarbershopId { get; private set; }
    public DateTime ScheduledAtUtc { get; }
    public decimal TotalPrice { get; private set; }
    public AppointmentStatus Status { get; private set; }

    public void Accept(Guid barberId)
    {
        VerifyBarberId(barberId);
        if (Status != AppointmentStatus.Pending)
            throw new AppointmentStatusTransitionException(Status, AppointmentStatus.Accepted);

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
        if (userId == Guid.Empty || (userId != BarberId && userId != ClientId))
            throw new ArgumentException(null, nameof(userId));
        if (nowUtc >= ScheduledAtUtc.AddHours(-2)) throw new InvalidOperationException();
        if (Status is AppointmentStatus.Cancelled or AppointmentStatus.Completed or AppointmentStatus.Rejected)
            throw new InvalidOperationException();

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
        if (Status != AppointmentStatus.InProgress) throw new InvalidOperationException();

        Status = AppointmentStatus.Completed;
    }

    private void VerifyBarberId(Guid barberId)
    {
        if (barberId == Guid.Empty) throw new ArgumentException(null, nameof(barberId));
        if (BarberId != barberId) throw new AppointmentActorNotAllowedException(Id, BarberId, barberId);
    }
}