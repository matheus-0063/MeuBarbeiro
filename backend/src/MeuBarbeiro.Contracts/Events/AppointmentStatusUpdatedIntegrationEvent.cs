namespace MeuBarbeiro.Contracts.Events;

public sealed record AppointmentStatusUpdatedIntegrationEvent(
    Guid AppointmentId,
    Guid BarberId,
    string Status,
    DateTime UpdatedAtUtc);
