using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence.Repositories;

public class SqliteServiceOfferingRepository(AppDbContext dbContext) : IServiceOfferingRepository
{
    public async Task<ValidationResult> AddAsync(ServiceOffering serviceOffering, CancellationToken cancellationToken = default)
    {
        dbContext.ServiceOfferings.Add(serviceOffering);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ValidationResult();
    }

    public async Task<IReadOnlyCollection<ServiceOffering>> ListByBarbershopAsync(Guid barbershopId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ServiceOfferings
            .Where(service => service.BarbershopId == barbershopId)
            .OrderBy(service => service.Name)
            .ToListAsync(cancellationToken);
    }
}
