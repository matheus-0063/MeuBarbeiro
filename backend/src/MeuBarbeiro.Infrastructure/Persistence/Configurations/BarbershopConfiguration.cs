using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuBarbeiro.Infrastructure.Persistence.Configurations;

public class BarbershopConfiguration : IEntityTypeConfiguration<Barbershop>
{
    public void Configure(EntityTypeBuilder<Barbershop> builder)
    {
        builder.ToTable("Barbershops");

        builder.HasKey(barbershop => barbershop.Id);

        builder.Property(barbershop => barbershop.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(barbershop => barbershop.City)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(barbershop => barbershop.Address)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(barbershop => barbershop.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(barbershop => barbershop.AverageRating)
            .IsRequired();
    }
}
