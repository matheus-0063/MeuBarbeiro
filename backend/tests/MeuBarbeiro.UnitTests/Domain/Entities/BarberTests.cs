using FluentAssertions;
using MeuBarbeiro.UnitTests.TestBuilder;

namespace MeuBarbeiro.UnitTests.Domain.Entities;

public class BarberTests
{
    [Fact]
    public void Barber_DeveGerarId_QuandoForCriado()
    {
        // Act 
        var barber = new BarberBuilder()
            .Build();
        
        // Assert
        barber.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Barber_DeveFalhar_QuandoUserIdForVazio()
    {
        // Act
        var func = () => new BarberBuilder()
            .WithUserId(Guid.Empty)
            .Build();
        
        // Assert
        func.Should().Throw<ArgumentException>();
    }
}