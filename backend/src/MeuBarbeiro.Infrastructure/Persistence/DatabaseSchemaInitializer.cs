using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence;

public sealed class DatabaseSchemaInitializer
{
    private const string CreateEventProcessingAuditsTableSql = """
        CREATE TABLE IF NOT EXISTS "EventProcessingAudits" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_EventProcessingAudits" PRIMARY KEY,
            "EventName" TEXT NOT NULL,
            "QueueName" TEXT NOT NULL,
            "Payload" TEXT NOT NULL,
            "ProcessedAtUtc" TEXT NOT NULL,
            "Status" TEXT NOT NULL,
            "ErrorMessage" TEXT NULL
        );
        """;

    public async Task EnsureSchemaAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(CreateEventProcessingAuditsTableSql, cancellationToken);
    }
}
