using FluentValidation.Results;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Abstractions.Persistence;

public interface IBarbershopRepository
{
    Task<ValidationResult> AddAsync(Barbershop barbershop, CancellationToken cancellationToken = default);
    Task<Barbershop?> GetByIdAsync(Guid barbershopId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Barbershop>> ListAsync(string? city = null, CancellationToken cancellationToken = default);
}
