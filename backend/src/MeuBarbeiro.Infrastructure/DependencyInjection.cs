using MeuBarbeiro.Application.Abstractions.Messaging;
using MeuBarbeiro.Application.Abstractions.Persistence;
using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Infrastructure.Messaging;
using MeuBarbeiro.Infrastructure.Persistence;
using MeuBarbeiro.Infrastructure.Persistence.Repositories;
using MeuBarbeiro.Infrastructure.Security.Jwt;
using MeuBarbeiro.Infrastructure.Security.Password;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeuBarbeiro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddSingleton<DatabaseSchemaInitializer>();
        
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        
        services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
        services.AddSingleton<RabbitMqTopologyInitializer>();

        services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
        
        services.AddScoped<IAppointmentRepository, SqliteAppointmentRepository>();
        services.AddScoped<IAppointmentServiceSelectionRepository, SqliteAppointmentServiceSelectionRepository>();
        services.AddScoped<IBarberRepository, SqliteBarberRepository>();
        services.AddScoped<IBarbershopRepository, SqliteBarbershopRepository>();
        services.AddScoped<IClientRepository, SqliteClientRepository>();
        services.AddScoped<IReviewRepository, SqliteReviewRepository>();
        services.AddScoped<IServiceOfferingRepository, SqliteServiceOfferingRepository>();
        services.AddScoped<IUserRepository, SqliteUserRepository>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
