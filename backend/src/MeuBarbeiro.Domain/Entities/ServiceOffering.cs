namespace MeuBarbeiro.Domain.Entities;

public sealed class ServiceOffering
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BarbershopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }

    public ServiceOffering() {  }

    public ServiceOffering(Guid barbershopId, string name, string description, decimal price, int durationMinutes)
    {
        ValidarBarbershopId(barbershopId);
        
        Id = Guid.NewGuid();
        BarbershopId = barbershopId;
        Name = name;
        Description = description;
        Price = price;
        DurationMinutes = durationMinutes;
    }

    private static void ValidarBarbershopId(Guid barbershopId)
    {
        if (barbershopId == Guid.Empty) throw new ArgumentException("BarbershopId é obrigatório");
    }
}
