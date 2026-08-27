namespace MeuBarbeiro.Application.DTOs.Services;

public class ServiceResponseDto
{
    public Guid Id { get; set; }
    public Guid BarbershopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}