using MeuBarbeiro.Application.DTOs.Barbers;
using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.Application.Mappings.Barbers;

public static class BarberMapping
{
    public static BarberResponseDto ToDto(this Barber entity, string name)
    {
        return new BarberResponseDto
        {
            Id = entity.Id,
            Name = name
        };
    }
}