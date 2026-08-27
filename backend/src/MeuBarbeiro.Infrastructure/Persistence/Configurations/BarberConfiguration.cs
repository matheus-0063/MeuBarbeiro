using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuBarbeiro.Infrastructure.Persistence.Configurations;

public class BarberConfiguration : IEntityTypeConfiguration<Barber>
{
    public void Configure(EntityTypeBuilder<Barber> builder)
    {
        builder.ToTable("Barbers");

        builder.HasKey(barber => barber.Id);

        builder.Property(barber => barber.UserId)
            .IsRequired();

        builder.Property(barber => barber.BarbershopId)
            .IsRequired(false);

        builder.HasIndex(barber => barber.UserId)
            .IsUnique();
    }
}