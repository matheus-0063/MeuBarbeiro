using MeuBarbeiro.Application.DTOs.Barbershop;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Mappings.Barbershops;

public static class BarbershopMapping
{
    public static Barbershop ToEntity(this CreateBarbershopRequestDto request, Guid ownerUserId)
    {
        return new Barbershop
        (
            ownerUserId,
            request.Name,
            request.City,
            request.Address,
            request.Description
        );
    }

    public static BarbershopResponseDto ToResponseDto(this Barbershop entity)
    {
        return new BarbershopResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            City = entity.City,
            Address = entity.Address,
            Description = entity.Description,
            AverageRating = entity.AverageRating
        };
    }
}