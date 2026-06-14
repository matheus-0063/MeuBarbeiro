namespace MeuBarbeiro.Domain.Entities;

public sealed class ServiceOffering
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BarbershopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}
