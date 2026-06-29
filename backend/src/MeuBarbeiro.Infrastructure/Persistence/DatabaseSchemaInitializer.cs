using Microsoft.EntityFrameworkCore;

namespace MeuBarbeiro.Infrastructure.Persistence;

public sealed class DatabaseSchemaInitializer
{
    private const string CreateUsersTableSql = """
        CREATE TABLE IF NOT EXISTS "Users" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY,
            "Name" TEXT NOT NULL,
            "Email" TEXT NOT NULL,
            "PasswordHash" TEXT NOT NULL,
            "Role" INTEGER NOT NULL,
            "CreateAt" TEXT NOT NULL
        );
        """;
    private const string CreateClientsTableSql = """
        CREATE TABLE IF NOT EXISTS "Clients" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_Clients" PRIMARY KEY,
            "UserId" TEXT NOT NULL
        );
        """;
    private const string CreateBarbersTableSql = """
        CREATE TABLE IF NOT EXISTS "Barbers" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_Barbers" PRIMARY KEY,
            "UserId" TEXT NOT NULL,
            "BarbershopId" TEXT NULL
        );
        """;
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
    private const string CreateAppointmentServiceSelectionsTableSql = """
        CREATE TABLE IF NOT EXISTS "AppointmentServiceSelections" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_AppointmentServiceSelections" PRIMARY KEY,
            "AppointmentId" TEXT NOT NULL,
            "ServiceOfferingId" TEXT NOT NULL
        );
        """;
    private const string CreateReviewsTableSql = """
        CREATE TABLE IF NOT EXISTS "Reviews" (
            "Id" TEXT NOT NULL CONSTRAINT "PK_Reviews" PRIMARY KEY,
            "AppointmentId" TEXT NOT NULL,
            "ClientId" TEXT NOT NULL,
            "BarberId" TEXT NOT NULL,
            "BarbershopId" TEXT NOT NULL,
            "Stars" INTEGER NOT NULL,
            "CreatedAtUtc" TEXT NOT NULL
        );
        """;

    public async Task EnsureSchemaAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(CreateUsersTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync("""CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON "Users" ("Email");""", cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(CreateClientsTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync("""CREATE UNIQUE INDEX IF NOT EXISTS "IX_Clients_UserId" ON "Clients" ("UserId");""", cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(CreateBarbersTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync("""CREATE UNIQUE INDEX IF NOT EXISTS "IX_Barbers_UserId" ON "Barbers" ("UserId");""", cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(CreateBarbershopsTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(CreateEventProcessingAuditsTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(CreateServiceOfferingsTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(CreateAppointmentServiceSelectionsTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync(CreateReviewsTableSql, cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync("""CREATE UNIQUE INDEX IF NOT EXISTS "IX_Reviews_AppointmentId" ON "Reviews" ("AppointmentId");""", cancellationToken);
    }
}
