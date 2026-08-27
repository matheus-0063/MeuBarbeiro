using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuBarbeiro.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(appointment => appointment.Id);

        builder.Property(appointment => appointment.ClientId)
            .IsRequired();

        builder.Property(appointment => appointment.BarberId)
            .IsRequired();

        builder.Property(appointment => appointment.BarbershopId)
            .IsRequired();

        builder.Property(appointment => appointment.ScheduledAtUtc)
            .IsRequired();

        builder.Property(appointment => appointment.TotalPrice)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(appointment => appointment.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
    }
}