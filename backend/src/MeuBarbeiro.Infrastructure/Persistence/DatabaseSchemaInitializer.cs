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
    private const string CreateBarbershopsTableSql = """
        CREATE TABLE IF NOT EXISTS "Barbershops" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_Barbershops" PRIMARY KEY,
            "Name" TEXT NOT NULL,
            "City" TEXT NOT NULL,
            "Address" TEXT NOT NULL,
            "Description" TEXT NOT NULL,
            "AverageRating" REAL NOT NULL
        );
        """;
    private const string CreateServiceOfferingsTableSql = """
        CREATE TABLE IF NOT EXISTS "ServiceOfferings" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_ServiceOfferings" PRIMARY KEY,
            "BarbershopId" TEXT NOT NULL,
            "Name" TEXT NOT NULL,
            "Price" TEXT NOT NULL,
            "Description" TEXT NOT NULL,
            "DurationMinutes" INTEGER NOT NULL
        );
        """;

    public async Task EnsureSchemaAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(CreateBarbershopsTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(CreateEventProcessingAuditsTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(CreateServiceOfferingsTableSql, cancellationToken);
    }
}
