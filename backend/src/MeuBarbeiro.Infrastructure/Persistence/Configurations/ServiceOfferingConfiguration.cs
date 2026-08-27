using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuBarbeiro.Infrastructure.Persistence.Configurations;

public class ServiceOfferingConfiguration : IEntityTypeConfiguration<ServiceOffering>
{
    public void Configure(EntityTypeBuilder<ServiceOffering> builder)
    {
        builder.ToTable("ServiceOfferings");

        builder.HasKey(service => service.Id);

        builder.Property(service => service.BarbershopId)
            .IsRequired();

        builder.Property(service => service.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(service => service.Price)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(service => service.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(service => service.DurationMinutes)
            .IsRequired();
    }
}