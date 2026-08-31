using FluentValidation.Results;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence.Repositories;

public class SqliteAppointmentServiceSelectionRepository(AppDbContext dbContext)
    : IAppointmentServiceSelectionRepository
{
    public async Task AddRangeAsync(IEnumerable<AppointmentServiceSelection> selections,
        CancellationToken cancellationToken = default)
    {
        dbContext.Set<AppointmentServiceSelection>().AddRange(selections);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AppointmentServiceSelection>> ListByAppointmentIdsAsync(
        IEnumerable<Guid> appointmentIds, CancellationToken cancellationToken = default)
    {
        var ids = appointmentIds.Distinct().ToArray();

        return await dbContext.Set<AppointmentServiceSelection>()
            .Where(selection => ids.Contains(selection.AppointmentId))
            .ToListAsync(cancellationToken);
    }
}