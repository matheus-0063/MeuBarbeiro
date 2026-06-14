using MeuBarbeiro.Application.DTOs.Services;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Mappings.Services;

public static class ServiceOfferingMapping
{
    public static ServiceOffering ToEntity(this AddServicesRequestDto request) => new()
    {
        Id = Guid.NewGuid(),
        BarbershopId = request.BarbershopId,
        Name = request.Name,
        Price = request.Price,
        Description = request.Description,
        DurationMinutes = request.DurationMinutes
    };

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
