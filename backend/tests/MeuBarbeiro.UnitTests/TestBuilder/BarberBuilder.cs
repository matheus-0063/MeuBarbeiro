using MeuBarbeiro.Domain.Entities;

namespace MeuBarbeiro.UnitTests.TestBuilder;

public class BarberBuilder
{
    private Guid _userId = Guid.NewGuid();

    public BarberBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public Barber Build()
    {
        var barber = new Barber(
            _userId
        );

        return barber;
    }
}