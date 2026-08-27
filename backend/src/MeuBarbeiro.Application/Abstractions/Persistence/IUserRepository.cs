using FluentValidation.Results;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<User>> ListByIdsAsync(IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default);

    Task<ValidationResult> AddAsync(User user, CancellationToken cancellationToken = default);
}