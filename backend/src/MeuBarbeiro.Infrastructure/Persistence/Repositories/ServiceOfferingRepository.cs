using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence.Repositories;

public class ServiceOfferingRepository(AppDbContext dbContext) : IServiceOfferingRepository
{
    public async Task<ServiceOffering?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.ServiceOfferings
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task AddAsync(ServiceOffering serviceOffering, CancellationToken cancellationToken = default)
    {
        dbContext.ServiceOfferings.Add(serviceOffering);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ServiceOffering serviceOffering, CancellationToken cancellationToken = default)
    {
        dbContext.ServiceOfferings.Update(serviceOffering);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ServiceOffering>> ListByIdsAsync(IEnumerable<Guid> serviceOfferingIds,
        CancellationToken cancellationToken = default)
    {
        var ids = serviceOfferingIds.Distinct().ToArray();

        return await dbContext.ServiceOfferings
            .Where(service => ids.Contains(service.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ServiceOffering>> ListByBarbershopAsync(Guid barbershopId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.ServiceOfferings
            .Where(service => service.BarbershopId == barbershopId)
            .OrderBy(service => service.Name)
            .ToListAsync(cancellationToken);
    }
}