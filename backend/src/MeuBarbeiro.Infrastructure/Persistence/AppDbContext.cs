using MeuBarbeiro.Domain.Entities;
using MeuBarbeiro.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
