using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence.Repositories;

public class SqliteBarberRepository(AppDbContext dbContext) : IBarberRepository
{
    public async Task<Barber?> GetByIdAsync(Guid barberId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Barbers
            .FirstOrDefaultAsync(barber => barber.Id == barberId, cancellationToken);
    }

    public async Task<Barber?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Barbers
            .FirstOrDefaultAsync(barber => barber.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Barber>> ListByBarbershopAsync(Guid barbershopId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Barbers
            .Where(barber => barber.BarbershopId == barbershopId)
            .ToListAsync(cancellationToken);
    }

    public async Task<ValidationResult> AddAsync(Barber barber, CancellationToken cancellationToken = default)
    {
        dbContext.Barbers.Add(barber);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ValidationResult();
    }
}
