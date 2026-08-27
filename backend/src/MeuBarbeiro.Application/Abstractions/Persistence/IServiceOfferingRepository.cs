using FluentValidation.Results;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Abstractions.Persistence;

public interface IServiceOfferingRepository
{
    Task<ServiceOffering?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(ServiceOffering serviceOffering, CancellationToken cancellationToken = default);

    Task UpdateAsync(ServiceOffering serviceOffering,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ServiceOffering>> ListByIdsAsync(IEnumerable<Guid> serviceOfferingIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ServiceOffering>> ListByBarbershopAsync(Guid barbershopId,
        CancellationToken cancellationToken = default);
}