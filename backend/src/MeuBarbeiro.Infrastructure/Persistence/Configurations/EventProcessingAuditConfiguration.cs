using MeuBarbeiro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeuBarbeiro.Infrastructure.Persistence.Configurations;

public class EventProcessingAuditConfiguration : IEntityTypeConfiguration<EventProcessingAudit>
{
    public void Configure(EntityTypeBuilder<EventProcessingAudit> builder)
    {
        builder.ToTable("EventProcessingAudits");

        builder.HasKey(audit => audit.Id);

        builder.Property(audit => audit.EventName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(audit => audit.QueueName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(audit => audit.Payload)
            .IsRequired();

        builder.Property(audit => audit.ProcessedAtUtc)
            .IsRequired();

        builder.Property(audit => audit.Status)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(audit => audit.ErrorMessage)
            .HasMaxLength(500);
    }
}
