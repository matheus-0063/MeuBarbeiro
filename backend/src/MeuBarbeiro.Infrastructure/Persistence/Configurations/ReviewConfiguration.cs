using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuBarbeiro.Infrastructure.Persistence.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(review => review.Id);

        builder.Property(review => review.AppointmentId)
            .IsRequired();

        builder.Property(review => review.ClientId)
            .IsRequired();

        builder.Property(review => review.BarberId)
            .IsRequired();

        builder.Property(review => review.BarbershopId)
            .IsRequired();

        builder.Property(review => review.Stars)
            .IsRequired();

        builder.Property(review => review.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(review => review.AppointmentId)
            .IsUnique();
    }
}