using FluentValidation.Results;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Abstractions.Persistence;

public interface IServiceOfferingRepository
{
    Task<ValidationResult> AddAsync(ServiceOffering serviceOffering, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ServiceOffering>> ListByBarbershopAsync(Guid barbershopId, CancellationToken cancellationToken = default);
}
