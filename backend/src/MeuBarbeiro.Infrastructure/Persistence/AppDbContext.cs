using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<EventProcessingAudit> EventProcessingAudits => Set<EventProcessingAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new EventProcessingAuditConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
