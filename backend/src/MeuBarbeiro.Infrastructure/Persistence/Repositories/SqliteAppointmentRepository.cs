using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence.Repositories;

public class SqliteAppointmentRepository(AppDbContext dbContext) : IAppointmentRepository
{
    public async Task<IReadOnlyCollection<Appointment>> ListByBarberAsync(Guid barberId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Appointments
            .Where(appointment => appointment.BarberId == barberId)
            .OrderBy(appointment => appointment.ScheduledAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Appointment>> ListByClientAsync(Guid clientId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Appointments
            .Where(appointment => appointment.ClientId == clientId)
            .OrderBy(appointment => appointment.ScheduledAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Appointment?> GetByIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Appointments
            .FirstOrDefaultAsync(appointment => appointment.Id == appointmentId, cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        dbContext.Appointments.Add(appointment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}