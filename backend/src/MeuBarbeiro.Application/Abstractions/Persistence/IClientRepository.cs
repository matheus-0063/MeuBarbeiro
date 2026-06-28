using FluentValidation.Results;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Abstractions.Persistence;

public interface IClientRepository
{
    Task<Client?> GetByIdAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<Client?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Client>> ListByIdsAsync(IEnumerable<Guid> clientIds, CancellationToken cancellationToken = default);
    Task<ValidationResult> AddAsync(Client client, CancellationToken cancellationToken = default);
}
