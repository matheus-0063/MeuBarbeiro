namespace MeuBarbeiro.Application.DTOs.Services;

public class CreateServicesRequestDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}