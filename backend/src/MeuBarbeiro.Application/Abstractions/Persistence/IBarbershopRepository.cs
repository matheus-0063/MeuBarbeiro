using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Abstractions.Persistence;

public interface IBarbershopRepository
{
    Task AddAsync(Barbershop barbershop, CancellationToken cancellationToken = default);
    Task UpdateAsync(Barbershop barbershop, CancellationToken cancellationToken = default);
    Task<Barbershop?> GetByIdAsync(Guid barbershopId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Barbershop>?> GetByBarbershopOwnerIdAsync(Guid barbershopOwnerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Barbershop>> ListByIdsAsync(IEnumerable<Guid> barbershopIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Barbershop>> ListAsync(string? city = null, CancellationToken cancellationToken = default);
}