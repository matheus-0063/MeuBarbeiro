using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.UnitTests.TestBuilder;

public class BarberBuilder
{
    private Guid _userId = Guid.NewGuid();
    private Guid _barbershopId = Guid.NewGuid();

    public BarberBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public BarberBuilder WithBarbershopId(Guid barbershopId)
    {
        _barbershopId = barbershopId;
        return this;
    }

    public Barber Build()
    {
        var barber = new Barber(
            userId: _userId,
            barbershopId: _barbershopId
        );

        return barber;
    } 
} 