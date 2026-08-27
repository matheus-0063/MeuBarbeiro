using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.UnitTests.TestBuilder;

public class ServiceOfferingBuilder
{
    private Guid _barbershopId = Guid.NewGuid();
    private string _description = string.Empty;
    private int _durationMinutes = 1;
    private string _name = string.Empty;
    private decimal _price = decimal.Zero;

    public ServiceOfferingBuilder WithBarbershopId(Guid barbershopId)
    {
        _barbershopId = barbershopId;
        return this;
    }

    public ServiceOfferingBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ServiceOfferingBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public ServiceOfferingBuilder WithDescription(string description)
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
            _barbershopId,
            _name,
            price: _price,
            description: _description,
            durationMinutes: _durationMinutes
        );

        return serviceOffering;
    }
}