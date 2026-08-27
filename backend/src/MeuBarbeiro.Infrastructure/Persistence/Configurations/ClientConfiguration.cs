using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuBarbeiro.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(client => client.Id);

        builder.Property(client => client.UserId)
            .IsRequired();

        builder.HasIndex(client => client.UserId)
            .IsUnique();
    }
}