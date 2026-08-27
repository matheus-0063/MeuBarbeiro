using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.UnitTests.TestBuilder;

public class BarbershopBuilder
{
    private string _address = string.Empty;
    private double _averageRating;
    private string _city = string.Empty;
    private string _description = string.Empty;
    private string _name = string.Empty;
    private Guid _ownerUserId;

    public BarbershopBuilder WithOwnerUserId(Guid ownerUserId)
    {
        _ownerUserId = ownerUserId;
        return this;
    }

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
            _ownerUserId,
            _name,
            _city,
            _address,
            _description
        );

        return barbershop;
    }
}