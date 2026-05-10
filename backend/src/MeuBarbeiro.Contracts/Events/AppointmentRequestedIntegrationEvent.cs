namespace MeuBarbeiro.Contracts.Events;

public sealed record AppointmentRequestedIntegrationEvent(
    Guid AppointmentId,
    Guid ClientId,
    Guid BarberId,
    Guid BarbershopId,
    DateTime ScheduledAtUtc,
    decimal TotalPrice);
