using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuBarbeiro.Infrastructure.Persistence.Configurations;

public class AppointmentServiceSelectionConfiguration : IEntityTypeConfiguration<AppointmentServiceSelection>
{
    public void Configure(EntityTypeBuilder<AppointmentServiceSelection> builder)
    {
        builder.ToTable("AppointmentServiceSelections");

        builder.HasKey(selection => selection.Id);

        builder.Property(selection => selection.AppointmentId)
            .IsRequired();

        builder.Property(selection => selection.ServiceOfferingId)
            .IsRequired();
    }
}