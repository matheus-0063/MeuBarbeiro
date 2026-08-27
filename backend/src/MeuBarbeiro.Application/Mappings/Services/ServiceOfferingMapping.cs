using MeuBarbeiro.Application.DTOs.Services;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Mappings.Services;

public static class ServiceOfferingMapping
{
    public static ServiceOffering ToEntity(this CreateServicesRequestDto request, Guid barbershopId)
    {
        return new ServiceOffering(
            barbershopId,
            request.Name,
            price: request.Price,
            description: request.Description,
            durationMinutes: request.DurationMinutes
        );
    }


    public static ServiceResponseDto ToResponseDto(this ServiceOffering entity)
    {
        return new ServiceResponseDto
        {
            Id = entity.Id,
            BarbershopId = entity.BarbershopId,
            Name = entity.Name,
            Price = entity.Price,
            Description = entity.Description,
            DurationMinutes = entity.DurationMinutes
        };
    }
}