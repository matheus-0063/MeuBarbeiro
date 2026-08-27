using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.UnitTests.TestBuilder;

public class ServiceOfferingBuilder
{
    private Guid _barbershopId = Guid.NewGuid();
    private string _name = String.Empty;
    private decimal _price = Decimal.Zero;
    private string _description = String.Empty;
    private int _durationMinutes = 1;

    public ServiceOfferingBuilder WithBarbershopId(Guid barbershopId)
    {
        _barbershopId = barbershopId;
        return this;
    }

    public ServiceOfferingBuilder WithName(String name)
    {
        _name = name;
        return this;
    }

    public ServiceOfferingBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public ServiceOfferingBuilder WithDescription(String description)
    {
        _description = description;
        return this;
    }

    public ServiceOfferingBuilder WithDurationMinutes(int durationMinutes)
    {
        _durationMinutes = durationMinutes;
        return this;
    }

    public ServiceOffering Build()
    {
        var serviceOffering = new ServiceOffering(
            barbershopId: _barbershopId,
            name: _name,
            price: _price,
            description: _description,
            durationMinutes: _durationMinutes
        );
        
        return serviceOffering;
    }
}