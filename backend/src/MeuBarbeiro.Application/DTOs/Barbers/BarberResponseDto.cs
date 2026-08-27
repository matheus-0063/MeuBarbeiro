namespace MeuBarbeiro.Application.DTOs.Barbers;

public sealed class BarberResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}