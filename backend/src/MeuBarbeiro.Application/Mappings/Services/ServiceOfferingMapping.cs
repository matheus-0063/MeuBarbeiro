using MeuBarbeiro.Application.DTOs.Services;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Mappings.Services;

public static class ServiceOfferingMapping
{
    public static ServiceOffering ToEntity(this CreateServicesRequestDto request, Guid barbershopId) =>
        new(
            barbershopId: barbershopId,
            name: request.Name,
            price: request.Price,
            description: request.Description,
            durationMinutes: request.DurationMinutes
        );


    public static ServiceResponseDto ToResponseDto(this ServiceOffering entity) => new()
    {
        Id = entity.Id,
        BarbershopId = entity.BarbershopId,
        Name = entity.Name,
        Price = entity.Price,
        Description = entity.Description,
        DurationMinutes = entity.DurationMinutes
    };
}