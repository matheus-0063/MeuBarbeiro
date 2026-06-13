using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Barbershop> Barbershops => Set<Barbershop>();
    public DbSet<EventProcessingAudit> EventProcessingAudits => Set<EventProcessingAudit>();
    public DbSet<ServiceOffering> ServiceOfferings => Set<ServiceOffering>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new BarbershopConfiguration());
        modelBuilder.ApplyConfiguration(new EventProcessingAuditConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceOfferingConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
