using FluentValidation.Results;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Abstractions.Persistence;

public interface IAppointmentRepository
{
    Task<IReadOnlyCollection<Appointment>> ListByBarberAsync(Guid barberId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Appointment>> ListByClientAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task<ValidationResult> AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
    Task<ValidationResult> UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);
}
