using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence.Repositories;

public class SqliteBarbershopRepository(AppDbContext dbContext) : IBarbershopRepository
{
    public async Task<ValidationResult> AddAsync(Barbershop barbershop, CancellationToken cancellationToken = default)
    {
        dbContext.Barbershops.Add(barbershop);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ValidationResult();
    }

    public async Task<Barbershop?> GetByIdAsync(Guid barbershopId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Barbershops
            .FirstOrDefaultAsync(barbershop => barbershop.Id == barbershopId, cancellationToken);
    }

    public async Task<ValidationResult> UpdateAsync(Barbershop barbershop, CancellationToken cancellationToken = default)
    {
        dbContext.Barbershops.Update(barbershop);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ValidationResult();
    }

    public async Task<IReadOnlyCollection<Barbershop>> ListAsync(string? city = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Barbershops.AsQueryable();

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(barbershop => barbershop.City == city);
        }

        return await query
            .OrderBy(barbershop => barbershop.Name)
            .ToListAsync(cancellationToken);
    }
}
