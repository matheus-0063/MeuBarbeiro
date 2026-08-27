namespace MeuBarbeiro.Application.DTOs.Barbershop;

public class CreateBarbershopRequestDto
{
    public string Name { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}