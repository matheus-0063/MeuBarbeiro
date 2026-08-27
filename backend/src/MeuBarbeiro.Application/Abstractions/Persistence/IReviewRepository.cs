using FluentValidation.Results;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Abstractions.Persistence;

public interface IReviewRepository
{
    Task<Review?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Review>> ListByAppointmentIdsAsync(IEnumerable<Guid> appointmentIds,
        CancellationToken cancellationToken = default);

    Task<double?> GetAverageStarsByBarbershopAsync(Guid barbershopId, CancellationToken cancellationToken = default);
    Task<ValidationResult> AddAsync(Review review, CancellationToken cancellationToken = default);
}