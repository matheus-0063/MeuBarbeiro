using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence.Repositories;

public class SqliteClientRepository(AppDbContext dbContext) : IClientRepository
{
    public async Task<Client?> GetByIdAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Clients
            .FirstOrDefaultAsync(client => client.Id == clientId, cancellationToken);
    }

    public async Task<Client?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Clients
            .FirstOrDefaultAsync(client => client.UserId == userId, cancellationToken);
    }

    public async Task<ValidationResult> AddAsync(Client client, CancellationToken cancellationToken = default)
    {
        dbContext.Clients.Add(client);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ValidationResult();
    }
}
