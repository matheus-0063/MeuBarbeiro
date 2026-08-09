using MeuBarbeiro.Application.Abstractions.Services;
using MeuBarbeiro.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MeuBarbeiro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IBarbershopService, BarbershopService>();
        services.AddScoped<IServicesService, ServicesService>();
        
        return services;
    }
}