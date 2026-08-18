using MeuBarbeiro.Domain.Enums;

namespace MeuBarbeiro.Domain.Exceptions;

public sealed class AppointmentStatusTransitionException(AppointmentStatus currentStatus, AppointmentStatus targetStatus) : DomainException($"A transição de {currentStatus} para {targetStatus} não é permitida.")
{
    public AppointmentStatus CurrentStatus => currentStatus;
    public AppointmentStatus TargetStatus => targetStatus;
}