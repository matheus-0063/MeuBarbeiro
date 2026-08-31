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
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.")));

        services.AddSingleton<DatabaseSchemaInitializer>();

        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.AddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
        services.AddSingleton<RabbitMqTopologyInitializer>();

        services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAppointmentServiceSelectionRepository, AppointmentServiceSelectionRepository>();
        services.AddScoped<IBarberRepository, BarberRepository>();
        services.AddScoped<IBarbershopRepository, BarbershopRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IServiceOfferingRepository, ServiceOfferingRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}