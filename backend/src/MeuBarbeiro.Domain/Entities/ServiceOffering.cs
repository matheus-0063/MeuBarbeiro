using MeuBarbeiro.Domain.Exceptions;

namespace MeuBarbeiro.Domain.Entities;

public sealed class ServiceOffering
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BarbershopId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public int DurationMinutes { get; private set; }

    public ServiceOffering() {  }

    public ServiceOffering(Guid barbershopId, string name, string description, decimal price, int durationMinutes)
    {
        ValidarBarbershopId(barbershopId);
        ValidarNameEDescription(name, description);
        ValidarPreco(price);
        ValidarDurationMinutes(durationMinutes);
        
        Id = Guid.NewGuid();
        BarbershopId = barbershopId;
        Name = name;
        Description = description;
        Price = price;
        DurationMinutes = durationMinutes;
    }

    public void UpdateDetails(string name, string description, decimal price, int durationMinutes)
    {
        ValidarNameEDescription(name, description);
        ValidarPreco(price);
        ValidarDurationMinutes(durationMinutes);
        
        Name = name;
        Description = description;
        Price = price;
        DurationMinutes = durationMinutes;
    }

    private static void ValidarBarbershopId(Guid barbershopId)
    {
        if (barbershopId == Guid.Empty) 
            throw new ServiceOfferingValidationException(nameof(BarbershopId), "BarbershopId é obrigatório");
    }

    private static void ValidarNameEDescription(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            throw new ServiceOfferingValidationException(nameof(Name), "Nome do serviço é obrigatório");
        
        if (string.IsNullOrWhiteSpace(description)) 
            throw new ServiceOfferingValidationException(nameof(Description), "Descrição do serviço é obrigatório");
    }

    private static void ValidarPreco(decimal price)
    {
        if (price <= 0)
            throw new ServiceOfferingValidationException(nameof(Price), "Preço do serviço não pode ser negatico");
    }

    private static void ValidarDurationMinutes(int durationMinutes)
    {
        if (durationMinutes <= 0)
            throw new ServiceOfferingValidationException(nameof(DurationMinutes), "Duração do serviço precisa ser maior que 0");
    }
}
