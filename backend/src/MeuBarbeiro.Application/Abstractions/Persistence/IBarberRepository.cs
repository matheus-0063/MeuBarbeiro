using FluentValidation.Results;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Abstractions.Persistence;

public interface IBarberRepository
{
    Task<Barber?> GetByIdAsync(Guid barberId, CancellationToken cancellationToken = default);
    Task<Barber?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Barber>> ListByBarbershopAsync(Guid barbershopId, CancellationToken cancellationToken = default);
    Task<ValidationResult> AddAsync(Barber barber, CancellationToken cancellationToken = default);
    Task<ValidationResult> UpdateAsync(Barber barber, CancellationToken cancellationToken = default);
}
