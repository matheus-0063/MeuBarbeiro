using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.UnitTests.TestBuilder;

public class BarbershopBuilder
{
    private string _name = string.Empty;
    private string _city = string.Empty;
    private string _address = string.Empty;
    private string _description = string.Empty;
    private double _averageRating = 0.0;

    public BarbershopBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public BarbershopBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    public BarbershopBuilder WithAddress(string address)
    {
        _address = address;
        return this;
    }

    public BarbershopBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public BarbershopBuilder WithAverageRating(double averageRating)
    {
        _averageRating = averageRating;
        return this;
    }

    public Barbershop Build()
    {
        var barbershop = new Barbershop(
            name: _name,
            city: _city,
            address: _address,
            description: _description
        );
        
        return barbershop;
    }
}