using FluentValidation.Results;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Abstractions.Persistence;

public interface IBarberRepository
{
    Task AddAsync(Barber barber, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(Barber barber, CancellationToken cancellationToken = default);
    
    Task<Barber?> GetByIdAsync(Guid barberId, CancellationToken cancellationToken = default);
    
    Task<Barber?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Barber>> ListByBarbershopAsync(Guid barbershopId,
        CancellationToken cancellationToken = default);
}