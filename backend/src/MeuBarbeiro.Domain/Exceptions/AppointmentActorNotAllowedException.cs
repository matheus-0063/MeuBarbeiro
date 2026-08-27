namespace MeuBarbeiro.Domain.Exceptions;

public sealed class AppointmentActorNotAllowedException(Guid appointmentId, Guid expectedBarberId, Guid attemptedBarberId) : DomainException($"O barbeiro informado não pode executar esta operação no agendamento '{appointmentId}'.")
{
    public Guid AppointmentId => appointmentId;
    public Guid ExpectedBarberId => expectedBarberId;
    public Guid AttemptedBarberId => attemptedBarberId;
}