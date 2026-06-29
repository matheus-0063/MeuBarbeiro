using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentServiceSelection> AppointmentServiceSelections => Set<AppointmentServiceSelection>();
    public DbSet<Barber> Barbers => Set<Barber>();
    public DbSet<Barbershop> Barbershops => Set<Barbershop>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<EventProcessingAudit> EventProcessingAudits => Set<EventProcessingAudit>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ServiceOffering> ServiceOfferings => Set<ServiceOffering>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentServiceSelectionConfiguration());
        modelBuilder.ApplyConfiguration(new BarberConfiguration());
        modelBuilder.ApplyConfiguration(new BarbershopConfiguration());
        modelBuilder.ApplyConfiguration(new ClientConfiguration());
        modelBuilder.ApplyConfiguration(new EventProcessingAuditConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceOfferingConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
