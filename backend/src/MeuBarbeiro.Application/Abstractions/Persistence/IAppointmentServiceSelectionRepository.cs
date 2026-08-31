using FluentValidation.Results;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Abstractions.Persistence;

public interface IAppointmentServiceSelectionRepository
{
    Task AddRangeAsync(IEnumerable<AppointmentServiceSelection> selections,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AppointmentServiceSelection>> ListByAppointmentIdsAsync(IEnumerable<Guid> appointmentIds,
        CancellationToken cancellationToken = default);
}