using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence.Repositories;

public class ReviewRepository(AppDbContext dbContext) : IReviewRepository
{
    public async Task<Review?> GetByAppointmentIdAsync(Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Reviews
            .FirstOrDefaultAsync(review => review.AppointmentId == appointmentId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Review>> ListByAppointmentIdsAsync(IEnumerable<Guid> appointmentIds,
        CancellationToken cancellationToken = default)
    {
        var ids = appointmentIds.Distinct().ToArray();

        return await dbContext.Reviews
            .Where(review => ids.Contains(review.AppointmentId))
            .ToListAsync(cancellationToken);
    }

    public async Task<double?> GetAverageStarsByBarbershopAsync(Guid barbershopId,
        CancellationToken cancellationToken = default)
    {
        var stars = await dbContext.Reviews
            .Where(review => review.BarbershopId == barbershopId)
            .Select(review => (double?)review.Stars)
            .ToListAsync(cancellationToken);

        if (stars.Count == 0) return null;

        return stars.Average();
    }

    public async Task AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        dbContext.Reviews.Add(review);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}