using MeuBarbeiro.Application.DTOs.Barbershop;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Mappings.Barbershops;

public static class BarbershopMapping
{
    public static Barbershop ToEntity(this CreateBarbershopRequestDto request) => new Barbershop
    {
        Id = Guid.NewGuid(),
        Name = request.Name,
        City = request.City,
        Address = request.Address,
        Description = request.Description,
    };

    public static BarbershopResponseDto ToResponseDto(this Barbershop entity) => new BarbershopResponseDto
    {
        Id = entity.Id,
        Name = entity.Name,
        City = entity.City,
        Address = entity.Address,
        Description = entity.Description,
        AverageRating = entity.AverageRating
    };
}
